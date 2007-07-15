using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace ProfilerUi
{
	class Tab : IDisposable
	{
		readonly CallTreeView callTree;
		Rectangle bounds;
		Control outer;

		public Rectangle Bounds { get { return bounds; } }
		public CallTreeView CallTree { get { return callTree; } } 

		const string fontName = "MS Sans Serif";
		internal static Font unselectedFont = new Font(fontName, 8.5f, FontStyle.Regular);
		internal static Font selectedFont = new Font(fontName, 8.5f, FontStyle.Bold);

		public Tab(CallTreeView callTree, Control outer)
		{
			this.callTree = callTree;
			this.outer = outer;
		}

		public void Paint(Graphics g, ref int x, bool selected, Rectangle clientRect)
		{
			Font f = selected ? selectedFont : unselectedFont;
			Brush tabBrush = selected ?
				SystemBrushes.ButtonHighlight : SystemBrushes.ButtonFace;

			int width = (int)g.MeasureString(callTree.Text, f, int.MaxValue, StringFormat.GenericTypographic).Width;

			g.FillRectangle(tabBrush, x + 1, 2, width + 18, selected ? 50 : clientRect.Height - 3);
			g.DrawRectangle(SystemPens.ButtonShadow, x + 1, 2, width + 18, 50);

			g.DrawString(callTree.Text, f, Brushes.Black, x + 10, 4, StringFormat.GenericTypographic);

			bounds = new Rectangle(x, 0, width + 20, clientRect.Height);

			x += 20 + width;
		}

		~Tab()
		{
			Dispose();
		}

		bool disposed;

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
