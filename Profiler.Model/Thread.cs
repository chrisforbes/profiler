using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using IjwFramework.Ui;

namespace Ijw.Profiler.Model
{
	public class Thread : IActivatible, IProfilerElement
	{
		internal Dictionary<uint, Function> roots = new Dictionary<uint, Function>();
		internal Stack<Activation<Function>> activations = new Stack<Activation<Function>>();

		readonly uint id;
		double time = 0.0;
		public ulong lastEntryTime = 0;

		public double TotalTime { get { return time; } }

		public uint Id { get { return id; } }
		public bool Interesting { get { return true; } }

		public Thread(uint id) { this.id = id; }

		public string TabTitle { get { return "Thread #" + id; } }

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

		public Node CreateView(double totalTime)
		{
			CallTreeNode n = new CallTreeNode(this, totalTime);
			n.Collapse();

			List<Function> fns = new List<Function>(roots.Values);
			fns.Sort(Function.ByTimeDecreasing);

			foreach (Function f in fns)
				n.Add(f.CreateView(time));

			return n;
		}

		public bool IsGcThread
		{
			get
			{
				foreach (Function f in roots.Values)
					if (f.name.MethodName == "Finalize")
						return true;
				return false;
			}
		}

		public string Name
		{
			get
			{
				string s = "Thread #" + id;
				if (IsGcThread)
					s += " (Garbage Collector)";

				return s;
			}
		}
	}
}
