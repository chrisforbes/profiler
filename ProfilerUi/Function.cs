using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using IjwFramework.Delegates;
using IjwFramework.Ui;

namespace ProfilerUi
{
	class Function : IActivatible, IProfilerElement
	{
		public Function(uint id, Name name, bool interesting)
		{
			this.id = id; 
			this.name = name;
			this.interesting = interesting;
		}

		int calls;

		public int Calls { get { return calls; } }
		public bool Interesting { get { return interesting; } }

		bool interesting;
		double time;
		public readonly uint id;

		public Dictionary<uint, Function> children = new Dictionary<uint, Function>();
		public Name name;

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

		public string TabTitle { get { return name.MethodName; } }

		public Node CreateView( double rootTime )
		{
			Node n = new CallTreeNode( this );

			n.Collapse();

			List<Function> fns = new List<Function>(children.Values);
			fns.Sort(ByTimeDecreasing);

			foreach (Function f in fns)
				n.Add(f.CreateView(rootTime));

			return n;
		}

		public void Complete(double milliseconds)
		{
			time += milliseconds;
			calls++;
		}

		public static Comparison<Function> ByTimeDecreasing =
			delegate(Function a, Function b) { return b.time.CompareTo(a.time); };

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

		static void MergeInto(Dictionary<uint, Function> dest, Function f)
		{
			Function o;
			dest[f.id] = dest.TryGetValue(f.id, out o) ?
				Merge(Iterators.Yield(o, f)) : f;
		}

		public static Function Merge(IEnumerable<Function> invocations)
		{
			Function f = null;

			foreach (Function i in invocations)
			{
				if (f == null)
					f = new Function(i.id, i.name, i.interesting);

				f.calls += i.calls;
				f.time += i.time;

				foreach (Function c in i.children.Values)
					MergeInto(f.children, c);
			}

			return f;
		}

		public void WriteTo(XmlWriter writer)
		{
			writer.WriteStartElement("function");
			writer.WriteAttributeString("id", id.ToString());
			writer.WriteAttributeString("calls", calls.ToString());
			writer.WriteAttributeString("time", time.ToString());

			foreach (Function f in children.Values)
				f.WriteTo(writer);

			writer.WriteEndElement();
		}
	}
}
