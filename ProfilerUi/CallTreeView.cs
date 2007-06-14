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

		public CallTreeView()
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			images.TransparentColor = Color.Fuchsia;
			images.Images.Add("collapsed", Image.FromFile("res/collapsed.bmp"));
			images.Images.Add("expanded", Image.FromFile("res/expanded.bmp"));
		}

		Brush selected = new SolidBrush(Color.FromArgb(0xee, 0xee, 0xff));

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = false;

			GraphicsState gs = e.Graphics.Save();
			e.Graphics.Clip = new Region( new Rectangle( new Point(0, e.Node.Bounds.Top), new Size( Width, e.Node.Bounds.Height )));

			if (e.Node == SelectedNode)
				e.Graphics.FillRectangle(selected, e.Bounds);

			Brush textBrush = GetBrush(e.Node.Tag as Function);
			Point p = e.Node.Bounds.Location;
			p.Offset(2, 1);
			e.Graphics.DrawString(e.Node.Text, Font, textBrush, p, StringFormat.GenericTypographic);

			if (e.Node.Nodes.Count > 0)
			{
				Image i = images.Images[e.Node.IsExpanded ? "expanded" : "collapsed"];
				e.Graphics.DrawImage(i, e.Node.Bounds.Left - 16, e.Node.Bounds.Top);
			}

			e.Graphics.Restore(gs);
			base.OnDrawNode(e);
		}

		Brush GetBrush(Function function)
		{
			if (filter == null || function == null)
				return Brushes.Black;

			return filter.IsFiltered(function) ? Brushes.Gray : Brushes.Black;
		}

		FunctionFilter filter = null;

		[DefaultValue(null)]
		public FunctionFilter Filter
		{
			get { return filter; }
			set { filter = value; Invalidate(); }
		}
	}
}