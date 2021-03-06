using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using IjwFramework.Types;
using IjwFramework.Ui;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ijw.Profiler.UI
{
	using Item = Pair<string, string>;

	class LegendBar : Control
	{
		readonly ImageProvider imageProvider;
		readonly List<IEnumerable<Item>> items =
			new List<IEnumerable<Item>>();
		int currentPage = 0;
		ToolTip tt = new ToolTip();

		public LegendBar(ImageProvider provider)
			: base()
		{
			imageProvider = provider;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();
			Cursor = Cursors.Hand;
			tt.SetToolTip(this, "Click for more");
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Painter p = new Painter( e.Graphics, new Rectangle(0,3,ClientSize.Width, ClientSize.Height - 4) );

			p.Pad(4);
			p.DrawString(string.Format("({0}/{1})", currentPage % items.Count + 1, items.Count),
				Font, Brushes.Blue, 1, ClientSize.Width);

			foreach (Item i in items[ currentPage % items.Count ] )
				PaintItem(i, p);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			int a1 = Height / 4;

			e.Graphics.FillRectangle(SystemBrushes.ButtonFace, ClientRectangle);
			using (Brush b = new LinearGradientBrush(new Point(0, a1),
				new Point(0, 0), SystemColors.ButtonFace, SystemColors.AppWorkspace))
				e.Graphics.FillRectangle(b, 0, 0, Width, a1);

			e.Graphics.DrawLine(Pens.Black, 0, 0, Width, 0);
		}

		void PaintItem(Pair<string, string> item, Painter p)
		{
			p.Pad(4);
			p.DrawImage(imageProvider.GetImage(item.First));
			p.Pad(2);
			p.DrawString(item.Second, Font, SystemBrushes.ControlText, 1, ClientSize.Width);
			p.Pad(4);
		}

		public void Add(IEnumerable<Pair<string,string>> page)
		{
			items.Add(page);
			Invalidate();
		}

		protected override void OnClick(EventArgs e)
		{
			++currentPage;
			Invalidate();
		}
	}
}
