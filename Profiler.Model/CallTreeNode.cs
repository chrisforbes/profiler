using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;

namespace Ijw.Profiler.Model
{
	public class CallTreeNode : Node<IProfilerElement>
	{
		public readonly double rootTime;

		public CallTreeNode(IProfilerElement value, double rootTime)
			: base(value)
		{
			this.rootTime = rootTime;
		}
	}
}
