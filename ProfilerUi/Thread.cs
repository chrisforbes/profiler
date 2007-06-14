using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProfilerUi
{
	class Thread
	{
		public Dictionary<uint, Function> roots = new Dictionary<uint, Function>();

		public Stack<Activation> activations = new Stack<Activation>();

		readonly int id;
		public double time = 0.0;
		public ulong lastEntryTime = 0;

		public int Id { get { return id; } }

		public Thread(int id) { this.id = id; }

		public TreeNode CreateView()
		{
			TreeNode n = new TreeNode("Thread #" + id.ToString() + " - " + time.ToString("F1") + "ms");
			n.Tag = this;

			List<Function> fns = new List<Function>(roots.Values);
			fns.Sort(delegate(Function a, Function b) { return b.time.CompareTo(a.time); });

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView());

			return n;
		}
	}
}
