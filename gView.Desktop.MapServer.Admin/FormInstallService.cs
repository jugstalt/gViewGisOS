using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using gView.Framework.system;
using System.Diagnostics;

namespace gView.Desktop.MapServer.Admin
{
    public partial class FormInstallService : Form
    {
        public enum Action { Install = 0, Unsinstall = 1, Start = 2, Stop = 3, Install_forced=4, SetAutomatic=5, SetManual=6, SetDisabled=7 }

        private Action _action;
        private bool _succeeded = false;

        public FormInstallService(Action action)
        {
            InitializeComponent();

            _action = action;

            //ServiceController sc = new ServiceController("gView.MapServer.Tasker");
            //sc.Start();
            //sc.WaitForStatus(ServiceControllerStatus.Stopped);


        }

        private void FormInstallService_Shown(object sender, EventArgs e)
        {
            string path = SystemVariables.ApplicationDirectory + @"\gView.MapServer.Tasker.exe";
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                ServiceInstaller c = new ServiceInstaller();

                if (_action==Action.Install || _action==Action.Install_forced)
                {
                    if (_action != Action.Install_forced)
                    {
                        try
                        {
                            ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                            ServiceControllerStatus status = sc.Status;
                            // Serive already installed!!
                            txtOutput.Text = "Service 'gView.MapServer.Tasker' allready installed...";

                            _succeeded = true;

                            this.Refresh();
                            Thread.Sleep(2000);

                            this.Close();

                            return;
                        }
                        catch { }
                    }
                    try
                    {
                        ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                        sc.Refresh();
                        ServiceControllerStatus status = sc.Status;

                        c.UnInstallService("gView.MapServer.Tasker");
                    }
                    catch { }

                    if (!c.InstallService(path, "gView.MapServer.Tasker", "gView MapServer Tasker", false))
                    {
                        txtOutput.Text = ServiceInstallUtil.InstallService(path);
                        btnContinue.Visible = true;
                        return;
                        //lblStatus.Text = "Error on installing windows service 'gView.MapServer.Tasker'";
                        //btnContinue.Visible = true;
                        //return;
                    }
                    txtOutput.Text = "Service 'gView.MapServer.Tasker' successfully installed...";
                }
                else if (_action == Action.SetAutomatic)
                {
                    ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                    sc.Refresh();

                    ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Automatic);
                }
                else if (_action == Action.SetManual)
                {
                    ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                    sc.Refresh();

                    ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Manual);
                }
                else if (_action == Action.SetDisabled)
                {
                    ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                    sc.Refresh();

                    ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Disabled);
                }
                else if (_action == Action.Unsinstall)
                {
                    try
                    {
                        // Service stoppen
                        ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                        sc.Refresh();
                        TimeSpan ts = new TimeSpan(0, 0, 30);
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, ts);
                    }
                    catch { }
                    if (!c.UnInstallService("gView.MapServer.Tasker"))
                    {
                        txtOutput.Text = "Error on uninstalling windows service 'gView.MapServer.Tasker'";
                        btnContinue.Visible = true;
                        return;
                    }
                    txtOutput.Text = "Service 'gView.MapServer.Tasker' successfully uninstalled...";
                }
                else if (_action == Action.Start)
                {
                    try
                    {
                        TimeSpan ts = new TimeSpan(0, 0, 30);
                        ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                        if (sc.Status != ServiceControllerStatus.Running)
                        {
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running, ts);
                        }
                        txtOutput.Text = "Service 'gView.MapServer.Tasker' successfully started...";
                    }
                    catch (Exception ex)
                    {
                        txtOutput.Text = "Error: " + ex.Message;
                        btnContinue.Visible = true;
                        return;
                    }
                }
                else if (_action == Action.Stop)
                {
                    try
                    {
                        TimeSpan ts = new TimeSpan(0, 0, 30);
                        ServiceController sc = new ServiceController("gView.MapServer.Tasker");
                        if (sc.Status != ServiceControllerStatus.Stopped)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, ts);
                        }
                        txtOutput.Text = "Service 'gView.MapServer.Tasker' successfully stopped...";
                    }
                    catch (Exception ex)
                    {
                        txtOutput.Text = "Error: " + ex.Message;
                        btnContinue.Visible = true;
                        return;
                    }
                }
            }

            _succeeded = true;

            this.Refresh();
            Thread.Sleep(2000);

            this.Close();
        }

        public bool Succeeded
        {
            get { return _succeeded; }
        }
        private void btnContinue_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    /*
    /// <summary>
    /// Summary description for ServiceInstaller.
    /// </summary>
    class ServiceInstaller
    {
        #region DLLImport
        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            uint lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword);


        [DllImport("advapi32.dll")]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        #endregion DLLImport

        #region Flags
        [Flags]
        enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }

        [Flags]
        public enum SCM_ACCESS : uint
        {
            /// <summary>
            /// Required to connect to the service control manager.
            /// </summary>
            SC_MANAGER_CONNECT = 0x00001,

            /// <summary>
            /// Required to call the CreateService function to create a service
            /// object and add it to the database.
            /// </summary>
            SC_MANAGER_CREATE_SERVICE = 0x00002,

            /// <summary>
            /// Required to call the EnumServicesStatusEx function to list the 
            /// services that are in the database.
            /// </summary>
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

            /// <summary>
            /// Required to call the LockServiceDatabase function to acquire a 
            /// lock on the database.
            /// </summary>
            SC_MANAGER_LOCK = 0x00008,

            /// <summary>
            /// Required to call the QueryServiceLockStatus function to retrieve 
            /// the lock status information for the database.
            /// </summary>
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

            /// <summary>
            /// Required to call the NotifyBootConfigStatus function.
            /// </summary>
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access 
            /// rights in this table.
            /// </summary>
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        /// <summary>
        /// Access to the service. Before granting the requested access, the
        /// system checks the access token of the calling process. 
        /// </summary>
        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            /// <summary>
            /// Required to call the QueryServiceConfig and 
            /// QueryServiceConfig2 functions to query the service configuration.
            /// </summary>
            SERVICE_QUERY_CONFIG = 0x00001,

            /// <summary>
            /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function 
            /// to change the service configuration. Because this grants the caller 
            /// the right to change the executable file that the system runs, 
            /// it should be granted only to administrators.
            /// </summary>
            SERVICE_CHANGE_CONFIG = 0x00002,

            /// <summary>
            /// Required to call the QueryServiceStatusEx function to ask the service 
            /// control manager about the status of the service.
            /// </summary>
            SERVICE_QUERY_STATUS = 0x00004,

            /// <summary>
            /// Required to call the EnumDependentServices function to enumerate all 
            /// the services dependent on the service.
            /// </summary>
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,

            /// <summary>
            /// Required to call the StartService function to start the service.
            /// </summary>
            SERVICE_START = 0x00010,

            /// <summary>
            ///     Required to call the ControlService function to stop the service.
            /// </summary>
            SERVICE_STOP = 0x00020,

            /// <summary>
            /// Required to call the ControlService function to pause or continue 
            /// the service.
            /// </summary>
            SERVICE_PAUSE_CONTINUE = 0x00040,

            /// <summary>
            /// Required to call the EnumDependentServices function to enumerate all
            /// the services dependent on the service.
            /// </summary>
            SERVICE_INTERROGATE = 0x00080,

            /// <summary>
            /// Required to call the ControlService function to specify a user-defined
            /// control code.
            /// </summary>
            SERVICE_USER_DEFINED_CONTROL = 0x00100,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
            /// </summary>
            SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL),

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SERVICE_QUERY_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_INTERROGATE |
                SERVICE_ENUMERATE_DEPENDENTS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SERVICE_CHANGE_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_USER_DEFINED_CONTROL,

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity or 
            /// SetServiceObjectSecurity function to access the SACL. The proper
            /// way to obtain this access is to enable the SE_SECURITY_NAME 
            /// privilege in the caller's current access token, open the handle 
            /// for ACCESS_SYSTEM_SECURITY access, and then disable the privilege.
            /// </summary>
            ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,

            /// <summary>
            /// Required to call the DeleteService function to delete the service.
            /// </summary>
            DELETE = ACCESS_MASK.DELETE,

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity function to query
            /// the security descriptor of the service object.
            /// </summary>
            READ_CONTROL = ACCESS_MASK.READ_CONTROL,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify
            /// the Dacl member of the service object's security descriptor.
            /// </summary>
            WRITE_DAC = ACCESS_MASK.WRITE_DAC,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify 
            /// the Owner and Group members of the service object's security 
            /// descriptor.
            /// </summary>
            WRITE_OWNER = ACCESS_MASK.WRITE_OWNER,
        }

        /// <summary>
        /// Service types.
        /// </summary>
        [Flags]
        public enum SERVICE_TYPE : uint
        {
            /// <summary>
            /// Driver service.
            /// </summary>
            SERVICE_KERNEL_DRIVER = 0x00000001,

            /// <summary>
            /// File system driver service.
            /// </summary>
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,

            /// <summary>
            /// Service that runs in its own process.
            /// </summary>
            SERVICE_WIN32_OWN_PROCESS = 0x00000010,

            /// <summary>
            /// Service that shares a process with one or more other services.
            /// </summary>
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,

            /// <summary>
            /// The service can interact with the desktop.
            /// </summary>
            SERVICE_INTERACTIVE_PROCESS = 0x00000100,
        }

        /// <summary>
        /// Service start options
        /// </summary>
        public enum SERVICE_START : uint
        {
            /// <summary>
            /// A device driver started by the system loader. This value is valid
            /// only for driver services.
            /// </summary>
            SERVICE_BOOT_START = 0x00000000,

            /// <summary>
            /// A device driver started by the IoInitSystem function. This value 
            /// is valid only for driver services.
            /// </summary>
            SERVICE_SYSTEM_START = 0x00000001,

            /// <summary>
            /// A service started automatically by the service control manager 
            /// during system startup. For more information, see Automatically 
            /// Starting Services.
            /// </summary>         
            SERVICE_AUTO_START = 0x00000002,

            /// <summary>
            /// A service started by the service control manager when a process 
            /// calls the StartService function. For more information, see 
            /// Starting Services on Demand.
            /// </summary>
            SERVICE_DEMAND_START = 0x00000003,

            /// <summary>
            /// A service that cannot be started. Attempts to start the service
            /// result in the error code ERROR_SERVICE_DISABLED.
            /// </summary>
            SERVICE_DISABLED = 0x00000004,
        }



        /// <summary>
        /// Severity of the error, and action taken, if this service fails 
        /// to start.
        /// </summary>
        public enum SERVICE_ERROR
        {
            /// <summary>
            /// The startup program ignores the error and continues the startup
            /// operation.
            /// </summary>
            SERVICE_ERROR_IGNORE = 0x00000000,

            /// <summary>
            /// The startup program logs the error in the event log but continues
            /// the startup operation.
            /// </summary>
            SERVICE_ERROR_NORMAL = 0x00000001,

            /// <summary>
            /// The startup program logs the error in the event log. If the 
            /// last-known-good configuration is being started, the startup 
            /// operation continues. Otherwise, the system is restarted with 
            /// the last-known-good configuration.
            /// </summary>
            SERVICE_ERROR_SEVERE = 0x00000002,

            /// <summary>
            /// The startup program logs the error in the event log, if possible.
            /// If the last-known-good configuration is being started, the startup
            /// operation fails. Otherwise, the system is restarted with the 
            /// last-known good configuration.
            /// </summary>
            SERVICE_ERROR_CRITICAL = 0x00000003,
        }

        #endregion

        public bool InstallService(string svcPath, string svcName, string svcDispName, bool startIt)
        {
            try
            {
                IntPtr sc_handle = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_CREATE_SERVICE);
                if (sc_handle.ToInt64() != 0)
                {
                    uint tagId = 0;
                    IntPtr sv_handle = CreateService(
                        sc_handle,
                        svcName,
                        svcDispName,
                        (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS,
                        (uint)SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                        (uint)SERVICE_START.SERVICE_DEMAND_START,
                        (uint)SERVICE_ERROR.SERVICE_ERROR_NORMAL,
                        svcPath, null, tagId, null, null, null);
                    if (sv_handle.ToInt64() == 0)
                    {
                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        //now trying to start the service
                        if (startIt)
                        {
                            int i = StartService(sv_handle, 0, null);
                            // If the value i is zero, then there was an error starting the service.
                            // note: error may arise if the service is already running or some other problem.
                            if (i == 0)
                            {
                                //Console.WriteLine("Couldnt start service");
                                return false;
                            }
                        }
                        //Console.WriteLine("Success");
                        CloseServiceHandle(sc_handle);
                        return true;
                    }
                }
                else
                    //Console.WriteLine("SCM not opened successfully");
                    return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public bool UnInstallService(string svcName)
        {
            IntPtr sc_hndl = OpenSCManager(null, null, (uint)SCM_ACCESS.GENERIC_WRITE);
            if (sc_hndl.ToInt64() != 0)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, svcName, DELETE);
                //Console.WriteLine(svc_hndl.ToInt64());
                if (svc_hndl.ToInt64() != 0)
                {
                    int i = DeleteService(svc_hndl);
                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }
    }
    */

    /// <summary>
/// Summary description for ServiceInstaller.
/// </summary>
    class ServiceInstaller
    {
        #region Private Variables
        private string _servicePath;
        private string _serviceName;
        private string _serviceDisplayName;
        #endregion Private Variables
        #region DLLImport
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);
        [DllImport("Advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
        int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
        string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);
        [DllImport("advapi32.dll")]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        #endregion DLLImport

        public bool InstallService(string svcPath, string svcName, string svcDispName, bool startService)
        {
            #region Constants declaration.
            int SC_MANAGER_CREATE_SERVICE = 0x0002;
            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            int SERVICE_ERROR_NORMAL = 0x00000001;
            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;
            int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
            SERVICE_QUERY_CONFIG |
            SERVICE_CHANGE_CONFIG |
            SERVICE_QUERY_STATUS |
            SERVICE_ENUMERATE_DEPENDENTS |
            SERVICE_START |
            SERVICE_STOP |
            SERVICE_PAUSE_CONTINUE |
            SERVICE_INTERROGATE |
            SERVICE_USER_DEFINED_CONTROL);
            int SERVICE_AUTO_START = 0x00000002;
            int SERVICE_DEMAND_START = 0x00000003;
            #endregion Constants declaration.
            try
            {
                IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                if (sc_handle != IntPtr.Zero)
                {
                    IntPtr sv_handle = CreateService(sc_handle, svcName, svcDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_DEMAND_START, SERVICE_ERROR_NORMAL, svcPath, null, 0, null, null, null);
                    if (sv_handle == IntPtr.Zero)
                    {
                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        //now trying to start the service
                        if (startService)
                        {
                            int i = StartService(sv_handle, 0, null);
                            // If the value i is zero, then there was an error starting the service.
                            // note: error may arise if the service is already running or some other problem.
                            if (i == 0)
                            {
                                //Console.WriteLine("Couldnt start service");
                                return false;
                            }
                            //Console.WriteLine("Success");
                        }
                        CloseServiceHandle(sc_handle);
                        return true;
                    }
                }
                else
                    //Console.WriteLine("SCM not opened successfully");
                    return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public bool UnInstallService(string svcName)
        {
            int GENERIC_WRITE = 0x40000000;
            IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
            if (sc_hndl != IntPtr.Zero)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, svcName, DELETE);
                //Console.WriteLine(svc_hndl.ToInt32());
                if (svc_hndl != IntPtr.Zero)
                {
                    int i = DeleteService(svc_hndl);
                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }
    }

    class ServiceInstallUtil
    {
        static public string InstallService(string svcPath)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = @"C:\windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe";
            proc.StartInfo.Arguments = "/i " + "\"" + svcPath + "\"";
            proc.StartInfo.RedirectStandardOutput=true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            string err = proc.StandardOutput.ReadToEnd();

            //proc.WaitForExit();

            return err;
        }

        static public string UninstallService(string svcPath)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = @"C:\windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe";
            proc.StartInfo.Arguments = "/u " + "\"" + svcPath + "\"";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            string err = proc.StandardOutput.ReadToEnd();

            //proc.WaitForExit();

            return err;
        }
    }
}