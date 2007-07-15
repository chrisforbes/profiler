using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProfilerUi
{
	class Monkey : TreeNode
	{
		public IProfilerElement Element { get { return Tag as IProfilerElement; } }

		public Monkey(IProfilerElement e, string text)
			: base( text )
		{
			Tag = e;
		}
	}
}
