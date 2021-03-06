namespace Ijw.Profiler.UI
{
	partial class NewRunDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			this.applicationBox = new System.Windows.Forms.TextBox();
			this.workingDirectoryBox = new System.Windows.Forms.TextBox();
			this.argumentsBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.startProfilingButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.agentBox = new System.Windows.Forms.ComboBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 29);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(59, 13);
			label1.TabIndex = 3;
			label1.Text = "Application";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 55);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(95, 13);
			label2.TabIndex = 4;
			label2.Text = "Working Directory:";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 81);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(60, 13);
			label3.TabIndex = 5;
			label3.Text = "Arguments:";
			// 
			// applicationBox
			// 
			this.applicationBox.Location = new System.Drawing.Point(115, 26);
			this.applicationBox.Name = "applicationBox";
			this.applicationBox.Size = new System.Drawing.Size(324, 20);
			this.applicationBox.TabIndex = 0;
			// 
			// workingDirectoryBox
			// 
			this.workingDirectoryBox.Location = new System.Drawing.Point(115, 52);
			this.workingDirectoryBox.Name = "workingDirectoryBox";
			this.workingDirectoryBox.Size = new System.Drawing.Size(355, 20);
			this.workingDirectoryBox.TabIndex = 1;
			// 
			// argumentsBox
			// 
			this.argumentsBox.Location = new System.Drawing.Point(115, 78);
			this.argumentsBox.Name = "argumentsBox";
			this.argumentsBox.Size = new System.Drawing.Size(355, 20);
			this.argumentsBox.TabIndex = 2;
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(445, 26);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 20);
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// startProfilingButton
			// 
			this.startProfilingButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.startProfilingButton.Location = new System.Drawing.Point(280, 146);
			this.startProfilingButton.Name = "startProfilingButton";
			this.startProfilingButton.Size = new System.Drawing.Size(108, 23);
			this.startProfilingButton.TabIndex = 4;
			this.startProfilingButton.Text = "Start Profiling";
			this.startProfilingButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(394, 146);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(12, 107);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(38, 13);
			label4.TabIndex = 5;
			label4.Text = "Agent:";
			// 
			// agentBox
			// 
			this.agentBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.agentBox.FormattingEnabled = true;
			this.agentBox.Location = new System.Drawing.Point(115, 105);
			this.agentBox.Name = "agentBox";
			this.agentBox.Size = new System.Drawing.Size(212, 21);
			this.agentBox.TabIndex = 6;
			// 
			// NewRunDialog
			// 
			this.AcceptButton = this.startProfilingButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(483, 181);
			this.Controls.Add(this.agentBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.startProfilingButton);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(label4);
			this.Controls.Add(label3);
			this.Controls.Add(label2);
			this.Controls.Add(label1);
			this.Controls.Add(this.argumentsBox);
			this.Controls.Add(this.workingDirectoryBox);
			this.Controls.Add(this.applicationBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "NewRunDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Session...";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox applicationBox;
		private System.Windows.Forms.TextBox workingDirectoryBox;
		private System.Windows.Forms.TextBox argumentsBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Button startProfilingButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ComboBox agentBox;
	}
}