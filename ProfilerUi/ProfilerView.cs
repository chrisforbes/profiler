using System;
using System.Collections.Generic;
using System.Text;
using IjwFramework.Ui;
using System.Windows.Forms;
using System.Drawing;

namespace Ijw.Profiler.UI
{
	class ProfilerView : ViewBase
	{
		public readonly CallTreeView view;
		public readonly Panel cc;

		public static ProfilerView Create(MultipleViewManager host, CallTreeView c, TreeColumnHeader h, LegendBar b)
		{
			Panel p = new Panel();
			return new ProfilerView(host, c, h, p, b);
		}

		ProfilerView(MultipleViewManager host, CallTreeView v, TreeColumnHeader h, Panel p, LegendBar b)
			: base( host, p )
		{
			cc = p;
			cc.Size = new Size(300, 300);

			cc.Controls.Add(v);
			v.Anchor = AnchorUtil.ClientArea;
			v.Location = new Point(0, 20);
			v.Size = new Size(cc.Width, cc.Height - 40);

			cc.Controls.Add(h);
			h.Anchor = AnchorUtil.TopEdge;
			h.Location = new Point();
			h.Size = new Size(cc.ClientSize.Width, 20);

			cc.Controls.Add(b);
			b.Anchor = AnchorUtil.BottomEdge;
			b.Location = new Point(0, cc.ClientSize.Height - 20);
			b.Size = new Size(cc.ClientSize.Width, 20);

			this.view = v;
		}

		public override string ToString() { return view.ToString(); }
	}
}
