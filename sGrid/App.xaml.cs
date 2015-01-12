using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using sGridClientUI;
using Boinc;
using System.Windows.Forms;
using sGrid.sGridClientApi;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using sGrid.Resources;

namespace sGrid
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// This class is in charge of starting the ClientController and doing first-time initialization. 
    /// </summary>
    public partial class App : System.Windows.Application
    {

        //A mutex for ensuring that only one instance is running.
        private Mutex oneInstanceMutex;
        private bool mutexIsMine;

        /// <summary>
        /// Brings the window with the given handle to foreground. 
        /// </summary>
        /// <param name="hWnd">The window handle of the window to show.</param>
        /// <returns>A bool indicating success or failure.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event
        /// and perform the actions to starts the client.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ////Debug Settings
            //foreach (string arg in e.Args)
            //{
            //    if (arg.ToLower() == "-d")
            //    {
            //        BoincClient.HideBoincWindow = false;
            //        System.Windows.MessageBox.Show("DEBUG");
            //    }
            //}

            ////Check whether sGrid is already running. 
            //if (!CheckOneInstance())
            //{
            //    //If true, exit. 
            //    mutexIsMine = false;
            //    Shutdown();
            //}
            //else
            //{
            //    //Else, start up.
            //    mutexIsMine = true;

            //    InstallationHelper installer = new InstallationHelper();

            //    if (installer.BoincInstallationDetected)
            //    {
            //        System.Windows.MessageBox.Show(ClientRes.BoincAlreadyInstalled);
            //    }

            //    installer.ConfigureBoincPaths();

            //    //Finally, start the application. 
            //    new ClientController(this);
            //}

            MainWindow w = new MainWindow();
            w.Show();
        }

        /// <summary>
        /// Checks whether sGrid is already running.
        /// <remarks>This function also brings sGrid to foreground if it is already running.</remarks>
        /// </summary>
        /// <returns>True, if sGrid is not running, false, if sGrid is already running.</returns>
        private bool CheckOneInstance()
        {
            bool createdNew = false;

            //Aquire sGrid mutex. 
            oneInstanceMutex = new Mutex(true, "sGridOneInstanceMutex", out createdNew);

            //If it was not ours, we shall show the main window of the running sGrid instance.
            if (!createdNew)
            {
                BringToForeground();
            }

            return createdNew;
        }

        /// <summary>
        /// Brings the current sGrid instance to foreground. 
        /// </summary>
        /// <param name="current">A bool indicating whether the current process's main window also should be brought to foreground.</param>
        public static void BringToForeground(bool alsoCurrent = false)
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id || alsoCurrent)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }

        /// <summary>
        /// Releases the one instance mutex.
        /// </summary>
        /// <param name="e">The exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            oneInstanceMutex.Close();
        }
    }
}
