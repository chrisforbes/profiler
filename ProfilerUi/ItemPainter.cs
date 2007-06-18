using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ProfilerUi
{
	class ItemPainter
	{
		readonly Graphics g;
		Point location;

		public ItemPainter(Graphics g, Point location)
		{
			this.g = g;
			this.location = location;
		}

		public void DrawImage(Image i)
		{
			g.DrawImage(i, location);
			location.Offset(i.Width, 0);
		}

		public void Pad(int pixels)
		{
			location.Offset(pixels, 0);
		}

		public void DrawText(string text, Font font, Brush brush, int dy)
		{
			int width = (int)g.MeasureString(text, font, int.MaxValue, StringFormat.GenericTypographic).Width;
			g.DrawString(text, font, brush, location.X, location.Y + dy, StringFormat.GenericTypographic);
			location.Offset(width, 0);
		}
	}
}
