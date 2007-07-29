using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.TabStrip;
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
			view.BorderStyle = BorderStyle.None;
		}

		public override string ToString() { return view.ToString(); }
	}
}