namespace gView.Framework.Offline.UI
{
    partial class FormCheckout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCheckout));
            this.label1 = new System.Windows.Forms.Label();
            this.txtCheckoutDescription = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtTargetClass = new System.Windows.Forms.TextBox();
            this.txtDatasetLocation = new System.Windows.Forms.TextBox();
            this.txtDatasetName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCheckout = new System.Windows.Forms.Button();
            this.btnScript = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkDELETE = new System.Windows.Forms.CheckBox();
            this.chkUPDATE = new System.Windows.Forms.CheckBox();
            this.chkINSERT = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioNewerWins = new System.Windows.Forms.RadioButton();
            this.radioChildWins = new System.Windows.Forms.RadioButton();
            this.radioParentWins = new System.Windows.Forms.RadioButton();
            this.radioNormalConflict = new System.Windows.Forms.RadioButton();
            this.radioNoConflict = new System.Windows.Forms.RadioButton();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            // txtCheckoutDescription
            // 
            this.txtCheckoutDescription.AccessibleDescription = null;
            this.txtCheckoutDescription.AccessibleName = null;
            resources.ApplyResources(this.txtCheckoutDescription, "txtCheckoutDescription");
            this.txtCheckoutDescription.BackgroundImage = null;
            this.txtCheckoutDescription.Font = null;
            this.txtCheckoutDescription.Name = "txtCheckoutDescription";
            // 
            // groupBox2
            // 
            this.groupBox2.AccessibleDescription = null;
            this.groupBox2.AccessibleName = null;
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.BackgroundImage = null;
            this.groupBox2.Controls.Add(this.btnSelect);
            this.groupBox2.Controls.Add(this.txtTargetClass);
            this.groupBox2.Controls.Add(this.txtDatasetLocation);
            this.groupBox2.Controls.Add(this.txtDatasetName);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Font = null;
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnSelect
            // 
            this.btnSelect.AccessibleDescription = null;
            this.btnSelect.AccessibleName = null;
            resources.ApplyResources(this.btnSelect, "btnSelect");
            this.btnSelect.BackgroundImage = null;
            this.btnSelect.Font = null;
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtTargetClass
            // 
            this.txtTargetClass.AccessibleDescription = null;
            this.txtTargetClass.AccessibleName = null;
            resources.ApplyResources(this.txtTargetClass, "txtTargetClass");
            this.txtTargetClass.BackgroundImage = null;
            this.txtTargetClass.Font = null;
            this.txtTargetClass.Name = "txtTargetClass";
            // 
            // txtDatasetLocation
            // 
            this.txtDatasetLocation.AccessibleDescription = null;
            this.txtDatasetLocation.AccessibleName = null;
            resources.ApplyResources(this.txtDatasetLocation, "txtDatasetLocation");
            this.txtDatasetLocation.BackgroundImage = null;
            this.txtDatasetLocation.Font = null;
            this.txtDatasetLocation.Name = "txtDatasetLocation";
            // 
            // txtDatasetName
            // 
            this.txtDatasetName.AccessibleDescription = null;
            this.txtDatasetName.AccessibleName = null;
            resources.ApplyResources(this.txtDatasetName, "txtDatasetName");
            this.txtDatasetName.BackgroundImage = null;
            this.txtDatasetName.Font = null;
            this.txtDatasetName.Name = "txtDatasetName";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Font = null;
            this.label4.Name = "label4";
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
            // btnCheckout
            // 
            this.btnCheckout.AccessibleDescription = null;
            this.btnCheckout.AccessibleName = null;
            resources.ApplyResources(this.btnCheckout, "btnCheckout");
            this.btnCheckout.BackgroundImage = null;
            this.btnCheckout.Font = null;
            this.btnCheckout.Name = "btnCheckout";
            this.btnCheckout.UseVisualStyleBackColor = true;
            this.btnCheckout.Click += new System.EventHandler(this.btnCheckout_Click);
            // 
            // btnScript
            // 
            this.btnScript.AccessibleDescription = null;
            this.btnScript.AccessibleName = null;
            resources.ApplyResources(this.btnScript, "btnScript");
            this.btnScript.BackgroundImage = null;
            this.btnScript.Font = null;
            this.btnScript.Name = "btnScript";
            this.btnScript.UseVisualStyleBackColor = true;
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.chkDELETE);
            this.groupBox1.Controls.Add(this.chkUPDATE);
            this.groupBox1.Controls.Add(this.chkINSERT);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // chkDELETE
            // 
            this.chkDELETE.AccessibleDescription = null;
            this.chkDELETE.AccessibleName = null;
            resources.ApplyResources(this.chkDELETE, "chkDELETE");
            this.chkDELETE.BackgroundImage = null;
            this.chkDELETE.Checked = true;
            this.chkDELETE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDELETE.Font = null;
            this.chkDELETE.Name = "chkDELETE";
            this.chkDELETE.UseVisualStyleBackColor = true;
            // 
            // chkUPDATE
            // 
            this.chkUPDATE.AccessibleDescription = null;
            this.chkUPDATE.AccessibleName = null;
            resources.ApplyResources(this.chkUPDATE, "chkUPDATE");
            this.chkUPDATE.BackgroundImage = null;
            this.chkUPDATE.Checked = true;
            this.chkUPDATE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUPDATE.Font = null;
            this.chkUPDATE.Name = "chkUPDATE";
            this.chkUPDATE.UseVisualStyleBackColor = true;
            // 
            // chkINSERT
            // 
            this.chkINSERT.AccessibleDescription = null;
            this.chkINSERT.AccessibleName = null;
            resources.ApplyResources(this.chkINSERT, "chkINSERT");
            this.chkINSERT.BackgroundImage = null;
            this.chkINSERT.Checked = true;
            this.chkINSERT.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkINSERT.Font = null;
            this.chkINSERT.Name = "chkINSERT";
            this.chkINSERT.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.AccessibleDescription = null;
            this.groupBox3.AccessibleName = null;
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.BackgroundImage = null;
            this.groupBox3.Controls.Add(this.radioNewerWins);
            this.groupBox3.Controls.Add(this.radioChildWins);
            this.groupBox3.Controls.Add(this.radioParentWins);
            this.groupBox3.Controls.Add(this.radioNormalConflict);
            this.groupBox3.Controls.Add(this.radioNoConflict);
            this.groupBox3.Font = null;
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // radioNewerWins
            // 
            this.radioNewerWins.AccessibleDescription = null;
            this.radioNewerWins.AccessibleName = null;
            resources.ApplyResources(this.radioNewerWins, "radioNewerWins");
            this.radioNewerWins.BackgroundImage = null;
            this.radioNewerWins.Font = null;
            this.radioNewerWins.Name = "radioNewerWins";
            this.radioNewerWins.UseVisualStyleBackColor = true;
            // 
            // radioChildWins
            // 
            this.radioChildWins.AccessibleDescription = null;
            this.radioChildWins.AccessibleName = null;
            resources.ApplyResources(this.radioChildWins, "radioChildWins");
            this.radioChildWins.BackgroundImage = null;
            this.radioChildWins.Font = null;
            this.radioChildWins.Name = "radioChildWins";
            this.radioChildWins.UseVisualStyleBackColor = true;
            // 
            // radioParentWins
            // 
            this.radioParentWins.AccessibleDescription = null;
            this.radioParentWins.AccessibleName = null;
            resources.ApplyResources(this.radioParentWins, "radioParentWins");
            this.radioParentWins.BackgroundImage = null;
            this.radioParentWins.Font = null;
            this.radioParentWins.Name = "radioParentWins";
            this.radioParentWins.UseVisualStyleBackColor = true;
            // 
            // radioNormalConflict
            // 
            this.radioNormalConflict.AccessibleDescription = null;
            this.radioNormalConflict.AccessibleName = null;
            resources.ApplyResources(this.radioNormalConflict, "radioNormalConflict");
            this.radioNormalConflict.BackgroundImage = null;
            this.radioNormalConflict.Checked = true;
            this.radioNormalConflict.Font = null;
            this.radioNormalConflict.Name = "radioNormalConflict";
            this.radioNormalConflict.TabStop = true;
            this.radioNormalConflict.UseVisualStyleBackColor = true;
            // 
            // radioNoConflict
            // 
            this.radioNoConflict.AccessibleDescription = null;
            this.radioNoConflict.AccessibleName = null;
            resources.ApplyResources(this.radioNoConflict, "radioNoConflict");
            this.radioNoConflict.BackgroundImage = null;
            this.radioNoConflict.Font = null;
            this.radioNoConflict.Name = "radioNoConflict";
            this.radioNoConflict.UseVisualStyleBackColor = true;
            // 
            // FormCheckout
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnScript);
            this.Controls.Add(this.btnCheckout);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtCheckoutDescription);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormCheckout";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCheckoutDescription;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtTargetClass;
        private System.Windows.Forms.TextBox txtDatasetLocation;
        private System.Windows.Forms.TextBox txtDatasetName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCheckout;
        private System.Windows.Forms.Button btnScript;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkDELETE;
        private System.Windows.Forms.CheckBox chkUPDATE;
        private System.Windows.Forms.CheckBox chkINSERT;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioParentWins;
        private System.Windows.Forms.RadioButton radioNormalConflict;
        private System.Windows.Forms.RadioButton radioNoConflict;
        private System.Windows.Forms.RadioButton radioChildWins;
        private System.Windows.Forms.RadioButton radioNewerWins;
    }
}