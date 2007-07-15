using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProfilerUi
{
	class Run : IDisposable
	{
		public readonly string binFile = Path.GetTempFileName();
		public readonly string txtFile = Path.GetTempFileName();

		bool disposed = false;

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				File.Delete(binFile);
				File.Delete(txtFile);
				GC.SuppressFinalize(this);
			}
		}

		~Run() { Dispose(); }
	}
}
