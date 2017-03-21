using System;
using System.Collections.Generic;
using System.Windows.Forms;
using gView.Framework.system.UI;
using System.Threading;
using gView.Framework.system;

namespace gView.Desktop.Offline.Sync
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new Form1());
        }

        static private void SplashScreen(object splash)
        {
            if (splash is Form)
                ((Form)splash).ShowDialog();
        }
        static private void Sleeper(object splash)
        {
            Thread.Sleep(1000);
            ((Form)splash).Close();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;

            string msg = "A problem has occured in this application:\r\n\r\n" +
                "\tMessage: " + ex.Message + "\r\n\tSource: " + ex.Source + "\r\n\r\n" +
                "Would you like to continue the application so that\r\n" +
                "you save your work?";

            if (MessageBox.Show(msg, "Expected Error", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                Application.Exit();
            }
        }
    }
}