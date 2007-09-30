using System;
using System.Collections.Generic;
using System.Text;
using Ijw.Profiler.Core;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ijw.Profiler.Agents.JVM
{
	public class JvmAgent : IAgent
	{
		INameFactory nameFactory = new JvmNameFactory();
		public Run Execute(RunParameters p)
		{
			Run run = new Run();

			ProcessStartInfo info = new ProcessStartInfo(p.exePath);
			info.WorkingDirectory = p.workingDirectory;
			info.UseShellExecute = false;
			info.EnvironmentVariables["JAVA_TOOL_OPTIONS"] = "-agentpath:" + Application.StartupPath + "\\PJvmImpl.dll";
			info.EnvironmentVariables["ijwprof_txt"] = run.txtFile;
			info.EnvironmentVariables["ijwprof_bin"] = run.binFile;
			
			info.Arguments = p.parameters;

			Process.Start(info).WaitForExit();
			return run;
		}

		public INameFactory NameFactory { get { return nameFactory; } }
		public string Id { get { return "jvm"; } }
		public string Name { get { return "IJW Profiler for Java"; } }
	}
}
