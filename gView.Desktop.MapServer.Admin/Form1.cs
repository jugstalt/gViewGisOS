using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Reflection;
using System.Xml;
using gView.Framework.system;

namespace gView.Desktop.MapServer.Admin
{
    public partial class Form1 : Form
    {
        private bool _installed = false;
        private ServiceControllerStatus _serviceStatus = ServiceControllerStatus.Stopped;

        public Form1()
        {
            InitializeComponent();

            cmbStartType.Items.Add(ServiceStartMode.Automatic);
            cmbStartType.Items.Add(ServiceStartMode.Manual);
            cmbStartType.Items.Add(ServiceStartMode.Disabled);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MakeGUI(true);
        }

        private void MakeGUI(bool reportErrors)
        {
            #region Tab Status
            try
            {
                ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                sc.Refresh();
                _serviceStatus = sc.Status;

                lblStatus.Text = "Service Status: " + _serviceStatus.ToString();
                _installed = true;
                btnInstall.Text = "Uninstall Windows Service...";

                switch (_serviceStatus)
                {
                    case ServiceControllerStatus.Stopped:
                        btnStart.Text = "Start Service";
                        btnStart.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_arrow_right;
                        picStatus.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_square_red;
                        break;
                    case ServiceControllerStatus.Running:
                        btnStart.Text = "Stop Service";
                        btnStart.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_square_red;
                        picStatus.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_arrow_right;
                        break;
                }
                btnStart.Enabled = true;
                btnInstall.Visible = false;

                ServiceStartMode mode = ServiceHelper.GetServiceStartMode("gView.MapServer.Tasker");
                cmbStartType.SelectedItem = mode;
            }
            catch (Exception ex)
            {
                if (reportErrors) MessageBox.Show(lblStatus.Text = ex.Message, "Error");
                btnStart.Enabled = false;
                btnInstall.Text = "Install Windows Service...";
                btnInstall.Visible = true;
                picStatus.Image = global::gView.Desktop.MapServer.Admin.Properties.Resources._16_message_warn;
            }
            #endregion

            #region Tab Config
            btnRefreshConfig_Click(this, new EventArgs());
            #endregion

            #region Tab Logging
            try
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\gView.MapServer.Instance.exe.config";
                XmlDocument config = new XmlDocument();
                config.Load(path);

                XmlNode node = config.SelectSingleNode("configuration/appSettings/add[@key='log_requests']");
                chkLogRequests.Enabled = (node != null);
                chkLogRequests.Checked = (node == null || node.Attributes["value"] == null) ?
                    false : node.Attributes["value"].Value.ToLower() == "true";

                node = config.SelectSingleNode("configuration/appSettings/add[@key='log_request_details']");
                chkLogRequestDetails.Enabled = (node != null);
                chkLogRequestDetails.Checked = (node == null || node.Attributes["value"] == null) ?
                    false : node.Attributes["value"].Value.ToLower() == "true";

                node = config.SelectSingleNode("configuration/appSettings/add[@key='log_errors']");
                chkLogErrors.Enabled = (node != null);
                chkLogErrors.Checked = (node == null || node.Attributes["value"] == null) ?
                    false : node.Attributes["value"].Value.ToLower() == "true";

                chkLogRequests.CheckedChanged += new EventHandler(chkLog_CheckedChanged);
                chkLogRequestDetails.CheckedChanged += new EventHandler(chkLog_CheckedChanged);
                chkLogErrors.CheckedChanged += new EventHandler(chkLog_CheckedChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
            }
            #endregion
        }

        void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\gView.MapServer.Instance.exe.config";
                XmlDocument config = new XmlDocument();
                config.Load(path);

                string key = "";
                if (sender == chkLogRequests) key = "log_requests";
                else if (sender == chkLogRequestDetails) key = "log_request_details";
                else if (sender == chkLogErrors) key = "log_errors";

                XmlNode node = config.SelectSingleNode("configuration/appSettings/add[@key='" + key + "']");
                if (node.Attributes["value"] != null)
                    node.Attributes["value"].Value = ((CheckBox)sender).Checked.ToString().ToLower();

                config.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            FormInstallService dlg = new FormInstallService(_installed == true ? FormInstallService.Action.Unsinstall : FormInstallService.Action.Install);

            dlg.ShowDialog();
            if (_installed == true && dlg.Succeeded)
            {
                _installed = false;
                btnStart.Enabled = false;
                btnInstall.Text = "Install Windows Service...";
            }
            else
            {
                MakeGUI(false);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                sc.Refresh();

                TimeSpan ts = new TimeSpan(0, 0, 10);
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, ts);
                        break;
                    case ServiceControllerStatus.Running:
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, ts);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            MakeGUI(true);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            MakeGUI(false);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnRefreshConfig_Click(object sender, EventArgs e)
        {
            MapServerConfig.Load();
            txtOutputUrl.Text = MapServerConfig.DefaultOutputUrl;
            txtOutputPath.Text = MapServerConfig.DefaultOutputPath;
            txtTilecachePath.Text = MapServerConfig.DefaultTileCachePath;
            txtOnlineresource.Text = MapServerConfig.DefaultOnlineResource;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", SystemVariables.CommonApplicationData + @"\mapServer\MapServerConfig.xml");
        }

        private void btnChangeStartType_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Change service start type to '" + cmbStartType.SelectedItem.ToString() + "'?", "Service Properties", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                sc.Refresh();

                ServiceHelper.ChangeStartMode(sc, (ServiceStartMode)cmbStartType.SelectedItem);
                MakeGUI(false);
            }
        }
    }
}