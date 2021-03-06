using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ijw.Profiler.Core;

namespace Ijw.Profiler.UI
{
	partial class NewRunDialog : Form
	{
		public NewRunDialog( RunParameters initialParameters, AgentLoader loader )
		{
			InitializeComponent();

			agentBox.DisplayMember = "Name";
			foreach (IAgent agent in loader.Agents)
				agentBox.Items.Add(agent);
			agentBox.SelectedItem = loader.Default;

			if (initialParameters != null)
			{
				applicationBox.Text = initialParameters.exePath;
				workingDirectoryBox.Text = initialParameters.workingDirectory;
				argumentsBox.Text = initialParameters.parameters;
				agentBox.SelectedItem = initialParameters.agent;
			}
		}

		void browseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Programs (*.exe)|*.exe";
			ofd.RestoreDirectory = true;
			ofd.FileName = applicationBox.Text;

			if (DialogResult.OK == ofd.ShowDialog())
			{
				applicationBox.Text = ofd.FileName;
				workingDirectoryBox.Text = Path.GetDirectoryName(ofd.FileName);
			}
		}

		public RunParameters Parameters
		{
			get
			{
				return new RunParameters(
					applicationBox.Text,
					workingDirectoryBox.Text,
					argumentsBox.Text,
					agentBox.SelectedItem as IAgent);
			}
		}
	}
}