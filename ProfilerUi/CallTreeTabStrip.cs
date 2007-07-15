using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace ProfilerUi
{
	class CallTreeTabStrip : Control
	{
		readonly List<Tab> tabs = new List<Tab>();
		readonly TabIterator iterator;

		public TabIterator Iterator { get { return iterator; } }
		public int Count { get { return tabs.Count; } }

		CloseBox closeBox;

		public event EventHandler Changed = delegate { };

		public CallTreeTabStrip()
		{
			BackColor = SystemColors.ButtonFace;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();

			closeBox = new CloseBox(this);
			closeBox.CloseClicked += delegate { CloseCurrent(); };
			iterator = new TabIterator(this);
		}

		public IEnumerable<CallTreeView> CallTrees
		{
			get
			{
				foreach (Tab tab in tabs)
					yield return tab.CallTree;
			}
		}

		Tab GetTab(CallTreeView callTree)
		{
			if (callTree == null)
				throw new ArgumentNullException("document");

			foreach (Tab tab in tabs)
				if (tab.CallTree == callTree)
					return tab;

			return null;
		}

		internal Tab GetTab(int index)
		{
			if (index < 0)
				return null;

			return tabs[index];
		}

		public CallTreeView CurrentCallTree
		{
			get
			{
				if (iterator.Current == null)
					return null;
				return iterator.Current.CallTree;
			}
		}

		public void Add(CallTreeView callTree)
		{
			Tab tab = GetTab(callTree);
			if (tab == null)
				tabs.Add(tab = new Tab(callTree, this));

			Changed(this, EventArgs.Empty);
			SelectCallTree( callTree );
		}

		public void SelectCallTree( CallTreeView callTree )
		{
			Tab tab = GetTab( callTree );
			if( tab != null )
				iterator.Current = tab;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;

			ControlPaint.DrawBorder(g,
				ClientRectangle,
				SystemColors.ButtonShadow,
				ButtonBorderStyle.Solid);

			int x = 1;
			foreach (Tab d in tabs)
				d.Paint(g, ref x, iterator.Current == d, ClientRectangle);

			closeBox.Paint(g);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			Tab tab = GetTab(e.Location);
			if (tab == null)
				return;

			switch (e.Button)
			{
				case MouseButtons.Left:
					iterator.Current = tab;
					break;
				case MouseButtons.Middle:
					CloseCallTree(tab.CallTree);
					break;

				default:
					break;
			}
		}

		public bool CloseAll()
		{
			tabs.Clear();
			iterator.Current = null;
			Changed(this, EventArgs.Empty);
			return true;
		}

		public void CloseCallTree(CallTreeView callTree)
		{
			if (callTree == null)
				return;
			Tab tab = GetTab(callTree);
			tabs.Remove(tab);
			tab.Dispose();
			Changed(this, EventArgs.Empty);
			Invalidate();
		}

		Tab GetTab(Point p)
		{
			foreach (Tab t in tabs)
				if (t.Bounds.Contains(p))
					return t;

			return null;
		}

		internal int IndexOf(Tab t)
		{
			return tabs.IndexOf(t);
		}

		public void CloseCurrent()
		{
			CloseCallTree(CurrentCallTree);
		}
	}
}
