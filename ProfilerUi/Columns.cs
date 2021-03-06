using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using IjwFramework.Ui;
using Ijw.Profiler.Core;
using Ijw.Profiler.Model;

namespace Ijw.Profiler.UI
{
	class Columns
	{
		static Font font, boldFont;
		readonly ImageProvider imageProvider;

		public Columns(Font font, Font boldFont, ImageProvider provider)
		{
			Columns.font = font;
			Columns.boldFont = boldFont;
			this.imageProvider = provider;
		}

		static Brush GetBrush(bool interesting)
		{
			return interesting ? SystemBrushes.WindowText : Brushes.Gray;
		}

		static Brush GetBrush(IProfilerElement e) { return GetBrush(e.Interesting); }
		static Brush GetBrush(CallerFunction f) { return GetBrush(f.Interesting); }

		static string GetImage2(Name name)
		{
			switch (name.Type)
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

		static string GetImage(Name name)
		{
			return GetImage2(name) + (name.Interesting ? "" : "_grey");
		}

		public static Action<IColumn, Painter, Node> BindSubtype<T>(Action<IColumn, Painter, Node<T>> f)
		{
			return (c, p, n) => f(c, p, (Node<T>)n);
		}

		public void RenderCallerColumn(IColumn c, Painter p, Node<CallerFunction> n)
		{
			string s = "call_out";
			if (!n.Value.Interesting) s += "_grey";
			p.DrawImage(imageProvider.GetImage(s));
			RenderName(c, n.Value.Name, p);
		}

		static Brush GetBrush<T>(T value)
		{
			if (value is IProfilerElement)
				return GetBrush((IProfilerElement)value);
			if (value is CallerFunction)
				return GetBrush((CallerFunction)(object)value);

			throw new NotImplementedException("wtf??");
		}

		public static Action<IColumn, Painter, Node> MakeRightAlignedColumn<T>(Func<T, object> extractValueFunc)
			where T : class
		{
			return MakeRightAlignedColumn<T, T>(extractValueFunc);
		}

		public static Action<IColumn, Painter, Node> MakeRightAlignedColumn<T,U>(Func<T, object> extractValueFunc)
			where T : class
		{
			return (c, p, n) =>
				{
					var nn = (Node<U>)n;

					var v = nn.Value as T;
					if (v == null)
						return;		// wrong type

					var value = extractValueFunc(v);
					RenderRightAligned(c, p, value.ToString(), GetBrush(v));
				};
		}

		public void RenderCallerCallsColumn(IColumn c, Painter p, Node<CallerFunction> n)
		{
			RenderRightAligned(c, p, n.Value.Calls.ToString(), GetBrush(n.Value));
		}

		void RenderName(IColumn c, Name name, Painter p)
		{
			Brush brush = GetBrush(name.Interesting);
			p.DrawImage(imageProvider.GetImage(GetImage(name)));
			p.Pad(2);
			p.DrawString(name.ClassName, font, brush, 1, c.Left + c.Width);
			if (!string.IsNullOrEmpty(name.MethodName))
				p.DrawString((name.Type == MethodType.Constructor ? "  " : " .") + name.MethodName,
					boldFont, brush, 1, c.Left + c.Width);
		}

		public void RenderFunctionColumn(IColumn c, Painter p, Node<IProfilerElement> n)
		{
			IProfilerElement e = n.Value;

			Brush brush = GetBrush(e);

			Function f = e as Function;
			if (f != null)
			{
				p.DrawImage(imageProvider.GetImage(GetTimeIcon(f)));
				RenderName(c, f.name, p);
				return;
			}

			Thread t = e as Thread;
			if (t != null)
			{
				p.DrawImage(imageProvider.GetImage("thread"));
				p.Pad(2);
				p.DrawString(t.Name, font, brush, 1, c.Left + c.Width);
			}
		}

		public void RenderPercentColumn(IColumn c, Painter p, Node n)
		{
			CallTreeNode nn = (CallTreeNode)n;
			Function e = nn.Value as Function;

			if (e != null)
				RenderRightAligned(c, p, (e.TotalTime / nn.rootTime).ToString("P1"), GetBrush(e));
		}

		public void RenderCallsColumn(IColumn c, Painter p, Node<IProfilerElement> nn)
		{
			IProfilerElement e = nn.Value;

			Function f = e as Function;
			if (f != null)
				RenderRightAligned(c, p, f.Calls.ToString(), GetBrush(e));
		}

		static void RenderRightAligned(IColumn c, Painter p, string s, Brush b)
		{
			p.SetPosition(c.Left + c.Width - 4);
			p.Alignment = StringAlignment.Far;
			p.DrawString(s, font, b, 1, c.Left + c.Width);
			p.Alignment = StringAlignment.Near;
		}

		
		static string GetTimeIcon2(Function f)
		{
			const double upperLimit = 0.9;
			const double lowerLimit = 0.1;
			if (f.TotalTime < 1e-5) return "call_in";

			double ratio = f.OwnTime / f.TotalTime;

			if (ratio < lowerLimit)
				return "call_in";
			
			if (ratio > upperLimit)
				return "call_in_self";

			return "call_in_mixed";
		}

		static string GetTimeIcon(Function f)
		{
			string s = GetTimeIcon2(f);
			if (!f.Interesting) s += "_grey";
			return s;
		}
	}
}