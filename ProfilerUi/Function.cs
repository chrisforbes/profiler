using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ProfilerUi
{
	class Function : IActivatible
	{
		public Function(string name) { this.name = name; }

		int calls;
		double time;

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

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode(string.Format("{0} - {1} calls - {2:F1}ms - [{3:F1}ms]",
				name, calls, time, OwnTime));
			n.Tag = this;

			List<Function> fns = new List<Function>(children.Values);
			fns.Sort(ByTimeDecreasing);

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView());

			return n;
		}

		public void Complete(double milliseconds)
		{
			time += milliseconds;
			calls++;
		}

		public static Comparison<Function> ByTimeDecreasing =
			delegate(Function a, Function b) { return b.time.CompareTo(a.time); };
	}
}
