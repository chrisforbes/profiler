using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace ProfilerUi
{
	partial class AboutBox : Form
	{
		public AboutBox( string product, string version, string copyright )
		{
			InitializeComponent();

			this.Text = String.Format("About {0}", product);
			this.labelProductName.Text = product;
			this.labelVersion.Text = String.Format("Version {0}", version);
			this.labelCopyright.Text = copyright;
		}
	}
}
