using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace gView.Setup.Uninstall
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                while (true)
                {
                    int procCount1 = Process.GetProcessesByName("gView.Desktop.Wpf.DataExplorer").Length;
                    int procCount2 = Process.GetProcessesByName("gView.Desktop.Wpf.Carto").Length;
                    int procCount3 = Process.GetProcessesByName("gView.Desktop.DataExplorer").Length;
                    int procCount4 = Process.GetProcessesByName("gView.Desktop.Carto").Length;

                    if (procCount1 == 0 && procCount2 == 0 && procCount3 == 0 && procCount4 == 0)
                        break;

                    String msg = String.Empty;
                    if (procCount1 > 0) msg += "\ngView.Desktop.Wpf.DataExplorer (" + procCount1 + ")";
                    if (procCount2 > 0) msg += "\ngView.Desktop.Wpf.Carto (" + procCount2 + ")";
                    if (procCount3 > 0) msg += "\ngView.Desktop.DataExplorer (" + procCount3 + ")";
                    if (procCount4 > 0) msg += "\ngView.Desktop.Carto (" + procCount4 + ")";

                    MessageBox.Show("Please close alle gView programs before continue..." + msg, "Warining", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                FileInfo[] dllsExists = (new DirectoryInfo(path)).GetFiles("gview.*.dll");
                FileInfo mapServerAdmin = new FileInfo(path + @"\gView.Desktop.MapServer.Admin.exe");
                if (mapServerAdmin.Exists && dllsExists != null && dllsExists.Length > 0)
                    RunExe(mapServerAdmin.FullName, "-service_stop");

                FileInfo gacInstall = new FileInfo(path + @"\gView.Setup.GACInstall.exe");
                if (gacInstall.Exists)
                    RunExe(gacInstall.FullName, "-u");

            }
            catch { }
            finally
            {
                this.Close();
            }
        }

        private void RunExe(string fileName, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;

            proc.Start();
            proc.WaitForExit();
        }
    }
}
