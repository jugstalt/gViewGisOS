using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.DataSources.Shape.UI
{
	/// <summary>
	/// Zusammenfassung für FormAddShapefile.
	/// </summary>
	internal class FormAddShapefile : System.Windows.Forms.Form,gView.Framework.Data.IDatasetEnum
	{
		private System.Windows.Forms.Button getFile;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.TextBox txtShp;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FormAddShapefile()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.getFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtShp = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // getFile
            // 
            this.getFile.Location = new System.Drawing.Point(525, 23);
            this.getFile.Name = "getFile";
            this.getFile.Size = new System.Drawing.Size(64, 35);
            this.getFile.TabIndex = 0;
            this.getFile.Text = "...";
            this.getFile.Click += new System.EventHandler(this.getFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Shapefile (*.shp)|*.shp";
            // 
            // txtShp
            // 
            this.txtShp.Location = new System.Drawing.Point(13, 23);
            this.txtShp.Name = "txtShp";
            this.txtShp.Size = new System.Drawing.Size(512, 26);
            this.txtShp.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAdd.Location = new System.Drawing.Point(384, 82);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(205, 47);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "OK";
            // 
            // FormAddShapefile
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
            this.ClientSize = new System.Drawing.Size(633, 225);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtShp);
            this.Controls.Add(this.getFile);
            this.Name = "FormAddShapefile";
            this.Text = "FormAddShapefile";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void getFile_Click(object sender, System.EventArgs e)
		{
			if(openFileDialog1.ShowDialog()==DialogResult.OK) 
			{
				txtShp.Text=openFileDialog1.FileName;
			}
		}


		#region IDatasetEnum Member

		public int _enumPos=0;
		public void Reset()
		{
			_enumPos=0;
		}

		public gView.Framework.Data.IDataset Next
		{
			get
			{
				if(_enumPos>0) return null;

				IDataset dataset=new ShapeDataset();
				dataset.ConnectionString=txtShp.Text;
				dataset.Open();
				_enumPos++;

				return dataset;
			}
		}

		#endregion
	}
}
