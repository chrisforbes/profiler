using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using IjwFramework.TabStrip;
using IjwFramework.Delegates;

namespace ProfilerUi
{
	public partial class Form1 : Form
	{
		MultipleViewManager viewManager;

		public Form1()
		{
			InitializeComponent(); 
			Text = "IJW Profiler 0.4";
			viewManager = new MultipleViewManager(workspace.ContentPanel);
		}

		Run ProfileProcess(RunParameters p)
		{
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

		CallTreeView CreateNewView(string name, Node node, CallTree src)
		{
			CallTreeView view = new CallTreeView( Filter, name, src );
			ProfilerView viewWrapper = new ProfilerView(viewManager, view);
			viewManager.Add(viewWrapper);
			viewManager.Select(viewWrapper);

			if (node != null)
			{
				if (node.Tag is Function)
					view.Text = node.TabName;
			}
			return view;
		}

		void NewRun(object sender, EventArgs e)
		{
			NewRunDialog dialog = new NewRunDialog(null);

			if (DialogResult.OK == dialog.ShowDialog())
				using (Run run = ProfileProcess(dialog.Parameters))
					LoadTraceData(run);
		}

		void LoadTraceData(Run run)
		{
			FunctionNameProvider names = new FunctionNameProvider(run.txtFile);
			string baseText = Text;
			Action<float> progressCallback =
				delegate(float frac) { Text = baseText + " - Slurping " + frac.ToString("P0"); Application.DoEvents(); };

			CallTree tree = new CallTree(run.binFile, names, progressCallback);
			CallTreeView view = CreateNewView(run.name, null, tree);

			Text = baseText + " - Preparing view...";
			Application.DoEvents();

			view.Nodes.Clear();
			foreach (Thread thread in tree.threads.Values)
				view.Nodes.Add(thread.CreateView(thread.TotalTime, Filter));

			Text = baseText;
		}

		Predicate<string> Filter = new FunctionFilter("System.", "Microsoft.", "MS.Internal.").EvalString;

		void OnCloseClicked(object sender, EventArgs e) { Close(); }

		Node GetSelectedNode()
		{
			ProfilerView current = viewManager.Current as ProfilerView;
			if (current == null)
				return null;

			return current.view.SelectedNode as Node;
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
							selectedNode = (Node)merged.CreateView(merged.TotalTime, Filter);
						}
					}
				}
			}

			IProfilerElement t = selectedNode.Element;

			CallTreeView v = CreateNewView(t.TabTitle, selectedNode, CurrentView.src);
			TreeNode n2 = t.CreateView(t.TotalTime, Filter);
			v.Nodes.Add(n2);
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
}
