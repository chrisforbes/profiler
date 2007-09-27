using System.IO;
namespace Ijw.Profiler.UI
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
				components.Dispose();

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.workspace = new System.Windows.Forms.ToolStripContainer();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openInNewTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showBacktraces = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.nextTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.previousTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showHotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.workspace.TopToolStripPanel.SuspendLayout();
			this.workspace.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// workspace
			// 
			// 
			// workspace.ContentPanel
			// 
			this.workspace.ContentPanel.Size = new System.Drawing.Size(647, 418);
			this.workspace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workspace.Location = new System.Drawing.Point(0, 0);
			this.workspace.Name = "workspace";
			this.workspace.Size = new System.Drawing.Size(647, 442);
			this.workspace.TabIndex = 0;
			this.workspace.Text = "toolStripContainer1";
			// 
			// workspace.TopToolStripPanel
			// 
			this.workspace.TopToolStripPanel.Controls.Add(this.menuStrip1);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(647, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSessionToolStripMenuItem,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newSessionToolStripMenuItem
			// 
			this.newSessionToolStripMenuItem.Name = "newSessionToolStripMenuItem";
			this.newSessionToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.newSessionToolStripMenuItem.Text = "&New Session...";
			this.newSessionToolStripMenuItem.Click += new System.EventHandler(this.NewRun);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.toolStripMenuItem2.Size = new System.Drawing.Size(190, 22);
			this.toolStripMenuItem2.Text = "Save Snapshot";
			this.toolStripMenuItem2.Click += new System.EventHandler(this.OnSave);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
			this.toolStripMenuItem3.Size = new System.Drawing.Size(190, 22);
			this.toolStripMenuItem3.Text = "Close";
			this.toolStripMenuItem3.Click += new System.EventHandler(this.CloseTab);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnCloseClicked);
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInNewTabToolStripMenuItem,
            this.showBacktraces,
            this.showHotToolStripMenuItem,
            this.toolStripSeparator1,
            this.nextTabToolStripMenuItem,
            this.previousTabToolStripMenuItem});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.viewToolStripMenuItem.Text = "&View";
			// 
			// openInNewTabToolStripMenuItem
			// 
			this.openInNewTabToolStripMenuItem.Name = "openInNewTabToolStripMenuItem";
			this.openInNewTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this.openInNewTabToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.openInNewTabToolStripMenuItem.Text = "&Open in new tab";
			this.openInNewTabToolStripMenuItem.Click += new System.EventHandler(this.OnOpenInNewTab);
			// 
			// showBacktraces
			// 
			this.showBacktraces.Name = "showBacktraces";
			this.showBacktraces.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
			this.showBacktraces.Size = new System.Drawing.Size(228, 22);
			this.showBacktraces.Text = "&Show Callers";
			this.showBacktraces.Click += new System.EventHandler(this.ShowBacktraces);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(225, 6);
			// 
			// nextTabToolStripMenuItem
			// 
			this.nextTabToolStripMenuItem.Name = "nextTabToolStripMenuItem";
			this.nextTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Tab)));
			this.nextTabToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.nextTabToolStripMenuItem.Text = "&Next Tab";
			this.nextTabToolStripMenuItem.Click += new System.EventHandler(this.GoToNextTab);
			// 
			// previousTabToolStripMenuItem
			// 
			this.previousTabToolStripMenuItem.Name = "previousTabToolStripMenuItem";
			this.previousTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
						| System.Windows.Forms.Keys.Tab)));
			this.previousTabToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.previousTabToolStripMenuItem.Text = "&Previous Tab";
			this.previousTabToolStripMenuItem.Click += new System.EventHandler(this.GoToPreviousTab);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.About);
			// 
			// checkForUpdatesToolStripMenuItem
			// 
			this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
			this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.checkForUpdatesToolStripMenuItem.Text = "&Check for Updates...";
			this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.CheckForUpdates);
			// 
			// showHotToolStripMenuItem
			// 
			this.showHotToolStripMenuItem.Name = "showHotToolStripMenuItem";
			this.showHotToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.showHotToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.showHotToolStripMenuItem.Text = "Show &Hotspots";
			this.showHotToolStripMenuItem.Click += new System.EventHandler(this.ShowHotspots);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(647, 442);
			this.Controls.Add(this.workspace);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.workspace.TopToolStripPanel.ResumeLayout(false);
			this.workspace.TopToolStripPanel.PerformLayout();
			this.workspace.ResumeLayout(false);
			this.workspace.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer workspace;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newSessionToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openInNewTabToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem nextTabToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem previousTabToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showBacktraces;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem showHotToolStripMenuItem;
		

	}
}

