using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;

namespace ProfilerUi
{
	class CallTreeNode : Node
	{
		public readonly IProfilerElement Value;

		public double rootTime;

		public CallTreeNode(IProfilerElement value, double rootTime)
			: base()
		{
			Value = value;
			this.rootTime = rootTime;
		}

		public string TabName { get { return Value.TabTitle; } }

		public CallTreeNode RootFunction
		{
			get
			{
				CallTreeNode n = this;
				if (!(n.Value is Function))
					return null;

				while ((n.parent is CallTreeNode) && (n.parent as CallTreeNode).Value is Function)
					n = n.parent as CallTreeNode;

				return n;
			}
		}
	}
}
