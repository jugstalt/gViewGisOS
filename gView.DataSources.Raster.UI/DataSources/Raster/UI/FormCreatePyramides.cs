using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.DataSources.Raster.UI
{
    public partial class FormCreatePyramides : Form
    {
        public FormCreatePyramides()
        {
            InitializeComponent();
        }

        private void btnGetImages_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtSource.Text = openFileDialog1.FileName;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            gView.DataSources.Raster.File.Pyramid pyramid = new gView.DataSources.Raster.File.Pyramid();

            pyramid.Create(txtSource.Text, txtSource.Text + ".mdb", 4);
        }
    }
}