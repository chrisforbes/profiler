using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProfilerUi
{
	class Function
	{
		public Function(Function caller, string name, uint id)
		{
			this.caller = caller;
			this.name = name;
			this.id = id;
		}

		public int calls = 0;
		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public Function caller;
		public string name;
		public uint id;

		public double time;

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode(name + "    (" + calls + " calls)    " + time.ToString("F1") + "ms");
			n.Tag = this;

			List<Function> fns = new List<Function>(children.Values);
			fns.Sort(delegate(Function a, Function b) { return b.time.CompareTo(a.time); });

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView());

			return n;
		}
	}
}
