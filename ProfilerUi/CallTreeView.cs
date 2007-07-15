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
		Predicate<Function> filter;

		string text;

		public override string Text
		{
			get { return text; }
			set { text = value; }
		}

		public CallTreeView( Predicate<Function> filter )
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			this.filter = filter;
		}

		Brush selected = new SolidBrush(Color.FromArgb(0xee, 0xee, 0xff));

		void Draw(Graphics g, Node monkey, Rectangle bounds)
		{
			g.Clip = new Region(new Rectangle(new Point(0, monkey.Bounds.Top), new Size(Width, monkey.Bounds.Height)));
			g.FillRectangle((monkey == SelectedNode) ? selected : SystemBrushes.Window, bounds);

			ItemPainter painter = new ItemPainter(g, monkey.Bounds.Location);
			painter.DrawImage(Node.images.Images[monkey.Key]);
			painter.Pad(2);
			painter.DrawText(monkey.EffectiveName, Font, GetBrush(monkey.Element as Function), 1);

			if (monkey.Nodes.Count > 0)
			{
				Image i = Node.images.Images[monkey.IsExpanded ? "expanded" : "collapsed"];
				g.DrawImage(i, monkey.Bounds.Left - 16, monkey.Bounds.Top);
			}
		}

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = false;
			GraphicsState gs = e.Graphics.Save();
			Draw(e.Graphics, e.Node as Node, e.Bounds);
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
