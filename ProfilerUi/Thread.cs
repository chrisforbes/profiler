using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ProfilerUi
{
	class Thread : IActivatible, IProfilerElement
	{
		public Dictionary<uint, Function> roots = new Dictionary<uint, Function>();

		public Stack<Activation<Function>> activations = new Stack<Activation<Function>>();

		readonly uint id;
		double time = 0.0;
		public ulong lastEntryTime = 0;

		public double TotalTime { get { return time; } }

		public uint Id { get { return id; } }

		public Thread(uint id) { this.id = id; }

		public string TabTitle { get { return "Thread #" + id; } }

		public TreeNode CreateView( double totalTime )
		{
			TreeNode n = new Node(this, "Thread #" + id.ToString() + " - " + time.ToString("F1") + "ms");

			List<Function> fns = new List<Function>(roots.Values);
			fns.Sort(Function.ByTimeDecreasing);

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView(time));

			return n;
		}

		public void Complete(double milliseconds)
		{
			time += milliseconds;
		}

		public List<Function> CollectInvocations(uint functionId)
		{
			List<Function> result = new List<Function>();
			foreach( Function f in roots.Values )
				f.CollectInvocationsInto(result, functionId);

			return result;
		}

		public void WriteTo(XmlWriter writer)
		{
			writer.WriteStartElement("thread");
			writer.WriteAttributeString("id", id.ToString());
			writer.WriteAttributeString("time", time.ToString());

			foreach (Function f in roots.Values )
				f.WriteTo(writer);

			writer.WriteEndElement();
		}
	}
}
