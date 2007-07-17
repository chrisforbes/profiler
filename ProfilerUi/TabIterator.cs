using System;
using System.Collections.Generic;
using System.Text;

namespace ProfilerUi
{
	class TabIterator<T>
		where T : class
	{
		int index;
		TabStrip<T> outer;
		Tab<T> current;

		public TabIterator(TabStrip<T> outer)
		{
			this.outer = outer;
			outer.Changed += delegate { Update(); };
		}

		void Update()
		{
			int newIndex = outer.IndexOf(current);
			if (newIndex >= 0)
				index = newIndex;
			else
			{
				Current = outer.GetTab(Math.Min(index, outer.Count - 1));
				index = outer.IndexOf(current);
			}
		}

		public void MoveNext()
		{
			index = (index + 1 < outer.Count) ? index + 1 : 0;
			Current = outer.GetTab(index);

			outer.Invalidate();
		}

		public void MovePrevious()
		{
			index = (index > 0) ? index - 1 : outer.Count - 1;
			Current = outer.GetTab(index);

			outer.Invalidate();
		}

		public Tab<T> Current
		{
			get { return current; }
			set
			{
				index = outer.IndexOf(value);
				current = value;
				outer.Invalidate();
				Changed(this, EventArgs.Empty);
			}
		}

		public event EventHandler Changed = delegate { };
	}
}
