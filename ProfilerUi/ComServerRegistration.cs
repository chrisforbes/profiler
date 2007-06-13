using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProfilerUi
{
	class ComServerRegistration : IDisposable
	{
		string dll;
		bool disposed;

		void WaitForProcess(string cmdline, string args)
		{
			Process p = Process.Start(cmdline, args);
			p.WaitForExit();
		}

		public ComServerRegistration(string dll)
		{
			this.dll = dll;
			WaitForProcess("regsvr32.exe", "/s \"" + dll + '"');
		}

		~ComServerRegistration()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (disposed)
				return;

			WaitForProcess("regsvr32.exe", "/s /u \"" + dll + '"');
			disposed = true;

			GC.SuppressFinalize(this);
		}
	}
}
