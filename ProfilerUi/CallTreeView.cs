using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;

namespace ProfilerUi
{
	class CallTreeView : TreeControl
	{
		public readonly CallTree src;

		public CallTreeView(ImageProvider provider, ColumnCollection collection, CallTree src, string title)
			: base(provider, collection)
		{
			this.src = src;
			this.title = title;
		}

		string title;

		public override string ToString()
		{
			return title;
		}
	}
}
