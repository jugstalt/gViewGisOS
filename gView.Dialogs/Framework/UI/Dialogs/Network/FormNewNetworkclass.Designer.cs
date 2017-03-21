namespace gView.Framework.UI.Dialogs.Network
{
    partial class FormNewNetworkclass
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
            this.wizardControl1 = new gView.Framework.UI.Controls.Wizard.WizardControl();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Size = new System.Drawing.Size(720, 482);
            this.wizardControl1.TabIndex = 0;
            // 
            // FormNewNetworkclass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 482);
            this.Controls.Add(this.wizardControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormNewNetworkclass";
            this.Text = "New Network Class";
            this.Load += new System.EventHandler(this.FormNewNetworkclass_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.Wizard.WizardControl wizardControl1;

    }
}