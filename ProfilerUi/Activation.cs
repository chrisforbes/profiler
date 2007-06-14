using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilerUi
{
	class Activation
	{
		public Activation(Function f, ulong start)
		{
			this.f = f;
			this.start = start;
		}

		Function f;
		ulong start;

		public void Complete(ulong end, ulong freq)
		{
			double t = GetTime(start, end, freq);
			f.time += t;
			f.calls++;
		}

		public Function Function { get { return f; } }

		public static double GetTime(ulong start, ulong end, ulong freq)
		{
			return 1000 * (end - start) / (double)freq;
		}
	}
}
