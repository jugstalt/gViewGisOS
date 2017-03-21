using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gView.Setup.PostInstallation
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool silent = false;

            if (args != null) {
                foreach (string arg in args)
                {
                    if (arg == "-s" || arg == "-silent")
                        silent = true;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(silent));
        }
    }
}