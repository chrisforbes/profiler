using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProfilerUi
{
	[ComVisible(true)]
	public class StartPageController
	{
		readonly Action<RunParameters> a;

		internal StartPageController(Action<RunParameters> a)
		{
			this.a = a;
		}

		public void Run(string cmd, string dir, string args)
		{
			a(new RunParameters(cmd, dir, args));
		}

		public void Snapshot(string cmd)
		{
			MessageBox.Show("TODO: load snapshot `" + cmd + "`");
		}
	}
}
