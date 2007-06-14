using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ProfilerUi
{
	class CallTreeView : TreeView
	{
		public CallTreeView()
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();
		}

		Brush selected = new SolidBrush(Color.FromArgb(0xee, 0xee, 0xff));
		Font fbold;

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			if (e.Node == SelectedNode)
				e.Graphics.FillRectangle(selected, e.Bounds);

			e.DrawDefault = true;

			base.OnDrawNode(e);
		}
	}
}