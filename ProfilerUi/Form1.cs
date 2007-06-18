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
		public Form1() { InitializeComponent(); }

		void ProfileProcess(string processName)
		{
			using (new ComServerRegistration("pcomimpl.dll"))
			{
				ProcessStartInfo info = new ProcessStartInfo(processName);
				info.WorkingDirectory = Path.GetDirectoryName(processName);
				info.UseShellExecute = false;
				info.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
				info.EnvironmentVariables["COR_PROFILER"] = "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}";

				Process.Start(info).WaitForExit();
			}
		}

		CallTreeView CreateNewView(string name)
		{
			TabPage page = new TabPage(name);
			CallTreeView view = new CallTreeView();
			page.Controls.Add(view);
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

			LoadLastRun(sender, e);
		}

		int runCount = 0;

		void LoadLastRun(object sender, EventArgs e)
		{
			FunctionNameProvider names = new FunctionNameProvider("c:\\profile.txt");

			CallTree tree = new CallTree("c:\\profile.bin", names);

			CallTreeView view = CreateNewView("Profile #" + ++runCount);

			view.Nodes.Clear();
			List<TreeNode> nodes = new List<TreeNode>();
			foreach (Thread thread in tree.threads.Values)
				nodes.Add(thread.CreateView());
			view.Nodes.AddRange(nodes.ToArray());

			string filterText = "System.*, Microsoft.*";
			FunctionFilter filter = new FunctionFilter(filterText.Replace("*", "").Split(new char[] { ' ', ',' },
				StringSplitOptions.RemoveEmptyEntries));

			view.Filter = filter.Evaluate;
		}

		void OnCloseClicked(object sender, EventArgs e)
		{
			Close();
		}

		void OnOpenInNewTab(object sender, EventArgs e)
		{
			MessageBox.Show("Wait for 0.3 please");
		}
	}
}