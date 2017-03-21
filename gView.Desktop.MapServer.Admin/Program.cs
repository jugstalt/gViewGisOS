using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gView.Desktop.MapServer.Admin
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length != 0)
            {
                switch (args[0].ToLower())
                {
                    case "-service_install":
                        Application.Run(new FormInstallService(FormInstallService.Action.Install));
                        return;
                    case "-service_install_forced":
                        Application.Run(new FormInstallService(FormInstallService.Action.Install_forced));
                        return;
                    case "-service_automatic":
                        Application.Run(new FormInstallService(FormInstallService.Action.SetAutomatic));
                        return;
                    case "-service_manual":
                        Application.Run(new FormInstallService(FormInstallService.Action.SetManual));
                        return;
                    case "-service_disable":
                        Application.Run(new FormInstallService(FormInstallService.Action.SetDisabled));
                        return;
                    case "-service_uninstall":
                        Application.Run(new FormInstallService(FormInstallService.Action.Unsinstall));
                        return;
                    case "-service_start":
                        Application.Run(new FormInstallService(FormInstallService.Action.Start));
                        return;
                    case "-service_stop":
                        Application.Run(new FormInstallService(FormInstallService.Action.Stop));
                        return;
                    case "-confirm_open":
                        if (MessageBox.Show("Open gView MapService Administrator to administrate the map server", "Postinstallation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                            DialogResult.Yes)
                            return;
                        break;
                }
            }
            Application.Run(new Form1());
        }
    }
}