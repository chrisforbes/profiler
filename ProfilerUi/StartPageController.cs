using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IjwFramework.Delegates;
using System.Diagnostics;

namespace ProfilerUi
{
	[ComVisible(true)]
	public class StartPageController
	{
		readonly Action<RunParameters> a;
		readonly Action updateFunction;
		readonly string version;

		internal StartPageController(string version, Action<RunParameters> a, Action updateFunction)
		{
			this.updateFunction = updateFunction;
			this.a = a;
			this.version = version;
		}

		public void Run(string cmd, string dir, string args)
		{
			a(new RunParameters(cmd, dir, args));
		}

		public void Update(string cmd)
		{
			updateFunction();
		}

		public void Snapshot(string cmd)
		{
			//todo
		}

		public void OpenExternalLink(string url)
		{
			Process.Start(url);
		}

		public string GetVersion()
		{
			return version;
		}
	}
}
