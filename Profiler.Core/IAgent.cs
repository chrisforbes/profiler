using System;
using System.Collections.Generic;
using System.Text;

namespace Ijw.Profiler.Core
{
	public interface IAgent
	{
		Run Execute(RunParameters p);
		string Name { get; }
		string Id { get; }
		INameFactory NameFactory { get; }
	}
}
