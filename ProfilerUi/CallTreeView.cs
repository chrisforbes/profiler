using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ProfilerUi
{
	class CallTreeView : TreeView
	{
		ImageList images = new ImageList();
		Predicate<Function> filter;

		public CallTreeView( Predicate<Function> filter )
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			this.filter = filter;

			try
			{
				images.TransparentColor = Color.Fuchsia;
				images.Images.Add("collapsed", Image.FromFile("res/collapsed.bmp"));
				images.Images.Add("expanded", Image.FromFile("res/expanded.bmp"));
				images.Images.Add("thread", Image.FromFile("res/thread.bmp"));
				images.Images.Add("prop_set", Image.FromFile("res/prop_set.bmp"));
				images.Images.Add("prop_get", Image.FromFile("res/prop_get.bmp"));
				images.Images.Add("method", Image.FromFile("res/method.bmp"));
			}
			catch { }
		}

		Brush selected = new SolidBrush(Color.FromArgb(0xee, 0xee, 0xff));

		bool IsPropGetter(Monkey n) { return n.Text.Contains("get_"); }
		bool IsPropSetter(Monkey n) { return n.Text.Contains("set_"); }

		string GetEffectiveName(Monkey n)
		{
			if (IsPropGetter(n))
				return n.Text.Replace("get_", "");

			if (IsPropSetter(n))
				return n.Text.Replace("set_", "");

			return n.Text;
		}

		string GetImageKey(Monkey n)
		{
			if (IsPropGetter(n))
				return "prop_get";
			if (IsPropSetter(n))
				return "prop_set";
			if (n.Tag is Thread)
				return "thread";

			return "method";
		}

		void Draw(Graphics g, Monkey monkey, Rectangle bounds)
		{
			g.Clip = new Region(new Rectangle(new Point(0, monkey.Bounds.Top), new Size(Width, monkey.Bounds.Height)));
			g.FillRectangle((monkey == SelectedNode) ? selected : SystemBrushes.Window, bounds);

			ItemPainter painter = new ItemPainter(g, monkey.Bounds.Location);
			painter.DrawImage(images.Images[GetImageKey(monkey)]);
			painter.Pad(2);
			painter.DrawText(GetEffectiveName(monkey), Font, GetBrush(monkey.Element as Function), 1);

			if (monkey.Nodes.Count > 0)
			{
				Image i = images.Images[monkey.IsExpanded ? "expanded" : "collapsed"];
				g.DrawImage(i, monkey.Bounds.Left - 16, monkey.Bounds.Top);
			}
		}

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = false;
			GraphicsState gs = e.Graphics.Save();
			Draw(e.Graphics, e.Node as Monkey, e.Bounds);
			e.Graphics.Restore(gs);
			base.OnDrawNode(e);
		}

		Brush GetBrush(Function function)
		{
			return (filter == null || function == null || !filter(function)) ?
				Brushes.Black : Brushes.Gray;
		}
	}
}