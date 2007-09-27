using System;
using System.Collections.Generic;
using System.Text;

namespace Ijw.Profiler.Core
{
	public interface INameFactory
	{
		Name Create(string rawName);
	}
}
