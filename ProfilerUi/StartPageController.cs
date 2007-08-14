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
		readonly string version;

		internal StartPageController(string version, Action<RunParameters> a)
		{
			this.a = a;
			this.version = version;
		}

		public void Run(string cmd, string dir, string args)
		{
			a(new RunParameters(cmd, dir, args));
		}

		public void Snapshot(string cmd)
		{
			//todo
		}

		public string GetVersion()
		{
			return version;
		}
	}
}
