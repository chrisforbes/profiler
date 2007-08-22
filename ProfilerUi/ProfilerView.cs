using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;
using System.Windows.Forms;

namespace ProfilerUi
{
	class ProfilerView : ViewBase
	{
		public readonly CallTreeView view;

		public ProfilerView(MultipleViewManager host, CallTreeView v)
			: base( host, v )
		{
			this.view = v;
		}

		public override string ToString() { return view.ToString(); }
	}
}
