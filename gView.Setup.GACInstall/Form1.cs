using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.EnterpriseServices.Internal;
using Microsoft.Win32;
using System.Management;

namespace gView.Setup.GACInstall
{
    public partial class Form1 : Form
    {
        private bool _install;

        public Form1(bool install)
        {
            InitializeComponent();

            _install = install;
            if (_install)
                this.Text = "Install GAC Assemblies";
            else
                this.Text = "Uninstall GAC Assemblies";
        }

        static internal string RegistryApplicationDirectory
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gViewGisOS", false);
                    string ret = key.GetValue("Install_Dir").ToString();
                    key.Close();

                    return ret;
                }
                catch
                {
                    return "";
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Publish p = new Publish();
            
            
            string appPath = Form1.RegistryApplicationDirectory;

            try
            {
                if (_install)
                {
                    System.Environment.SetEnvironmentVariable("GVIEW4_HOME", appPath + @"\", EnvironmentVariableTarget.Machine);
                }
                else
                {
                    System.Environment.SetEnvironmentVariable("GVIEW4_HOME", String.Empty, EnvironmentVariableTarget.Machine);
                }
            }
            catch { }

            DirectoryInfo di = new DirectoryInfo(Path.Combine(appPath, "Framework"));

            FileInfo[] dlls = di.GetFiles("*.dll");

            progressBar1.Minimum = 0;
            progressBar1.Maximum = dlls.Length;
            progressBar1.Value = 0;

            foreach (FileInfo fi in dlls)
            {
                lblAssembly.Text = fi.FullName;
                this.Refresh();

                if (_install)
                    p.GacInstall(fi.FullName);
                else
                    p.GacRemove(fi.FullName);

                progressBar1.Value++;
                this.Refresh();

                System.Threading.Thread.Sleep(50);
            }

            // Set Envirment Variables
            string PATH = System.Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            string PATH2 = String.Empty;
            bool foundPath = false;
            foreach (string path in PATH.Split(';'))
            {
                if (path.ToLower() != appPath.ToLower())
                {
                    if (PATH2 != String.Empty) PATH2 += ";";
                    PATH2 += path;
                }
                else
                {
                    foundPath = true;
                }
            }

            if (_install)
            {
                if (!foundPath)
                {
                    if (PATH2 != String.Empty) PATH2 += ";";
                    PATH2 += appPath;

                    System.Environment.SetEnvironmentVariable("PATH", PATH2);
                }
            }
            else
            {
                if (foundPath)
                {
                    System.Environment.SetEnvironmentVariable("PATH", PATH2, EnvironmentVariableTarget.Machine);
                }
            }

            // MapServer.Monitor Service installieren
            //try
            //{
            //    string filename = RegistryApplicationDirectory + @"\gView.MapServer.Monitor.exe";
            //    FileInfo fi = new FileInfo(filename);
            //    if (fi.Exists)
            //    {
            //        if (_install)
            //        {
            //            UInt32 result = (UInt32)mc.InvokeMethod("Install", new object[] { filename });
            //        }
            //        else
            //        {
            //            UInt32 result = (UInt32)mc.InvokeMethod("Uninstall", new object[] { filename });
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Can't " + (_install ? "install" : "uninstall") + " gView.MapServer.Monitor Service...\n" + ex.Message, "Warning");
            //}

            this.Close();
        }
    }
}