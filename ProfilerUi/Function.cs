using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ProfilerUi
{
	class Function : IActivatible, IProfilerElement
	{
		public Function(uint id, string name) { this.id = id; this.name = name; }

		int calls;
		double time;
		public readonly uint id;

		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public string name;

		public double TimeInChildren
		{
			get
			{
				double value = 0.0;
				foreach (Function f in children.Values)
					value += f.time;

				return value;
			}
		}

		public double OwnTime { get { return time - TimeInChildren; } }
		public double TotalTime { get { return time; } }

		public string TabTitle { get { return name.Substring(name.IndexOf("::") + 2); } }

		public TreeNode CreateView( double rootTime )
		{
			TreeNode n = new Node( this, string.Format("{0} - {1} calls - {4:F1}% {2:F1}ms - [{3:F1}ms]",
				name, calls, time, OwnTime, 100.0 * time / rootTime));

			List<Function> fns = new List<Function>(children.Values);
			fns.Sort(ByTimeDecreasing);

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView(rootTime));

			return n;
		}

		public void Complete(double milliseconds)
		{
			time += milliseconds;
			calls++;
		}

		public static Comparison<Function> ByTimeDecreasing =
			delegate(Function a, Function b) { return b.time.CompareTo(a.time); };

		// note: this just collects top-level invocations.

		public List<Function> CollectInvocations(uint functionId)
		{
			List<Function> result = new List<Function>();
			CollectInvocationsInto(result, functionId);
			return result;
		}

		internal void CollectInvocationsInto(List<Function> list, uint functionId)
		{
			if (id == functionId)
				list.Add(this);
			else
				foreach (Function f in children.Values)
					f.CollectInvocationsInto(list, functionId);
		}

		static IEnumerable<T> Yield<T>(params T[] t) { return t; }

		public static Function Merge(IEnumerable<Function> invocations)
		{
			Function f = null;

			foreach (Function i in invocations)
			{
				if (f == null)
					f = new Function(i.id, i.name);

				f.calls += i.calls;
				f.time += i.time;

				foreach (Function c in i.children.Values)
				{
					Function oc;
					if (!f.children.TryGetValue(c.id, out oc))
						oc = c;
					else
						oc = Merge(Yield(oc, c));

					f.children[oc.id] = oc;
				}
			}

			return f;
		}
	}
}
