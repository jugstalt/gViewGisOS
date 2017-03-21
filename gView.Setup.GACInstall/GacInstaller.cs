using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.EnterpriseServices.Internal;
using Microsoft.Win32;

namespace gView.Setup.GACInstall
{
    [RunInstaller(true)]
    public partial class GacInstaller : Installer
    {
        public GacInstaller()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            InstallProc(true);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            InstallProc(false);
        }

        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);

            InstallProc(false);
        }

        private void InstallProc(bool install)
        {
            Publish p = new Publish();

            string appPath = this.RegistryApplicationDirectory;

            try
            {
                if (install)
                {
                    System.Environment.SetEnvironmentVariable("GVIEW4_HOME", appPath, EnvironmentVariableTarget.Machine);
                }
                else
                {
                    System.Environment.SetEnvironmentVariable("GVIEW4_HOME", String.Empty, EnvironmentVariableTarget.Machine);
                }
            }
            catch { }

            DirectoryInfo di = new DirectoryInfo(Path.Combine(appPath, "Framework"));

            FileInfo[] dlls = di.GetFiles("*.dll");


            foreach (FileInfo fi in dlls)
            {
                if (install)
                    p.GacInstall(fi.FullName);
                else
                    p.GacRemove(fi.FullName);
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

            if (install)
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
        }

        private string RegistryApplicationDirectory
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
    }
}