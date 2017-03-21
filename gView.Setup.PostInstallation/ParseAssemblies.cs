using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using gView.Framework.system.UI;
using Microsoft.Win32;

namespace gView.Setup.PostInstallation
{
    internal class ParseAssemblies
    {
        public delegate void ParseAssembyEvent(string Name, int pos, int max);
        public event ParseAssembyEvent ParseAssembly = null;
        public delegate void FinishedEvent();
        public event FinishedEvent Finished = null;
        public delegate void AddComponentEvent(string Name);
        public event AddComponentEvent AddComponent = null;
        public delegate void AddPluginEvent(string Name, Type type);
        public event AddPluginEvent AddPlugin = null;
        public delegate void AddPluginExceptionEvent(object sender, AddPluginExceptionEventArgs args);
        public event AddPluginExceptionEvent AddPluginException = null;
        public List<string> _assemblies = new List<string>();

        public void Parse(object path)
        {
            if (!(path is string) || String.IsNullOrEmpty((string)path))
            {
                MessageBox.Show("Can't read installation path from Registry\nLocalMachine/Software/gViewGisOS/Install_Dir", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DirectoryInfo di;
            try
            {
                di = new DirectoryInfo((string)path);
                if (!di.Exists) return;
            }
            catch { return; }

            AssemblyExplorer explorer = new AssemblyExplorer();
            explorer.AddPlugin += new AssemblyExplorer.AddPluginEvent(explorer_AddPlugin);
            explorer.AddPluginException += new AssemblyExplorer.AddPluginExceptionEvent(explorer_AddPluginException);

            FileInfo[] fis = di.GetFiles("*.dll");

            int pos = 1;
            foreach (FileInfo ass in fis)
            {
                if (ParseAssembly != null) ParseAssembly(ass.FullName, pos, fis.Length);

                string xml = explorer.Explore(ass.FullName);

                if (!String.IsNullOrEmpty(xml))
                {
                    try
                    {
                        addComponent(ass.FullName);
                        if (AddComponent != null) AddComponent(ass.FullName);
                        writeComponents();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (xml == null) // Canceled...
                {
                    break;
                }
                pos++;
            }
            if (Finished != null) Finished();
        }

        void explorer_AddPluginException(object sender, AddPluginExceptionEventArgs args)
        {
            if (AddPluginException != null)
                AddPluginException(sender, args);
        }

        void explorer_AddPlugin(string Name, Type type)
        {
            if (AddPlugin != null)
                AddPlugin(Name, type);
        }

        private void writeComponents()
        {
            gView.Framework.system.UI.AssemblyExplorer explorer = new gView.Framework.system.UI.AssemblyExplorer();

            //// Refresh Components
            string str = String.Empty;
            //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\gView\assemblies", false);
            //if (key == null) return;

            foreach (string assembly in _assemblies)
            {
                str += explorer.Explore(assembly) + "\n";
            }
            //key.Close();

            FileInfo fi = new FileInfo(gView.Framework.system.SystemVariables.MyCommonApplicationData + @"\gViewGisOS_plugins.xml");
            if (!fi.Directory.Exists)
                fi.Directory.Create();

            StreamWriter sw = new StreamWriter(fi.FullName);
            sw.WriteLine("<components>\n" + str + "</components>");
            sw.Close();
        }

        private void addComponent(string assemblyPath)
        {
            _assemblies.Add(assemblyPath);
        }
    }
}
