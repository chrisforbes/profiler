using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;

namespace ProfilerUi
{
	class CallerTreeNode : Node
	{
		readonly CallerFunction value;

		public CallerFunction Value { get { return value; } }

		public CallerTreeNode(CallerFunction cf)
			: base()
		{
			value = cf;
		}
	}
}
