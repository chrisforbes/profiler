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

namespace ProfilerUi
{
	public partial class Form1 : Form
	{
		string version = "0.7.2";

		MultipleViewManager viewManager;
		ViewBase startPage; 
		ImageProvider imageProvider = new ImageProvider("res/");
		ColumnCollection callTreeColumns = new ColumnCollection();
		ColumnCollection callerColumns = new ColumnCollection();

		public Form1()
		{
			Columns cc = new Columns(Font, new Font(Font, FontStyle.Bold), imageProvider);

			callTreeColumns.CreateAutoWidth("Function", cc.RenderFunctionColumn);
			callTreeColumns.CreateFixedWidth("Calls", 50, cc.RenderCallsColumn);
			callTreeColumns.CreateFixedWidth("Time %", 50, cc.RenderPercentColumn);
			callTreeColumns.CreateFixedWidth("Total Time", 70, cc.RenderTotalTimeColumn);
			callTreeColumns.CreateFixedWidth("Own Time", 70, cc.RenderOwnTimeColumn);
			callTreeColumns.CreateFixedWidth("", 16, delegate { });

			callerColumns.CreateAutoWidth("Function", cc.RenderCallerColumn);
			callerColumns.CreateFixedWidth("Calls", 50, cc.RenderCallerCallsColumn);
			callerColumns.CreateFixedWidth("Time", 70, cc.RenderCallerTimeColumn);
			callerColumns.CreateFixedWidth("", 16, delegate { });

			InitializeComponent();

			Text = "IJW Profiler " + version;
			viewManager = new MultipleViewManager(workspace.ContentPanel);

			startPage = new WebView(viewManager,
				"file://" + Path.GetFullPath("mru.xml"), new StartPageController(version, NewRun, delegate { CheckForUpdates(this, EventArgs.Empty); }));

			viewManager.Add(startPage);

			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);
		}

		Run ProfileProcess(RunParameters p)
		{
			MruList.AddRun(p);

			using (new ComServerRegistration("pcomimpl.dll"))
			{
				Run run = new Run();

				ProcessStartInfo info = new ProcessStartInfo(p.exePath);
				info.WorkingDirectory = p.workingDirectory;
				info.UseShellExecute = false;
				info.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
				info.EnvironmentVariables["COR_PROFILER"] = "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}";

				info.EnvironmentVariables["ijwprof_txt"] = run.txtFile;
				info.EnvironmentVariables["ijwprof_bin"] = run.binFile;

				Process.Start(info).WaitForExit();
				return run;
			}
		}

		TreeControl CreateNewView(string name, Node node, CallTree src, ColumnCollection cc)
		{
			CallTreeView view = new CallTreeView( imageProvider, cc, src, name );
			ProfilerView viewWrapper = ProfilerView.Create(viewManager, view, new TreeColumnHeader(cc));
			viewManager.Add(viewWrapper);
			viewManager.Select(viewWrapper);

			return view;
		}

		void NewRun(RunParameters r)
		{
			NewRunDialog dialog = new NewRunDialog(r);

			if (DialogResult.OK == dialog.ShowDialog())
				using (Run run = ProfileProcess(dialog.Parameters))
					LoadTraceData(run);
		}

		void NewRun(object sender, EventArgs e) { NewRun(null); }

		void LoadTraceData(Run run)
		{
			FunctionNameProvider names = new FunctionNameProvider(run.txtFile);
			string baseText = Text;
			Action<float> progressCallback =
				delegate(float frac) { Text = baseText + " - Slurping " + frac.ToString("P0"); Application.DoEvents(); };

			CallTree tree = new CallTree(run.binFile, names, progressCallback, Filter);
			TreeControl view = CreateNewView(run.name, null, tree, callTreeColumns);

			Text = baseText + " - Preparing view...";

			foreach (Thread thread in tree.threads.Values)
			{
				Node n = thread.CreateView(thread.TotalTime);
				
				view.Root.Add(n);
				n.Expand();
			}

			Text = baseText;
		}

		Predicate<string> Filter = new FunctionFilter("System.", "Microsoft.", "MS.Internal.").EvalString;

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
			TreeControl v = CreateNewView(t.TabTitle, n2, src, callTreeColumns);
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
			CallTreeNode selectedNode = GetSelectedNode();
			if (selectedNode == null)
				return;

			CallTreeNode rootNode = selectedNode.RootFunction;
			IProfilerElement selected = (rootNode != null) 
				? OfferToMerge(rootNode.Value, selectedNode.Value as Function)
				: selectedNode.Value;

			OpenInNewTab(CurrentView.src, selectedNode.Value);
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

		void CheckForUpdates(object sender, EventArgs e)
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
			TreeControl tc = CreateNewView("Callers of " + cf.Name.MethodName, root,
				CurrentView.src, callerColumns);
			tc.Root.Add(root);
			tc.SelectedNode = root;
		}
	}
}
