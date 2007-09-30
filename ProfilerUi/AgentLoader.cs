using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.UI
{
	class AgentLoader
	{
		string defaultAgent;
		List<IAgent> agents;

		static IEnumerable<Assembly> GetAgentAssemblies()
		{
			foreach (string dllName in Directory.GetFiles(Application.StartupPath, "*.Agents.*.dll"))
				yield return Assembly.LoadFile(dllName);
		}

		static IEnumerable<IAgent> GetAgents(IEnumerable<Assembly> assemblies)
		{
			foreach (Assembly a in assemblies)
				foreach (Type t in a.GetTypes())
					if (typeof(IAgent).IsAssignableFrom(t))
						yield return (IAgent)t.GetConstructor(Type.EmptyTypes).Invoke(null);
		}

		public AgentLoader(string def)
		{
			agents = new List<IAgent>(GetAgents(GetAgentAssemblies()));
			defaultAgent = def;
		}

		public IEnumerable<IAgent> Agents { get { return agents; } }

		public IAgent Default
		{
			get
			{
				foreach (IAgent a in agents)
					if (defaultAgent == a.Id)
						return a;

				return Util.First(agents);
			}
		}

		public IAgent GetAgent(string agent)
		{
			foreach (IAgent a in agents)
				if (agent == a.Id)
					return a;

			return Default;
		}
	}
}
