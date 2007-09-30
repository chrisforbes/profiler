using System;
using System.Collections.Generic;
using System.Text;

namespace Ijw.Profiler.Model
{
	class Activation<T>
		where T : IActivatible
	{
		public Activation(T target, ulong start)
		{
			this.target = target;
			this.start = start;
		}

		T target;
		ulong start;

		public void Complete(ulong end, ulong freq)
		{
			double t = GetTime(start, end, freq);
			target.Complete(t);
		}

		public T Target { get { return target; } }

		public static double GetTime(ulong start, ulong end, ulong freq)
		{
			if (end < start)
				return 0;	//hack
			return 1000 * (end - start) / (double)freq;
		}
	}
}
