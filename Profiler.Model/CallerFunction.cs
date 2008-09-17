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
		double ownTime, totalTime, minTime = double.MaxValue, maxTime = double.MinValue;

		public Name Name { get { return name; } }
		public int Calls { get { return calls; } }
		public double OwnTime { get { return ownTime; } }
		public double TotalTime { get { return totalTime; } }
		public bool Interesting { get { return name.Interesting; } }
		public double Average { get { if (calls > 0) return totalTime / (double)calls; else return 0.0; } }
		public double MinTime { get { return minTime; } }
		public double MaxTime { get { return maxTime; } }

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
			if (f.MinTime < minTime)
				minTime = f.MinTime;
			if (f.MaxTime > maxTime)
				maxTime = f.MaxTime;

			AddCaller(f.Parent);
		}

		public CallerFunction(Function f)
		{
			this.name = f.name;
			this.id = f.Id;

			this.calls = f.Calls;
			this.ownTime = f.OwnTime;
			this.totalTime = f.TotalTime;
			this.minTime = f.MinTime;
			this.maxTime = f.MaxTime;

			AddCaller(f.Parent);
		}

		public Node CreateView()
		{
			Node<CallerFunction> n = new Node<CallerFunction>(this);
			n.Collapse();

			foreach (CallerFunction f in callers.Values)
				n.Add(f.CreateView());

			return n;
		}
	}
}