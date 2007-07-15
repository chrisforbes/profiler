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

		public string Key
		{
			get
			{
				if (IsPropGetter) return "prop_get";
				if (IsPropSetter) return "prop_set";
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
			}
			catch { }
		}
	}
}
