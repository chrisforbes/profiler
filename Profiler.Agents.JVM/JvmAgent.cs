using System;
using System.Collections.Generic;
using System.Text;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Agents.JVM
{
	public class JvmAgent : IAgent
	{
		public Run Execute(RunParameters p)
		{
			throw new NotImplementedException();
		}

		public INameFactory NameFactory { get { throw new NotImplementedException(); } }

		public string Name { get { return "Java"; } }
	}
}
