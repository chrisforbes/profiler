using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IjwFramework.Delegates;
using System.Diagnostics;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.UI
{
	[ComVisible(true)]
	public class StartPageController
	{
		readonly Action<RunParameters> a;
		readonly Action updateFunction;
		readonly string version;
		readonly AgentLoader loader;

		internal StartPageController(string version, Action<RunParameters> a, Action updateFunction, AgentLoader loader)
		{
			this.updateFunction = updateFunction;
			this.a = a;
			this.version = version;
			this.loader = loader;
		}

		public void Run(string cmd, string dir, string args, string agent)
		{
			a(new RunParameters(cmd, dir, args, loader.GetAgent(agent)));
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

		public string GetInstalledAgents()
		{
			StringBuilder sb = new StringBuilder();

			IAgent defaultAgent = loader.Default;

			foreach (IAgent agent in loader.Agents)
			{
				if (agent == defaultAgent)
					sb.AppendLine("<li><b>" + agent.Name + " (Default)</b></li>");
				else
					sb.AppendLine("<li>" + agent.Name + "</li>");
			}

			return sb.ToString();
		}
	}
}
