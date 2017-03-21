using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Xml;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Framework.system.UI;
using System.Reflection;

namespace gView.Setup.PostInstallation
{
    public partial class Form1 : Form
    {
        private Thread _thread;
        private bool _silent=false;

        public Form1(bool silent)
        {
            _silent = silent;

            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ParseAssemblies parser = new ParseAssemblies();
            parser.ParseAssembly += new ParseAssemblies.ParseAssembyEvent(parser_ParseAssembly);
            //parser.AddComponent += new ParseAssemblies.AddComponentEvent(parser_AddComponent);
            parser.AddPlugin += new ParseAssemblies.AddPluginEvent(parser_AddPlugin);
            parser.AddPluginException += new ParseAssemblies.AddPluginExceptionEvent(parser_AddPluginException);
            parser.Finished += new ParseAssemblies.FinishedEvent(parser_Finished);
            //parser.Parse(SystemVariables.RegistryApplicationDirectory);

            _thread = new Thread(new ParameterizedThreadStart(parser.Parse));

            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (String.IsNullOrEmpty(SystemVariables.StartupDirectory))
            {
                if (!_silent)
                {
                    if (MessageBox.Show("Can't find register key:\nLocalMachine/Software/gViewGisOS/Install_Dir\n\nDo you want to set this Key to: " + path, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        this.Close();
                        return;
                    }
                }
                SystemVariables.RegistryApplicationDirectory = path;
            }
            else if (path.ToLower() != SystemVariables.StartupDirectory.ToLower())
            {
                if (!_silent)
                {
                    if (MessageBox.Show("Current application path '" + SystemVariables.StartupDirectory + "' differs from your current Assembly-Path:\n\n" + path + "\n\nWould you like to change application path to current path?",
                        "Information",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SystemVariables.RegistryApplicationDirectory = path;
                    }
                }
                else
                {
                    SystemVariables.RegistryApplicationDirectory = path;
                }
            }

            _thread.Start(SystemVariables.StartupDirectory);
        }

        delegate void parser_FinishedCallback();
        void parser_Finished()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new parser_FinishedCallback(parser_Finished));
            }
            else
            {
                FileInfo fi = new FileInfo(SystemVariables.CommonApplicationData + @"\mapServer\MapServerConfig.xml");
                if (!fi.Exists)
                {
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();

                    FileInfo fi2 = new FileInfo(SystemVariables.ApplicationDirectory + @"\MapServerConfig.Setup.xml");
                    if (fi2.Exists)
                        fi2.CopyTo(fi.FullName);
                }

                Thread.Sleep(1000);
                this.Close();

                //Application.Exit();
            }
        }

        private delegate void parser_ParseAssemblyCallback(string Name, int pos, int max);
        void parser_ParseAssembly(string Name, int pos, int max)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(
                    new parser_ParseAssemblyCallback(parser_ParseAssembly),
                    new object[] { Name, pos, max });
            }
            else
            {
                txtAssembly.Text = Name;
                progressBar1.Maximum = max + 1;
                progressBar1.Value = pos;
            }
        }

        private delegate void parser_AddComponentCallback(string Name);
        void parser_AddComponent(string Name)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(
                    new parser_AddComponentCallback(parser_AddComponent),
                    new object[] { Name });
            }
            else
            {
                //txtComponent.Text = Name;
            }
        }

        private delegate void parser_AddPluginCallback(string Name, Type type);
        void parser_AddPlugin(string Name, Type type)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(
                    new parser_AddPluginCallback(parser_AddPlugin),
                    new object[] { Name, type });
            }
            else
            {
                txtComponent.Text = Name;
            }
        }

        private delegate void parser_AddPluginExceptionCallback(object sender,AddPluginExceptionEventArgs args);
        void parser_AddPluginException(object sender, AddPluginExceptionEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(
                    new parser_AddPluginExceptionCallback(parser_AddPluginException),
                    new object[] { sender, args });
            }
            else
            {
                if (MessageBox.Show(
                    args.Type.FullName + ":\n" + 
                    args.Exception.Message + "\n" + 
                    args.Exception.StackTrace + "\n\nIgnore this plug-in and continue?",
                    "Error",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    args.Cancel = true;
                    MessageBox.Show("Installation canceled by the user...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}