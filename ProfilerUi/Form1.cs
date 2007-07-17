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
			Text = "IJW Profiler 0.3.1";
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
					currentView.Focus();
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
			CallTreeView view = new CallTreeView( Filter );
			view.Text = name;
			tabStrip.Add(view);

			tabStrip.SelectCallTree(view);

			if (node != null)
			{
				if (node.Tag is Function)
					view.Text = node.TabName;
			}
			return view;
		}

		void NewRun(object sender, EventArgs e)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = "Application|*.exe";
			d.RestoreDirectory = true;

			if (DialogResult.OK == d.ShowDialog())
				using (Run run = ProfileProcess(d.FileName))
					LoadTraceData(run);
		}

		void LoadTraceData(Run run)
		{
			FunctionNameProvider names = new FunctionNameProvider(run.txtFile);
			string baseText = Text;
			Action<float> progressCallback = delegate(float frac) { Text = baseText + " - Slurping " + frac.ToString("P0"); };

			Predicate<string> shouldHideFunction = delegate { return false; };

			CallTree tree = new CallTree(run.binFile, names, shouldHideFunction, progressCallback);
			CallTreeView view = CreateNewView(run.name, null);

			Text = baseText + " - Preparing view...";

			view.Nodes.Clear();
			foreach (Thread thread in tree.threads.Values)
				view.Nodes.Add(thread.CreateView(thread.TotalTime));

			Text = baseText;
		}

		Predicate<string> Filter = new FunctionFilter("System.", "Microsoft.").EvalString;

		void OnCloseClicked(object sender, EventArgs e) { Close(); }

		Node GetSelectedNode()
		{
			if (tabStrip.CurrentCallTree == null)
				return null;

			return tabStrip.CurrentCallTree.SelectedNode as Node;
		}

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			Node selectedNode = GetSelectedNode();
			if (selectedNode == null)
				return;

			Node rootNode = selectedNode.RootFunction;
			
			if (rootNode != null)
			{
				IProfilerElement rootFunction = rootNode.Element;
				Function selectedFunction = selectedNode.Element as Function;

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
							selectedNode = (Node)merged.CreateView(merged.TotalTime);
						}
					}
				}
			}

			IProfilerElement t = selectedNode.Element;

			CallTreeView v = CreateNewView(t.TabTitle, selectedNode);
			TreeNode n2 = t.CreateView(t.TotalTime);
			v.Nodes.Add(n2);
			v.SelectedNode = n2;
		}
	}
}
