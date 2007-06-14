using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ProfilerUi
{
	class Function
	{
		public Function(string name)
		{
			this.name = name;
		}

		public int calls = 0;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public string name;
		public double time;

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode(name + "    (" + calls + " calls)    " + time.ToString("F1") + "ms");
			n.Tag = this;

			List<Function> fns = new List<Function>(children.Values);
			fns.Sort(byTimeDecreasing);

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView());

			return n;
		}

		static Comparison<Function> byTimeDecreasing = delegate(Function a, Function b)
		{
			return b.time.CompareTo(a.time);
		};
	}

	class FunctionFilter
	{
		Regex regex;

		public FunctionFilter(string[] patterns)
		{
			string p = "";
			foreach (string s in patterns)
			{
				string r = "(" + Regex.Escape(s) + ")";
				p = string.IsNullOrEmpty(p) ? r : p + "|" + r;
			}

			regex = new Regex(p);
		}

		public bool IsFiltered(Function f)
		{
			return regex.IsMatch(f.name);
		}
	}
}
