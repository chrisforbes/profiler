using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ProfilerUi
{
	public partial class Form1 : Form
	{
		CallTreeView currentView = null;

		public Form1()
		{
			InitializeComponent(); 
			Text = "IJW Profiler 0.3";
			tabStrip.Height = 20;

			workspace.ContentPanel.BackColor = SystemColors.AppWorkspace;

			EventHandler e = delegate
			{
				if (currentView != null)
					workspace.ContentPanel.Controls.Remove(currentView);

				currentView = tabStrip.CurrentCallTree;

				if (currentView != null)
				{
					workspace.ContentPanel.Controls.Add(currentView);
					currentView.BorderStyle = BorderStyle.None;
					currentView.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
					currentView.Bounds = new Rectangle(1, 20, workspace.ContentPanel.ClientSize.Width - 2, 
						workspace.ContentPanel.ClientSize.Height - 21);
				}
			};

			tabStrip.Iterator.Changed += e;
			tabStrip.Changed += e;
		}

		Run ProfileProcess(string processName)
		{
			using (new ComServerRegistration("pcomimpl.dll"))
			{
				Run run = new Run();

				ProcessStartInfo info = new ProcessStartInfo(processName);
				info.WorkingDirectory = Path.GetDirectoryName(processName);
				info.UseShellExecute = false;
				info.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
				info.EnvironmentVariables["COR_PROFILER"] = "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}";

				info.EnvironmentVariables["ijwprof_txt"] = run.txtFile;
				info.EnvironmentVariables["ijwprof_bin"] = run.binFile;

				Process.Start(info).WaitForExit();

				return run;
			}
		}

		CallTreeView CreateNewView(string name, Node node)
		{
			CallTreeView view = new CallTreeView( GetFunctionFilter() );
			view.Text = name;
			tabStrip.Add(view);

			tabStrip.SelectCallTree(view);

			if (node != null)
			{
				if (node.Tag is Function)
					view.Text = node.TabName;
				//page.ImageKey = node.Key;
			}
			return view;
		}

		void NewRun(object sender, EventArgs e)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = "Application|*.exe";
			d.RestoreDirectory = true;

			if (DialogResult.OK != d.ShowDialog())
				return;

			Run run = ProfileProcess(d.FileName);

			LoadTraceData(run);
		}

		void LoadTraceData( Run run )
		{
			using (run)
			{
				FunctionNameProvider names = new FunctionNameProvider(run.txtFile);

				string baseText = Text;

				Action<float> progressCallback = delegate(float frac)
				{
					Text = baseText + " - Slurping " + frac.ToString("P0");
				};

				CallTree tree = new CallTree(run.binFile, names, progressCallback);
				CallTreeView view = CreateNewView(run.name, null);

				Text = baseText + " - Preparing view...";

				view.Nodes.Clear();
				foreach (Thread thread in tree.threads.Values)
					view.Nodes.Add(thread.CreateView(thread.TotalTime));

				Text = baseText;
			}
		}

		Predicate<Function> GetFunctionFilter()
		{
			return new FunctionFilter("System.", "Microsoft.").Evaluate;
		}

		void OnCloseClicked(object sender, EventArgs e) { Close(); }

		Node GetSelectedNode()
		{
			if (tabStrip.CurrentCallTree == null)
				return null;

			return tabStrip.CurrentCallTree.SelectedNode as Node;
		}

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			Node n = GetSelectedNode();
			if (n == null)
				return;

			IProfilerElement t = n.Element;

			CallTreeView v = CreateNewView(t.TabTitle, n);
			TreeNode n2 = t.CreateView(t.TotalTime);
			v.Nodes.Add(n2);
			v.SelectedNode = n2;
		}
	}
}
