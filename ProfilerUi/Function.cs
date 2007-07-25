using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using IjwFramework.Delegates;

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

		public TreeNode CreateView( double rootTime, Predicate<string> filter )
		{
			TreeNode n = new Node( this, string.Format("{0} - {1} calls - {4:F1}% {2:F1}ms - [{3:F1}ms]",
				name, calls, time, OwnTime, 100.0 * time / rootTime));

			List<Function> fns = new List<Function>(WantedChildren(filter));
			fns.Sort(ByTimeDecreasing);

			foreach (Function f in fns)
				n.Nodes.Add(f.CreateView(rootTime, filter));

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
					f = new Function(i.id, i.name);

				f.calls += i.calls;
				f.time += i.time;

				foreach (Function c in i.children.Values)
					MergeInto(f.children, c);
			}

			return f;
		}

		IEnumerable<Function> WantedChildren(Predicate<string> f)
		{
			if (!f(name)) return children.Values;

			Dictionary<uint, Function> result = new Dictionary<uint, Function>();
			Function o;

			foreach (Function func in WantedChildrenInternal(f))
				MergeInto(result, func);

			return result.Values;
		}

		IEnumerable<Function> WantedChildrenInternal(Predicate<string> f)
		{
			Queue<Function> q = new Queue<Function>();

			foreach (Function child in children.Values)
				q.Enqueue(child);

			while (q.Count > 0)
			{
				Function current = q.Dequeue();
				if (!f(current.name))
					yield return current;
				else
					foreach (Function cc in current.children.Values)
						q.Enqueue(cc);
			}
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
