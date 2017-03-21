namespace gView.Framework.GeoProcessing.UI
{
    partial class FormGeoProcessor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGeoProcessor));
            this.wpActivity = new IDE.Controls.WizardPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picActivity = new System.Windows.Forms.PictureBox();
            this.tvActivity = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.wpSource = new IDE.Controls.WizardPage();
            this.wpTarget = new IDE.Controls.WizardPage();
            this.wpProperties = new IDE.Controls.WizardPage();
            this.wpActivity.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picActivity)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardControl
            // 
            this.wizardControl.SelectedIndex = 0;
            this.wizardControl.Size = new System.Drawing.Size(493, 486);
            this.wizardControl.Title = "gView Geoprocessor";
            this.wizardControl.WizardPages.AddRange(new IDE.Controls.WizardPage[] {
            this.wpActivity,
            this.wpSource,
            this.wpTarget,
            this.wpProperties});
            // 
            // wpActivity
            // 
            this.wpActivity.CaptionTitle = "(Page Title)";
            this.wpActivity.Controls.Add(this.groupBox1);
            this.wpActivity.FullPage = false;
            this.wpActivity.Location = new System.Drawing.Point(0, 0);
            this.wpActivity.Name = "wpActivity";
            this.wpActivity.Size = new System.Drawing.Size(493, 335);
            this.wpActivity.SubTitle = "";
            this.wpActivity.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picActivity);
            this.groupBox1.Controls.Add(this.tvActivity);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(493, 335);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Activity";
            // 
            // picActivity
            // 
            this.picActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picActivity.Location = new System.Drawing.Point(252, 19);
            this.picActivity.Name = "picActivity";
            this.picActivity.Size = new System.Drawing.Size(238, 313);
            this.picActivity.TabIndex = 2;
            this.picActivity.TabStop = false;
            // 
            // tvActivity
            // 
            this.tvActivity.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvActivity.HideSelection = false;
            this.tvActivity.ImageIndex = 0;
            this.tvActivity.ImageList = this.imageList1;
            this.tvActivity.Location = new System.Drawing.Point(3, 19);
            this.tvActivity.Name = "tvActivity";
            this.tvActivity.SelectedImageIndex = 0;
            this.tvActivity.Size = new System.Drawing.Size(249, 313);
            this.tvActivity.TabIndex = 4;
            this.tvActivity.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvActivity_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "24-tools.png");
            this.imageList1.Images.SetKeyName(1, "24-tool-b.png");
            // 
            // wpSource
            // 
            this.wpSource.CaptionTitle = "(Page Title)";
            this.wpSource.FullPage = false;
            this.wpSource.Location = new System.Drawing.Point(0, 0);
            this.wpSource.Name = "wpSource";
            this.wpSource.Selected = false;
            this.wpSource.Size = new System.Drawing.Size(493, 335);
            this.wpSource.SubTitle = "(Page Description not defined)";
            this.wpSource.TabIndex = 1;
            // 
            // wpTarget
            // 
            this.wpTarget.CaptionTitle = "(Page Title)";
            this.wpTarget.FullPage = false;
            this.wpTarget.Location = new System.Drawing.Point(0, 0);
            this.wpTarget.Name = "wpTarget";
            this.wpTarget.Selected = false;
            this.wpTarget.Size = new System.Drawing.Size(493, 335);
            this.wpTarget.SubTitle = "(Page Description not defined)";
            this.wpTarget.TabIndex = 2;
            // 
            // wpProperties
            // 
            this.wpProperties.CaptionTitle = "(Page Title)";
            this.wpProperties.FullPage = false;
            this.wpProperties.Location = new System.Drawing.Point(0, 0);
            this.wpProperties.Name = "wpProperties";
            this.wpProperties.Selected = false;
            this.wpProperties.Size = new System.Drawing.Size(493, 335);
            this.wpProperties.SubTitle = "(Page Description not defined)";
            this.wpProperties.TabIndex = 3;
            // 
            // FormGeoProcessor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 486);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormGeoProcessor";
            this.Text = "Geoprocessing Wizard";
            this.wpActivity.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picActivity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private IDE.Controls.WizardPage wpActivity;
        private System.Windows.Forms.PictureBox picActivity;
        private IDE.Controls.WizardPage wpSource;
        private IDE.Controls.WizardPage wpTarget;
        private System.Windows.Forms.GroupBox groupBox1;
        private IDE.Controls.WizardPage wpProperties;
        private System.Windows.Forms.TreeView tvActivity;
        private System.Windows.Forms.ImageList imageList1;
    }
}