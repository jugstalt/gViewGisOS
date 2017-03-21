namespace gView.Framework.GeoProcessing.UI.ActivityBase
{
    partial class BufferControl
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDouble = new gView.Framework.UI.Controls.NumericTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txtDouble);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(322, 62);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Buffer";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Distance:";
            // 
            // txtDouble
            // 
            this.txtDouble.AllowNegative = true;
            this.txtDouble.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDouble.DataType = gView.Framework.UI.Controls.NumericTextBox.NumericDataType.doubleType;
            this.txtDouble.DigitsInGroup = 0;
            this.txtDouble.Flags = 0;
            this.txtDouble.Location = new System.Drawing.Point(65, 26);
            this.txtDouble.MaxDecimalPlaces = 4;
            this.txtDouble.MaxWholeDigits = 9;
            this.txtDouble.Name = "txtDouble";
            this.txtDouble.Prefix = "";
            this.txtDouble.RangeMax = 1.7976931348623157E+308;
            this.txtDouble.RangeMin = -1.7976931348623157E+308;
            this.txtDouble.Size = new System.Drawing.Size(251, 20);
            this.txtDouble.TabIndex = 1;
            this.txtDouble.Text = "100";
            this.txtDouble.TextChanged += new System.EventHandler(this.txtDouble_TextChanged);
            // 
            // BufferControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "BufferControl";
            this.Size = new System.Drawing.Size(328, 276);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private gView.Framework.UI.Controls.NumericTextBox txtDouble;
        private System.Windows.Forms.Label label1;
    }
}
