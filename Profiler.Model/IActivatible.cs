using System;
using System.Collections.Generic;
using System.Text;

namespace Ijw.Profiler.Model
{
	interface IActivatible
	{
		void Complete(double milliseconds);
	}
}
