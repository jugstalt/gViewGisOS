namespace gView.Framework.GeoProcessing.UI
{
    partial class FeatureImportMethodControl
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnAllFeatures = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblSelection = new System.Windows.Forms.Label();
            this.filterFeaturesControl1 = new FilterFeaturesControl();
            this.btnFilterFeatures = new System.Windows.Forms.RadioButton();
            this.btnSelectedFeatures = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAllFeatures
            // 
            this.btnAllFeatures.AutoSize = true;
            this.btnAllFeatures.Location = new System.Drawing.Point(14, 18);
            this.btnAllFeatures.Name = "btnAllFeatures";
            this.btnAllFeatures.Size = new System.Drawing.Size(80, 17);
            this.btnAllFeatures.TabIndex = 0;
            this.btnAllFeatures.TabStop = true;
            this.btnAllFeatures.Text = "All Features";
            this.btnAllFeatures.UseVisualStyleBackColor = true;
            this.btnAllFeatures.CheckedChanged += new System.EventHandler(this.btnAllFeatures_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblSelection);
            this.groupBox1.Controls.Add(this.filterFeaturesControl1);
            this.groupBox1.Controls.Add(this.btnFilterFeatures);
            this.groupBox1.Controls.Add(this.btnSelectedFeatures);
            this.groupBox1.Controls.Add(this.btnAllFeatures);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(355, 181);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Read Method";
            // 
            // lblSelection
            // 
            this.lblSelection.AutoSize = true;
            this.lblSelection.Location = new System.Drawing.Point(130, 42);
            this.lblSelection.Name = "lblSelection";
            this.lblSelection.Size = new System.Drawing.Size(63, 13);
            this.lblSelection.TabIndex = 4;
            this.lblSelection.Text = "(0 Features)";
            // 
            // filterFeaturesControl1
            // 
            this.filterFeaturesControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filterFeaturesControl1.FilterClause = "";
            this.filterFeaturesControl1.Location = new System.Drawing.Point(29, 56);
            this.filterFeaturesControl1.Name = "filterFeaturesControl1";
            this.filterFeaturesControl1.Size = new System.Drawing.Size(318, 121);
            this.filterFeaturesControl1.TabIndex = 3;
            this.filterFeaturesControl1.TableClass = null;
            this.filterFeaturesControl1.FilterClauseChanged += new System.EventHandler(this.filterFeaturesControl1_FilterClauseChanged);
            // 
            // btnFilterFeatures
            // 
            this.btnFilterFeatures.AutoSize = true;
            this.btnFilterFeatures.Location = new System.Drawing.Point(14, 64);
            this.btnFilterFeatures.Name = "btnFilterFeatures";
            this.btnFilterFeatures.Size = new System.Drawing.Size(14, 13);
            this.btnFilterFeatures.TabIndex = 2;
            this.btnFilterFeatures.TabStop = true;
            this.btnFilterFeatures.UseVisualStyleBackColor = true;
            this.btnFilterFeatures.CheckedChanged += new System.EventHandler(this.btnFilterFeatures_CheckedChanged);
            // 
            // btnSelectedFeatures
            // 
            this.btnSelectedFeatures.AutoSize = true;
            this.btnSelectedFeatures.Location = new System.Drawing.Point(14, 40);
            this.btnSelectedFeatures.Name = "btnSelectedFeatures";
            this.btnSelectedFeatures.Size = new System.Drawing.Size(111, 17);
            this.btnSelectedFeatures.TabIndex = 1;
            this.btnSelectedFeatures.TabStop = true;
            this.btnSelectedFeatures.Text = "Selected Features";
            this.btnSelectedFeatures.UseVisualStyleBackColor = true;
            this.btnSelectedFeatures.CheckedChanged += new System.EventHandler(this.btnSelectedFeatures_CheckedChanged);
            // 
            // FeatureImportMethodControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "FeatureImportMethodControl";
            this.Size = new System.Drawing.Size(370, 319);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton btnAllFeatures;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btnFilterFeatures;
        private System.Windows.Forms.RadioButton btnSelectedFeatures;
        private FilterFeaturesControl filterFeaturesControl1;
        private System.Windows.Forms.Label lblSelection;
    }
}
