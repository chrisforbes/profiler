using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using IjwFramework.Ui;

namespace ProfilerUi
{
	class Columns
	{
		readonly Font font, boldFont;
		readonly ImageProvider imageProvider;

		public Columns(Font font, Font boldFont, ImageProvider provider)
		{
			this.font = font;
			this.boldFont = boldFont;
			this.imageProvider = provider;
		}

		static Brush GetBrush(IProfilerElement e)
		{
			Function f = e as Function;
			if (f == null || f.Interesting)
				return SystemBrushes.WindowText;

			return Brushes.Gray;
		}

		static string GetImage(MethodType t)
		{
			switch (t)
			{
				case MethodType.Method: return "method";
				case MethodType.PropertyGet: return "prop_get";
				case MethodType.PropertySet: return "prop_set";
				case MethodType.EventAdd: return "event_add";
				case MethodType.EventRemove: return "event_remove";
				case MethodType.Constructor: return "ctor";
				default:
					return "method";
			}
		}

		public void RenderFunctionColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			Brush brush = GetBrush(e);

			Function f = e as Function;
			if (f != null)
			{
				Image i = imageProvider.GetImage(GetImage(f.name.Type));
				p.DrawImage(i);
				p.Pad(2);
				p.DrawString(f.name.ClassName, font, brush, 1, c.Left + c.Width);
				p.DrawString((f.name.Type == MethodType.Constructor ? "  " : " .") + f.name.MethodName, boldFont, brush, 1, c.Left + c.Width);
				return;
			}

			Thread t = e as Thread;
			if (t != null)
			{
				Image i = imageProvider.GetImage("thread");
				p.DrawImage(i);
				p.Pad(2);
				p.DrawString("Thread #" + t.Id + (t.IsGcThread ? " (Garbage Collector)" : ""), font, brush, 1, c.Left + c.Width);
			}
		}

		public void RenderPercentColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			Function e = nn.Value as Function;

			if (e == null)
				return;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;

			double frac = e.TotalTime / nn.rootTime;

			p.DrawString(frac.ToString("P1"), font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		public void RenderTotalTimeColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;
			p.DrawString(e.TotalTime.ToString("F1") + " ms", font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		public void RenderOwnTimeColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			Function e = nn.Value as Function;
			if (e == null)
				return;

			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;
			p.DrawString(e.OwnTime.ToString("F1") + " ms", font, GetBrush(e), 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		public void RenderCallsColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			IProfilerElement e = nn.Value;

			Function f = e as Function;
			if (f != null)
			{
				p.SetPosition(c.Left + c.Width - 4);
				p.Alignment = StringAlignment.Far;
				p.DrawString(f.Calls.ToString(), font, GetBrush(e), 1, c.Left + c.Width);
				p.Alignment = StringAlignment.Near;
			}
		}
	}
}