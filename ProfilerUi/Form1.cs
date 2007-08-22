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

namespace ProfilerUi
{
	public partial class Form1 : Form
	{
		MultipleViewManager viewManager;
		ViewBase startPage; 
		ImageProvider imageProvider = new ImageProvider("res/");
		ColumnCollection callTreeColumns = new ColumnCollection();
		TreeColumnHeader header;

		Font boldFont;

		public Form1()
		{
			string version = "0.7";

			callTreeColumns.CreateAutoWidth("Function", RenderFunctionColumn);
			callTreeColumns.CreateFixedWidth("Calls", 50, RenderCallsColumn);
			callTreeColumns.CreateFixedWidth("Time %", 50, RenderPercentColumn);
			callTreeColumns.CreateFixedWidth("Total Time", 70, RenderTotalTimeColumn);
			callTreeColumns.CreateFixedWidth("Own Time", 70, RenderOwnTimeColumn);
			callTreeColumns.CreateFixedWidth("", 16, delegate { });

			InitializeComponent();

			boldFont = new Font(Font, FontStyle.Bold);

			Text = "IJW Profiler " + version;
			viewManager = new MultipleViewManager(workspace.ContentPanel);

			startPage = new WebView(viewManager,
				"file://" + Path.GetFullPath("mru.xml"), new StartPageController(version, NewRun));

			viewManager.Add(startPage);
			header = new TreeColumnHeader(callTreeColumns);

			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			callTreeColumns.WidthUpdatedHandler(ClientSize.Width);
		}

		Brush GetBrush(IProfilerElement e)
		{
			Function f = e as Function;
			if (f == null || f.Interesting)
				return SystemBrushes.WindowText;

			return Brushes.Gray;
		}

		string GetImage(MethodType t)
		{
			switch (t)
			{
				case MethodType.Method: return "method";
				case MethodType.PropertyGet: return "prop_get";
				case MethodType.PropertySet: return "prop_set";
				case MethodType.EventAdd: return "event_add";
				case MethodType.EventRemove: return "event_remove";
				case MethodType.Constructor: return "ctor";
				default:
					return "method";
			}
		}

		void RenderFunctionColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			Brush brush = GetBrush(e);

			Function f = e as Function;
			if (f != null)
			{
				Image i = imageProvider.GetImage(GetImage(f.name.Type));
				p.DrawImage(i);
				p.Pad(2);
				p.DrawString(f.name.ClassName, Font, brush, 1, c.Left + c.Width);
				p.DrawString((f.name.Type == MethodType.Constructor ? "  " : " .") + f.name.MethodName, boldFont, brush, 1, c.Left + c.Width);
				return;
			}

			Thread t = e as Thread;
			if (t != null)
			{
				Image i = imageProvider.GetImage("thread");
				p.DrawImage(i);
				p.Pad(2);
				p.DrawString("Thread #" + t.Id + (t.IsGcThread ? " (Garbage Collector)" : ""), Font, brush, 1, c.Left + c.Width);
			}
		}

		void RenderPercentColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			Function e = nn.Value as Function;

			if (e == null)
				return;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;

			double frac = e.TotalTime / nn.rootTime;

			p.DrawString(frac.ToString("P1"), Font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		void RenderTotalTimeColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;
			p.DrawString(e.TotalTime.ToString("F1") + " ms", Font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		void RenderOwnTimeColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			Function e = nn.Value as Function;
			if (e == null)
				return;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;
			p.DrawString(e.OwnTime.ToString("F1") + " ms", Font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		void RenderCallsColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			Function f = e as Function;
			if (f != null)
			{
				p.SetPosition(c.Left + c.Width - 4);
				p.Alignment = StringAlignment.Far;
				p.DrawString(f.Calls.ToString(), Font, GetBrush(e), 1, c.Left + c.Width);
				p.Alignment = StringAlignment.Near;
			}
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

		TreeControl CreateNewView(string name, CallTreeNode node, CallTree src)
		{
			CallTreeView view = new CallTreeView( imageProvider, callTreeColumns, src, name );
			ProfilerView viewWrapper = ProfilerView.Create(viewManager, view, header);
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
			TreeControl view = CreateNewView(run.name, null, tree);

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

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			CallTreeNode selectedNode = GetSelectedNode();
			if (selectedNode == null)
				return;

			CallTreeNode rootNode = selectedNode.RootFunction;

			if (rootNode != null)
			{
				IProfilerElement rootFunction = rootNode.Value;
				Function selectedFunction = selectedNode.Value as Function;

				if (rootFunction != null && selectedFunction != null)
				{
					List<Function> invocations = rootFunction.CollectInvocations(selectedFunction.id);

					if (invocations.Count > 1)
					{
						if (DialogResult.Yes == MessageBox.Show(
							"There are multiple invocations of this function in the call tree. Would you like to merge them?",
							"Merge multiple instances?",
							MessageBoxButtons.YesNo))
						{
							Function merged = Function.Merge(invocations);
							selectedNode = (CallTreeNode)merged.CreateView(merged.TotalTime);
						}
					}
				}
			}

			IProfilerElement t = selectedNode.Value;

			TreeControl v = CreateNewView(t.TabTitle, selectedNode, CurrentView.src);
			Node n2 = t.CreateView(t.TotalTime);
			v.Root.Add(n2);
			v.SelectedNode = n2;
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
	}

	class CallTreeView : TreeControl
	{
		public readonly CallTree src;

		public CallTreeView(ImageProvider provider, ColumnCollection collection, CallTree src, string title)
			: base(provider, collection)
		{
			this.src = src;
			this.title = title;
		}

		string title;

		public override string ToString()
		{
			return title;
		}
	}

	class CallTreeNode : Node
	{
		public readonly IProfilerElement Value;

		public double rootTime;

		public CallTreeNode(IProfilerElement value, double rootTime)
			: base()
		{
			Value = value;
			this.rootTime = rootTime;
		}

		public string TabName { get { return Value.TabTitle; } }

		public CallTreeNode RootFunction
		{
			get
			{
				CallTreeNode n = this;
				if (!(n.Value is Function))
					return null;

				while ((n.parent is CallTreeNode) && (n.parent as CallTreeNode).Value is Function)
					n = n.parent as CallTreeNode;

				return n;
			}
		}
	}
}
