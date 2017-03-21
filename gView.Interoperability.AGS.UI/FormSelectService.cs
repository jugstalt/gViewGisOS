using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Interoperability.AGS.Dataset;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Interoperability.AGS.Proxy;
using gView.Interoperability.AGS.Helper;
using gView.Framework.Web;

namespace gView.Interoperability.AGS.UI
{
    public partial class FormSelectService : Form,IModalDialog,IConnectionString
    {
        public FormSelectService()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                lstServices.Items.Clear();
                foreach (ServiceDescription sd in ArcServerHelper.MapServerServices(txtServer.Text, ProxySettings.Proxy("http://" + txtServer.Text)))
                {
                    lstServices.Items.Add(new ServiceDescriptionItem(sd));
                }
            }
            catch(Exception ex)
            {
                Cursor = Cursors.Default;

                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #region IModalDialog Member

        public bool OpenModal()
        {
            if (this.ShowDialog() == DialogResult.OK)
                return true;

            return false;
        }

        #endregion

        #region IConnectionString Member

        private string _connectionString = "";
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text;
            if (chkRouteThroughUser.Checked) username = "$";
            if (chkRouteThroughRole.Checked) username = "#";

            _connectionString = "server=" + txtServer.Text + 
                ";user=" + username + 
                ";pwd=" + txtPwd.Text + ";service=" + ((ServiceDescriptionItem)lstServices.SelectedItems[0]).ServiceDescription.Url;
        }

        private void lstServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (lstServices.SelectedItems.Count > 0);
        }

        private void chkRouteThroughUser_CheckedChanged(object sender, EventArgs e)
        {
            txtUser.Enabled = txtPwd.Enabled = !chkRouteThroughUser.Checked;
        }

        private void chkRouteThroughRole_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRouteThroughRole.Checked)
            {
                chkRouteThroughUser.Checked = true;
            }
        }

        #region Item Classes
        private class ServiceDescriptionItem : ListViewItem
        {
            private ServiceDescription _sd;

            public ServiceDescriptionItem(ServiceDescription sd)
            {
                _sd = sd;
            }

            public ServiceDescription ServiceDescription
            {
                get
                {
                    return _sd;
                }
            }
            public override string ToString()
            {
                return _sd.Name;
            }
        }
        #endregion
    }
}