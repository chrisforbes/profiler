using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.Model
{
	public class CallerFunction
	{
		readonly Name name;
		readonly uint id;

		int calls;
		double ownTime, totalTime;

		public Name Name { get { return name; } }
		public int Calls { get { return calls; } }
		public double OwnTime { get { return ownTime; } }
		public double TotalTime { get { return totalTime; } }
		public bool Interesting { get { return name.Interesting; } }

		readonly Dictionary<uint, CallerFunction> callers = new Dictionary<uint, CallerFunction>();

		void AddCaller(Function f)
		{
			if (f == null)
				return;

			if (callers.ContainsKey(f.Id))
				callers[f.Id].Merge(f);
			else
				callers.Add(f.Id, new CallerFunction(f));
		}

		public void Merge(Function f)
		{
			calls += f.Calls;
			totalTime += f.TotalTime;
			ownTime += f.OwnTime;

			AddCaller(f.Parent);
		}

		public CallerFunction(Function f)
		{
			this.name = f.name;
			this.id = f.Id;

			this.calls = f.Calls;
			this.ownTime = f.OwnTime;
			this.totalTime = f.TotalTime;

			AddCaller(f.Parent);
		}

		public Node CreateView()
		{
			CallerTreeNode n = new CallerTreeNode(this);
			n.Collapse();

			foreach (CallerFunction f in callers.Values)
				n.Add(f.CreateView());

			return n;
		}
	}
}