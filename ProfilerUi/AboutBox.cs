using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using IjwFramework;

namespace Ijw.Profiler.UI
{
	partial class AboutBox : Form
	{
		public AboutBox()
		{
			InitializeComponent();

			this.Text = String.Format("About {0}", Product.Name);
			this.labelProductName.Text = Product.Name;
			this.labelVersion.Text = String.Format("Version {0}", Product.ShortVersion);
			this.labelCopyright.Text = Product.Copyright;
		}
	}
}
