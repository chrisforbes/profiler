using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ProfilerUi
{
	class Node : TreeNode
	{
		public IProfilerElement Element { get { return Tag as IProfilerElement; } }
		static Regex r = new Regex(@"`\d+");

		public Node(IProfilerElement e, string text)
			: base(r.Replace( text, "<>"))
		{
			Tag = e;
		}

		bool IsPropGetter { get { return Text.Contains("get_"); } }
		bool IsPropSetter { get { return Text.Contains("set_"); } }
		bool IsEventAdd { get { return Text.Contains("add_"); } }
		bool IsEventRemove { get { return Text.Contains("remove_"); } }

		public string Key
		{
			get
			{
				if (IsPropGetter) return "prop_get";
				if (IsPropSetter) return "prop_set";
				if (IsEventAdd) return "event_add";
				if (IsEventRemove) return "event_remove";
				if (Tag is Thread) return "thread";
				return "method";
			}
		}

		public string EffectiveName
		{
			get
			{
				if (IsPropGetter) return Text.Replace("get_", "");
				if (IsPropSetter) return Text.Replace("set_", "");
				if (IsEventAdd) return Text.Replace("add_", "");
				if (IsEventRemove) return Text.Replace("remove_", "");
				return Text;
			}
		}

		public string TabName
		{
			get
			{
				int start = EffectiveName.IndexOf("::") + 2;
				return EffectiveName.Substring(start, EffectiveName.IndexOf(' ') - start); 
			}
		}

		public Node RootFunction
		{
			get
			{
				if (!(Element is Function))
					return null;

				Node n = this;

				while (n != null && n.Parent != null)
					n = n.Parent as Node;

				return n;
			}
		}

		internal static ImageList images = new ImageList();
		
		static Node()
		{
			try
			{
				images.TransparentColor = Color.Fuchsia;
				images.Images.Add("collapsed", Image.FromFile("res/collapsed.bmp"));
				images.Images.Add("expanded", Image.FromFile("res/expanded.bmp"));
				images.Images.Add("thread", Image.FromFile("res/thread.bmp"));
				images.Images.Add("prop_set", Image.FromFile("res/prop_set.bmp"));
				images.Images.Add("prop_get", Image.FromFile("res/prop_get.bmp"));
				images.Images.Add("method", Image.FromFile("res/method.bmp"));
				images.Images.Add("app", Image.FromFile("res/app.bmp"));
				images.Images.Add("event_add", Image.FromFile("res/event_add.bmp"));
				images.Images.Add("event_remove", Image.FromFile("res/event_remove.bmp"));
			}
			catch { }
		}
	}
}
