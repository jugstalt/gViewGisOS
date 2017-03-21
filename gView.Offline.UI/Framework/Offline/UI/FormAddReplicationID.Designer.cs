namespace gView.Framework.Offline.UI
{
    partial class FormAddReplicationID
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddReplicationID));
            this.label1 = new System.Windows.Forms.Label();
            this.txtFieldname = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblCounter = new System.Windows.Forms.Label();
            this.progressDisk1 = new gView.Framework.Offline.UI.ProgressDisk();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // txtFieldname
            // 
            this.txtFieldname.AccessibleDescription = null;
            this.txtFieldname.AccessibleName = null;
            resources.ApplyResources(this.txtFieldname, "txtFieldname");
            this.txtFieldname.BackgroundImage = null;
            this.txtFieldname.Font = null;
            this.txtFieldname.Name = "txtFieldname";
            // 
            // btnCreate
            // 
            this.btnCreate.AccessibleDescription = null;
            this.btnCreate.AccessibleName = null;
            resources.ApplyResources(this.btnCreate, "btnCreate");
            this.btnCreate.BackgroundImage = null;
            this.btnCreate.Font = null;
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblCounter
            // 
            this.lblCounter.AccessibleDescription = null;
            this.lblCounter.AccessibleName = null;
            resources.ApplyResources(this.lblCounter, "lblCounter");
            this.lblCounter.Font = null;
            this.lblCounter.Name = "lblCounter";
            // 
            // progressDisk1
            // 
            this.progressDisk1.AccessibleDescription = null;
            this.progressDisk1.AccessibleName = null;
            resources.ApplyResources(this.progressDisk1, "progressDisk1");
            this.progressDisk1.BackgroundImage = null;
            this.progressDisk1.BlockSize = gView.Framework.Offline.UI.BlockSize.Large;
            this.progressDisk1.Font = null;
            this.progressDisk1.Name = "progressDisk1";
            this.progressDisk1.SquareSize = 106;
            // 
            // FormAddReplicationID
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.lblCounter);
            this.Controls.Add(this.progressDisk1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtFieldname);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormAddReplicationID";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormAddReplicationID_FormClosing);
            this.Load += new System.EventHandler(this.FormAddReplicationID_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFieldname;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblCounter;
        private gView.Framework.Offline.UI.ProgressDisk progressDisk1;
    }
}