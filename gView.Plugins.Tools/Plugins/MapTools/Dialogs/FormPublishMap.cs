using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormPublishMap : Form
    {
        public FormPublishMap()
        {
            InitializeComponent();
        }

        public string Server { get { return txtServer.Text; } set { txtServer.Text = value; } }
        public int Port { get { return (int)numPort.Value; } set { numPort.Value = value; } }

        public string Username { get { return txtUserName.Text; } set { txtUserName.Text = value; } }
        public string Password { get { return txtPassword.Text; } set { txtPassword.Text = value; } }

        public string ServiceName { get { return txtServiceName.Text; } set { txtServiceName.Text = value; } }
    }
}
