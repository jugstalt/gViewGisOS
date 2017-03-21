namespace gView.Framework.GeoProcessing.UI
{
    partial class DataSelectorControl
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
            this.gbDataName = new System.Windows.Forms.GroupBox();
            this.importMethodCtrl = new gView.Framework.GeoProcessing.UI.FeatureImportMethodControl();
            this.cmbData = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.gbDataName.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDataName
            // 
            this.gbDataName.Controls.Add(this.importMethodCtrl);
            this.gbDataName.Controls.Add(this.cmbData);
            this.gbDataName.Controls.Add(this.btnAdd);
            this.gbDataName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDataName.Location = new System.Drawing.Point(0, 0);
            this.gbDataName.Name = "gbDataName";
            this.gbDataName.Size = new System.Drawing.Size(407, 240);
            this.gbDataName.TabIndex = 0;
            this.gbDataName.TabStop = false;
            this.gbDataName.Text = "groupBox1";
            // 
            // importMethodCtrl
            // 
            this.importMethodCtrl.DatasetElement = null;
            this.importMethodCtrl.FilterClause = "";
            this.importMethodCtrl.Location = new System.Drawing.Point(7, 46);
            this.importMethodCtrl.Name = "importMethodCtrl";
            this.importMethodCtrl.QueryMethod = gView.Framework.GeoProcessing.QueryMethod.All;
            this.importMethodCtrl.Size = new System.Drawing.Size(382, 189);
            this.importMethodCtrl.TabIndex = 2;
            this.importMethodCtrl.FilterClauseChanged += new System.EventHandler(this.importMethodCtrl_FilterClauseChanged);
            this.importMethodCtrl.QueryMethodChanged += new System.EventHandler(this.importMethodCtrl_QueryMethodChanged);
            // 
            // cmbData
            // 
            this.cmbData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbData.FormattingEnabled = true;
            this.cmbData.Location = new System.Drawing.Point(6, 19);
            this.cmbData.Name = "cmbData";
            this.cmbData.Size = new System.Drawing.Size(338, 21);
            this.cmbData.TabIndex = 1;
            this.cmbData.SelectedIndexChanged += new System.EventHandler(this.cmbData_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAdd.Image = global::gView.GeoProcessing.UI.Properties.Resources.add_16;
            this.btnAdd.Location = new System.Drawing.Point(350, 19);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(51, 21);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // DataSelectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbDataName);
            this.Name = "DataSelectorControl";
            this.Size = new System.Drawing.Size(407, 240);
            this.gbDataName.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDataName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ComboBox cmbData;
        private FeatureImportMethodControl importMethodCtrl;
    }
}
