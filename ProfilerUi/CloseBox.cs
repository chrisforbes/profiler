using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ProfilerUi
{
	class CloseBox
	{
		Control host;
		bool hover;

		bool Hover
		{
			get { return hover; }
			set
			{
				if (hover == value)
					return;
				hover = value;
				host.Invalidate();
			}
		}

		Rectangle bounds
		{
			get { return new Rectangle(host.Width - 18, 2, 16, host.Height - 4); }
		}

		public event EventHandler CloseClicked = delegate { };

		public CloseBox(Control host)
		{
			this.host = host;

			host.Resize += delegate
			{
				host.Invalidate();
			};

			host.MouseLeave += delegate
			{
				Hover = false;
				host.Invalidate();
			};

			host.MouseMove += delegate(object sender, MouseEventArgs e)
			{
				Hover = bounds.Contains(e.Location);
			};

			host.MouseUp += delegate(object sender, MouseEventArgs e)
			{
				if (bounds.Contains(e.Location) && e.Button == MouseButtons.Left)
					CloseClicked(this, EventArgs.Empty);
			};
		}

		public void Paint(Graphics g)
		{
			if ((host as CallTreeTabStrip) != null && (host as CallTreeTabStrip).CurrentCallTree == null)
				return;

			if (Hover)
			{
				g.FillRectangle(SystemBrushes.ButtonHighlight, bounds);
				g.DrawRectangle(Pens.Black, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			}

			g.DrawString("\x72", new Font("Marlett", 8.5f), Brushes.Black, new PointF(bounds.X + 1, bounds.Y + 3));
		}
	}
}
