using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilerUi
{
	interface INameFactory
	{
		Name Create(string rawName);
	}
}
