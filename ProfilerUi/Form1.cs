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

		void LoadLastRun(object sender, EventArgs e)
		{
			FunctionNameProvider names = new FunctionNameProvider("c:\\profile.txt");

			CallTree tree = new CallTree("c:\\profile.bin", names);

			callTreeView1.BeginUpdate();
			callTreeView1.Nodes.Clear();
			foreach (Thread thread in tree.threads.Values)
				callTreeView1.Nodes.Add(thread.CreateView());
			callTreeView1.EndUpdate();
		}
	}
}