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
		readonly Predicate<string> filter;
		readonly string text;
		public readonly CallTree src;

		public override string ToString() { return text; }

		public CallTreeView( Predicate<string> filter, string text, CallTree src )
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			this.filter = filter;
			this.text = text;
			this.src = src;
		}

		Brush selected = new SolidBrush(Color.FromArgb(0xee, 0xee, 0xff));

		void Draw(Graphics g, Node node, Rectangle bounds)
		{
			g.Clip = new Region(new Rectangle(new Point(0, node.Bounds.Top), new Size(Width, node.Bounds.Height)));
			g.FillRectangle((node == SelectedNode) ? selected : SystemBrushes.Window, bounds);

			ItemPainter painter = new ItemPainter(g, node.Bounds.Location);
			painter.DrawImage(Node.images.Images[node.Key]);
			painter.Pad(2);
			painter.DrawText(node.EffectiveName, Font, GetBrush(node.Element as Function), 1);

			if (node.Nodes.Count > 0)
			{
				Image i = Node.images.Images[node.IsExpanded ? "expanded" : "collapsed"];
				g.DrawImage(i, node.Bounds.Left - 16, node.Bounds.Top);
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
			return (filter == null || function == null || !filter(function.name)) ?
				Brushes.Black : Brushes.Gray;
		}
	}
}
