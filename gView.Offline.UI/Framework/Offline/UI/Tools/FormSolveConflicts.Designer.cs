namespace gView.Framework.Offline.UI.Tools
{
    partial class FormSolveConflicts
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSolveConflicts));
            this.tvConflicts = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.conflictControl1 = new gView.Framework.Offline.UI.Tools.ConflictControl();
            this.btnSolve = new System.Windows.Forms.ToolStripButton();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.btnHighlight = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvConflicts
            // 
            this.tvConflicts.AccessibleDescription = null;
            this.tvConflicts.AccessibleName = null;
            resources.ApplyResources(this.tvConflicts, "tvConflicts");
            this.tvConflicts.BackgroundImage = null;
            this.tvConflicts.Font = null;
            this.tvConflicts.HideSelection = false;
            this.tvConflicts.Name = "tvConflicts";
            this.tvConflicts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvConflicts_AfterSelect);
            // 
            // splitter1
            // 
            this.splitter1.AccessibleDescription = null;
            this.splitter1.AccessibleName = null;
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.BackgroundImage = null;
            this.splitter1.Font = null;
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AccessibleDescription = null;
            this.toolStrip1.AccessibleName = null;
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.BackgroundImage = null;
            this.toolStrip1.Font = null;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSolve,
            this.btnRemove,
            this.toolStripSeparator1,
            this.btnHighlight});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AccessibleDescription = null;
            this.toolStripSeparator1.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // conflictControl1
            // 
            this.conflictControl1.AccessibleDescription = null;
            this.conflictControl1.AccessibleName = null;
            resources.ApplyResources(this.conflictControl1, "conflictControl1");
            this.conflictControl1.BackColor = System.Drawing.Color.White;
            this.conflictControl1.BackgroundImage = null;
            this.conflictControl1.Conflict = null;
            this.conflictControl1.Font = null;
            this.conflictControl1.Name = "conflictControl1";
            // 
            // btnSolve
            // 
            this.btnSolve.AccessibleDescription = null;
            this.btnSolve.AccessibleName = null;
            resources.ApplyResources(this.btnSolve, "btnSolve");
            this.btnSolve.BackgroundImage = null;
            this.btnSolve.Image = global::gView.Offline.UI.Properties.Resources.arrow_join;
            this.btnSolve.Name = "btnSolve";
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.AccessibleDescription = null;
            this.btnRemove.AccessibleName = null;
            resources.ApplyResources(this.btnRemove, "btnRemove");
            this.btnRemove.BackgroundImage = null;
            this.btnRemove.Image = global::gView.Offline.UI.Properties.Resources.arrow_divide;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnHighlight
            // 
            this.btnHighlight.AccessibleDescription = null;
            this.btnHighlight.AccessibleName = null;
            resources.ApplyResources(this.btnHighlight, "btnHighlight");
            this.btnHighlight.BackgroundImage = null;
            this.btnHighlight.Image = global::gView.Offline.UI.Properties.Resources.asterisk_yellow;
            this.btnHighlight.Name = "btnHighlight";
            this.btnHighlight.Click += new System.EventHandler(this.btnHighlight_Click);
            // 
            // FormSolveConflicts
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.conflictControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tvConflicts);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Name = "FormSolveConflicts";
            this.Load += new System.EventHandler(this.FormSolveConflicts_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvConflicts;
        private System.Windows.Forms.Splitter splitter1;
        private ConflictControl conflictControl1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSolve;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnHighlight;
    }
}