using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Agents.CLR
{
	public class ClrAgent : IAgent
	{
		static Predicate<string> isFunctionInteresting = 
			new NameFilter("System.", "Microsoft.", "MS.Internal.").IsFunctionInteresting;

		INameFactory nameFactory = new ClrNameFactory(isFunctionInteresting);

		public Run Execute(RunParameters p)
		{
			using (new ComServerRegistration(Application.StartupPath + "/pcomimpl.dll"))
			{
				Run run = new Run();

				ProcessStartInfo info = new ProcessStartInfo(p.exePath);
				info.WorkingDirectory = p.workingDirectory;
				info.UseShellExecute = false;
				info.EnvironmentVariables["Cor_Enable_Profiling"] = "1";
				info.EnvironmentVariables["COR_PROFILER"] = "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}";

				info.EnvironmentVariables["ijwprof_txt"] = run.txtFile;
				info.EnvironmentVariables["ijwprof_bin"] = run.binFile;

				info.Arguments = p.parameters;

				Process.Start(info).WaitForExit();
				return run;
			}
		}

		public string Name { get { return "IJW Profiler for the .NET Framework"; } }
		public string Id { get { return "clr"; } }
		public INameFactory NameFactory { get { return nameFactory; } }
	}
}
