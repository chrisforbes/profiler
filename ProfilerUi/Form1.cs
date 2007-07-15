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
		public Form1() { InitializeComponent(); Text = "IJW Profiler 0.2.7"; }

		string profilerTextOutput, profilerBinOutput;

		void ProfileProcess(string processName)
		{
			using (new ComServerRegistration("pcomimpl.dll"))
			{
				ProcessStartInfo info = new ProcessStartInfo(processName);
				info.WorkingDirectory = Path.GetDirectoryName(processName);
				info.UseShellExecute = false;
				info.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
				info.EnvironmentVariables["COR_PROFILER"] = "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}";

				profilerTextOutput = Path.GetTempFileName();
				profilerBinOutput = Path.GetTempFileName();

				info.EnvironmentVariables["ijwprof_txt"] = profilerTextOutput;
				info.EnvironmentVariables["ijwprof_bin"] = profilerBinOutput;

				Process.Start(info).WaitForExit();
			}
		}

		CallTreeView CreateNewView(string name)
		{
			TabPage page = new TabPage(name);
			CallTreeView view = new CallTreeView();
			page.Controls.Add(view);
			page.Tag = view;
			view.Dock = DockStyle.Fill;

			tabControl1.Controls.Add(page);
			tabControl1.SelectedTab = page;
			return view;
		}

		void NewRun(object sender, EventArgs e)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = "Application|*.exe";
			d.RestoreDirectory = true;

			if (DialogResult.OK != d.ShowDialog())
				return;

			ProfileProcess(d.FileName);

			LoadTraceData();
		}

		int runCount = 0;

		void LoadTraceData()
		{
			FunctionNameProvider names = new FunctionNameProvider(profilerTextOutput);

			string baseText = Text;

			Action<float> progressCallback = delegate(float frac)
			{
				Text = baseText + " - Slurping " + frac.ToString("P0");
				Application.DoEvents();
			};

			CallTree tree = new CallTree(profilerBinOutput, names, progressCallback);

			CallTreeView view = CreateNewView("Profile #" + ++runCount);

			Text = baseText + " - Preparing view...";
			Application.DoEvents();

			view.Nodes.Clear();
			foreach (Thread thread in tree.threads.Values)
				view.Nodes.Add(thread.CreateView());

			view.Filter = GetFunctionFilter();

			Text = baseText;
		}

		Predicate<Function> GetFunctionFilter()
		{
			string filterText = "System.*, Microsoft.*";
			FunctionFilter filter = new FunctionFilter(filterText.Replace("*", "").Split(new char[] { ' ', ',' },
				StringSplitOptions.RemoveEmptyEntries));

			return filter.Evaluate;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			Close();
		}

		TreeNode GetSelectedNode()
		{
			if (tabControl1.SelectedTab == null)
				return null;

			CallTreeView view = tabControl1.SelectedTab.Tag as CallTreeView;
			if (view == null)
				return null;

			return view.SelectedNode;
		}

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			TreeNode n = GetSelectedNode();
			if (n == null)
				return;

			Thread t = n.Tag as Thread;
			if (t != null)
			{
				CallTreeView v = CreateNewView("Thread #" + t.Id);
				TreeNode n2 = t.CreateView();
				v.Nodes.Add(n2);
				v.Filter = GetFunctionFilter();
				v.Focus();
				v.SelectedNode = n2;
				return;
			}

			Function f = n.Tag as Function;
			if (f != null)
			{
				CallTreeView v = CreateNewView(f.name.Substring(f.name.IndexOf("::") + 2));
				TreeNode n2 = f.CreateView(f.TotalTime);
				v.Nodes.Add(n2);	//todo: offer to merge
				v.Filter = GetFunctionFilter();
				v.Focus();
				v.SelectedNode = n2;
				return;
			}
		}

		void OnClose(object sender, FormClosedEventArgs e)
		{
			if (profilerTextOutput != null)
				File.Delete(profilerTextOutput);
			if (profilerBinOutput != null)
				File.Delete(profilerBinOutput);
		}
	}
}