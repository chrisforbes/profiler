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

		bool IsPropGetter(TreeNode n) { return n.Text.Contains("get_"); }
		bool IsPropSetter(TreeNode n) { return n.Text.Contains("set_"); }

		string GetEffectiveName(TreeNode n)
		{
			if (IsPropGetter(n))
				return n.Text.Replace("get_", "");

			if (IsPropSetter(n))
				return n.Text.Replace("set_", "");

			return n.Text;
		}

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = false;

			GraphicsState gs = e.Graphics.Save();
			e.Graphics.Clip = new Region(new Rectangle(new Point(0, e.Node.Bounds.Top), new Size(Width, e.Node.Bounds.Height)));

			e.Graphics.FillRectangle((e.Node == SelectedNode) ? selected : SystemBrushes.Window, e.Bounds);

			Brush textBrush = GetBrush(e.Node.Tag as Function);
			Point p = e.Node.Bounds.Location;

			if (e.Node.Tag is Thread)
			{
				e.Graphics.DrawImage(images.Images["thread"], e.Node.Bounds.Left, e.Node.Bounds.Top);
				p.Offset(16, 0);
			}
			else
			{
				string img = IsPropGetter(e.Node) ? "prop_get" : IsPropSetter(e.Node) ? "prop_set" : "method";
				e.Graphics.DrawImage(images.Images[img], e.Node.Bounds.Left, e.Node.Bounds.Top);
				p.Offset(16, 0);
			}

			p.Offset(2, 1);
			e.Graphics.DrawString(GetEffectiveName( e.Node ), Font, textBrush, p, StringFormat.GenericTypographic);

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