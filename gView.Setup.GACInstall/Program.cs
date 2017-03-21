using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gView.Setup.GACInstall
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 0) return;
            switch (args[0].ToLower())
            {
                case "-i":
                    Application.Run(new Form1(true));
                    break;
                case "-u":
                    Application.Run(new Form1(false));
                    break;
            }
        }
    }
}