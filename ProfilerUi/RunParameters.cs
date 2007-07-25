using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilerUi
{
	class RunParameters
	{
		public readonly string exePath;
		public readonly string workingDirectory;
		public readonly string parameters;

		public RunParameters(string exePath, string workingDirectory, string parameters)
		{
			this.exePath = exePath;
			this.workingDirectory = workingDirectory;
			this.parameters = parameters;
		}
	}
}
