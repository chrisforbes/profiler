namespace ProfilerUi
{
	partial class Form1
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
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button3 = new System.Windows.Forms.Button();
			this.callTreeView1 = new ProfilerUi.CallTreeView();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(109, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "New profiling run";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.NewRun);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(127, 12);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(109, 23);
			this.button2.TabIndex = 0;
			this.button2.Text = "Load last run";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.LoadLastRun);
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(242, 14);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(312, 20);
			this.textBox1.TabIndex = 2;
			this.textBox1.Text = "System.*, Microsoft.*";
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.Location = new System.Drawing.Point(560, 12);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 3;
			this.button3.Text = "Refresh";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.UpdateFilter);
			// 
			// callTreeView1
			// 
			this.callTreeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.callTreeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
			this.callTreeView1.Location = new System.Drawing.Point(12, 41);
			this.callTreeView1.Name = "callTreeView1";
			this.callTreeView1.Size = new System.Drawing.Size(623, 389);
			this.callTreeView1.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(647, 442);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.callTreeView1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.DoubleBuffered = true;
			this.Name = "Form1";
			this.Text = "IJW Profiler 0.2";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private CallTreeView callTreeView1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button3;
	}
}

