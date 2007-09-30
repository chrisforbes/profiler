using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using IjwFramework.Ui;
using IjwFramework.Delegates;
using IjwFramework.Updates;
using IjwFramework.Types;
using Ijw.Profiler.Core;
using Ijw.Profiler.Model;

namespace Ijw.Profiler.UI
{
	public partial class Form1 : Form
	{
		string version = "0.9.4";

		MultipleViewManager viewManager;
		WebView startPage; 
		ImageProvider imageProvider = new ImageProvider(Application.StartupPath + "/res/");
		ColumnCollection callTreeColumns = new ColumnCollection();
		ColumnCollection callerColumns = new ColumnCollection();
		AgentLoader loader;

		static string[] StripOptionFromArgs(string[] args, out string defaultAgent)
		{
			defaultAgent = "clr";
			if (args.Length == 0)
				return args;

			if (args[0].StartsWith("-agent:"))
			{
				defaultAgent = args[0].Substring(7);
				return Util.Rest(args);
			}
			else
				return args;
		}

		public Form1( string[] args )
		{
			string defaultAgent;
			args = StripOptionFromArgs(args, out defaultAgent);
			loader = new AgentLoader(defaultAgent);

			Columns cc = new Columns(Font, new Font(Font, FontStyle.Bold), imageProvider);

			callTreeColumns.CreateAutoWidth("Function", 
				Columns.BindSubtype<IProfilerElement>(cc.RenderFunctionColumn));
			callTreeColumns.CreateFixedWidth("Calls", 50, 
				Columns.BindSubtype<IProfilerElement>(cc.RenderCallsColumn));
			callTreeColumns.CreateFixedWidth("Time %", 50, 
				Columns.BindSubtype<IProfilerElement>(cc.RenderPercentColumn));
			callTreeColumns.CreateFixedWidth("Total Time", 70, 
				Columns.BindSubtype<IProfilerElement>(cc.RenderTotalTimeColumn));
			callTreeColumns.CreateFixedWidth("Own Time", 70, 
				Columns.BindSubtype<IProfilerElement>(cc.RenderOwnTimeColumn));
			callTreeColumns.CreateFixedWidth("", 16, delegate { });

			callerColumns.CreateAutoWidth("Function", 
				Columns.BindSubtype<CallerFunction>(cc.RenderCallerColumn));
			callerColumns.CreateFixedWidth("Calls", 50, 
				Columns.BindSubtype<CallerFunction>(cc.RenderCallerCallsColumn));
			callerColumns.CreateFixedWidth("Own Time", 70, 
				Columns.BindSubtype<CallerFunction>(cc.RenderCallerOwnTimeColumn));
			callerColumns.CreateFixedWidth("Total Time", 70, 
				Columns.BindSubtype<CallerFunction>(cc.RenderCallerTotalTimeColumn));
			callerColumns.CreateFixedWidth("", 16, delegate { });
			
			InitializeComponent();

			Text = "IJW Profiler " + version;
			viewManager = new MultipleViewManager(workspace.ContentPanel);

			startPage = new WebView(viewManager,
				"file://" + Application.StartupPath + "/mru.xml", 
				new StartPageController(version, NewRun, CheckForUpdates, loader));

			viewManager.Add(startPage);

			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);

			if (args.Length > 0)
				NewRun(new RunParameters(args));
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);
		}

		Run ProfileProcess(RunParameters p, AgentLoader loader)
		{
			MruList.AddRun(p, loader);
			startPage.Refresh();

			return p.agent.Execute(p);
		}

		TreeControl CreateNewView(string name, CallTree src, ColumnCollection cc)
		{
			CallTreeView view = new CallTreeView( imageProvider, cc, src, name );
			ProfilerView viewWrapper = ProfilerView.Create(viewManager, view, new TreeColumnHeader(cc), MakeLegendBar());
			viewManager.Add(viewWrapper);
			viewManager.Select(viewWrapper);

			return view;
		}

		void NewRun(RunParameters r)
		{
			NewRunDialog dialog = new NewRunDialog(r, loader);

			if (DialogResult.OK == dialog.ShowDialog())
				using (Run run = ProfileProcess(dialog.Parameters, loader))
					LoadTraceData(run, r.agent);
		}

		void NewRun(object sender, EventArgs e) { NewRun(null); }

		void LoadTraceData(Run run, IAgent agent)
		{
			FunctionNameProvider names = new FunctionNameProvider(run.txtFile, agent.NameFactory);

			string baseText = Text;
			Action<float> progressCallback =
				delegate(float frac) { Text = baseText + " - Slurping " + frac.ToString("P0"); Application.DoEvents(); };

			CallTree tree = new CallTree(run.binFile, names.GetName, progressCallback);
			TreeControl view = CreateNewView(run.name, tree, callTreeColumns);

			Text = baseText + " - Preparing view...";

			foreach (Thread thread in tree.threads.Values)
			{
				Node n = thread.CreateView(thread.TotalTime);
				
				view.Root.Add(n);
				n.Expand();
			}

			Text = baseText;
		}

		void OnCloseClicked(object sender, EventArgs e) { Close(); }

		CallTreeNode GetSelectedNode()
		{
			ProfilerView current = viewManager.Current as ProfilerView;
			if (current == null)
				return null;

			return current.view.SelectedNode as CallTreeNode;
		}

		void OpenInNewTab(CallTree src, IProfilerElement root)
		{
			IProfilerElement t = root;
			Node n2 = t.CreateView(t.TotalTime);
			TreeControl v = CreateNewView(t.TabTitle, src, callTreeColumns);
			v.Root.Add(n2);
			v.SelectedNode = n2;
		}

		IProfilerElement OfferToMerge(IProfilerElement root, Function selected)
		{
			if (root == null || selected == null)
				return selected;

			List<Function> invocations = root.CollectInvocations(selected.Id);

			if (invocations.Count == 1)
				return selected;

			if (DialogResult.Yes == MessageBox.Show(
				"There are multiple invocations of this function in the call tree. Would you like to merge them?",
				"Merge multiple invocations?",
				MessageBoxButtons.YesNo))
				return Function.Merge(invocations);
			else
				return selected;
		}

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			Node<IProfilerElement> selectedNode = GetSelectedNode();
			if (selectedNode == null)
				return;

			Node<IProfilerElement> rootNode = RootFunction(selectedNode);
			IProfilerElement selected = (rootNode != null) 
				? OfferToMerge(rootNode.Value, selectedNode.Value as Function)
				: selectedNode.Value;

			OpenInNewTab(CurrentView.src, selected);
		}

		static Node<IProfilerElement> RootFunction(Node<IProfilerElement> p)
		{
			Node<IProfilerElement> n = p;
			if (!(n.Value is Function))
				return null;

			while ((n.parent is Node<IProfilerElement>) && (n.parent as Node<IProfilerElement>).Value is Function)
				n = n.parent as Node<IProfilerElement>;

			return n;
		}

		CallTreeView CurrentView
		{
			get
			{
				ProfilerView v = viewManager.Current as ProfilerView;
				return (v == null) ? null : v.view;
			}
		}

		void OnSave(object sender, EventArgs e)
		{
			if (CurrentView == null)
				return;

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.RestoreDirectory = true;
			sfd.Filter = "Snapshot files (*.xml)|*.xml";

			if (DialogResult.OK == sfd.ShowDialog())
				CurrentView.src.WriteTo(sfd.FileName);
		}

		void GoToNextTab(object sender, EventArgs e) { viewManager.MoveNext(); }
		void GoToPreviousTab(object sender, EventArgs e) { viewManager.MovePrevious(); }
		void CloseTab(object sender, EventArgs e) { viewManager.CloseCurrent(); }

		void CheckForUpdates(object sender, EventArgs e) { CheckForUpdates(); }

		void CheckForUpdates()
		{
			UpdateManager.CheckForUpdates("IJW Profiler", version);
		}

		void About(object sender, EventArgs e)
		{
			new AboutBox("IJW Profiler", version, "(c)2007 IJW Software (NZ)").ShowDialog();
		}

		void ShowBacktraces(object sender, EventArgs e)
		{
			CallTreeNode selected = GetSelectedNode();
			if (selected == null)
				return;

			Function f = selected.Value as Function;

			if (f == null)
			{
				MessageBox.Show("Cannot show backtrace for a non-function object");
				return;
			}

			CallerFunction cf = CurrentView.src.GetBacktrace(f);

			Node root = cf.CreateView();
			root.Expand();
			TreeControl tc = CreateNewView("Callers of " + cf.Name.MethodName, CurrentView.src, callerColumns);
			tc.Root.Add(root);
			tc.SelectedNode = root;
		}

		void ShowHotspots(object sender, EventArgs e)
		{
			if (CurrentView == null) return;
			List<CallerFunction> callerFunctions = CurrentView.src.GetHotspots(10);
			
			TreeControl tc = CreateNewView("Hotspots of " + CurrentView.ToString(), CurrentView.src, callerColumns);

			foreach (CallerFunction cf in callerFunctions)
				tc.Root.Add(cf.CreateView());
		}

		LegendBar MakeLegendBar()
		{
			LegendBar bar = new LegendBar(imageProvider);

			List<Pair<string, string>> pageOne = new List<Pair<string, string>>();
			pageOne.Add(new Pair<string, string>("call_in", "Time is spent in child functions"));
			pageOne.Add(new Pair<string, string>("call_in_self", "Time is spent in this function"));
			pageOne.Add(new Pair<string, string>("call_in_mixed", "Time is spent in this function and children"));

			bar.Add(pageOne);

			List<Pair<string, string>> pageTwo = new List<Pair<string, string>>();
			pageTwo.Add(new Pair<string, string>("method", "Method"));
			pageTwo.Add(new Pair<string, string>("ctor", "Constructor"));
			pageTwo.Add(new Pair<string, string>("prop_get", "Property Get"));
			pageTwo.Add(new Pair<string, string>("prop_set", "Property Set"));
			pageTwo.Add(new Pair<string, string>("event_add", "Event Subscribe"));
			pageTwo.Add(new Pair<string, string>("event_remove", "Event Unsubscribe"));

			bar.Add(pageTwo);

			return bar;
		}
	}
}
