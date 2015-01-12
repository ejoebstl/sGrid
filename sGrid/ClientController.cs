using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using sGridClientUI;
using Boinc;
using System.Windows.Forms;
using Resource = sGrid.Resources.ClientRes;
using sGrid.sGridClientApi;

namespace sGrid
{
    /// <summary>
    /// This class is responsible for the logic behind the client.
    /// It provides the methods to handle the requests of the user.
    /// 
    /// There are 4 states: not logged in, idle, paused, calculating.
    /// </summary>
    class ClientController
    {
        /// <summary>
        /// Represents the notify icon for the applicaion.
        /// </summary>
        private System.Windows.Forms.NotifyIcon notify;

        /// <summary>
        /// The window responsible for the main controls of the
        /// client.
        /// </summary>
        public MainWindow MasterWindow;

        /// <summary>
        /// The window responsible for the log-in process.
        /// </summary>
        private LogIn login;

        /// <summary>
        /// The application which is currently running.
        /// </summary>
        private App app;

        /// <summary>
        /// The clientEventController is responsible for managing
        /// the incoming events.
        /// </summary>
        private ClientEventController clientEventController;

        /// <summary>
        /// The manager responsible for the current state of the
        /// client.
        /// </summary>
        public ConfigurationManager ConfigManager { get; private set; }

        /// <summary>
        /// The ConfigChanger is responsible for changing the
        /// configuration of the client.
        /// </summary>
        public Configurator ConfigChanger { get; private set; }

        /// <summary>
        /// The boinc core managing the calculation.
        /// </summary>
        public BoincClient Boinc { get; private set; }

        /// <summary>
        /// The clientApiWrapper is used to connect to the server.
        /// </summary>
        internal ClientApiWrapper ApiWrapper { get; private set; }

        /// <summary>
        /// The token verifying if the incoming messages derive
        /// from the rigth server.
        /// </summary>
        public string AntiForgeryToken { get; set; }

        public ClientState State
        {
            get
            {
                if (ConfigManager.CurrentUser == null) return ClientState.LoggedOut;
                if ((ConfigManager.CurrentProjectShortName == null) || (clientEventController.Watcher.Projects.Length == 0)) return ClientState.Idle;
                if (clientEventController.Watcher.Projects[0].Suspended) return ClientState.Paused;
                return ClientState.Calculating;
            }
        }

        /// <summary>
        /// The website of sGrid.
        /// </summary>
        public const string WebsiteUrl = "http://sgrid.azurewebsites.net";

        #region constructor and methods which initialize

        /// <summary>
        /// Creates the configuration with the default settings.
        /// </summary>
        private Configuration CreateStandardConfiguration()
        {
            //adds a standart configuration for test purposes
            //normally this should be done by the installer
            Configuration c = new Configuration();
            c.Autostart = true;
            c.CpuUsageLimit = 50;
            c.DiskSpaceLimit = 1000;
            c.RunOnBattery = false;
            c.RunOnEnergySaver = false;
            c.UseGpu = false;
            //Limit boinc to work with only one processor by setting the corresponding preferences. 
            //This ensures that only one result is being worked on simultaneously. 
            c.ProcessorCoresInUse = 1;
            return c;
        }

        /// <summary>
        /// Creates a new instance of this class and initializes the
        /// values.
        /// </summary>
        /// <param name="app">The application that has been started.</param>
        public ClientController(App app)
        {
            //initialize the private members
            this.app = app;
            Boinc = new BoincClient();
            clientEventController = new ClientEventController(this, Boinc);
            ConfigManager = new ConfigurationManager();
            ConfigChanger = new Configurator(Boinc);
            ApiWrapper = new ClientApiWrapper(this);
            MasterWindow = new MainWindow(this);
            login = new LogIn(this);

            CheckConfiguration();

            InitNotifier();

            //test if a user is logged in
            if (State != ClientState.LoggedOut)
            {
                StartBoinc();
            }
            else
            {
                //we have to get the user
                login.InitializeBrowser();
                login.Show();
            }
            UpdateClientUI();
        }

        private void CheckConfiguration()
        {
            Configuration stdConf = CreateStandardConfiguration();
            Configuration conf = ConfigManager.CurrentConfiguration;
            if (conf == null)
            {
                conf = stdConf;
            }
            else
            {
                if ((conf.CpuUsageLimit < 0) || (100 < conf.CpuUsageLimit))
                    conf.CpuUsageLimit = stdConf.CpuUsageLimit;
                if ((conf.DiskSpaceLimit < 0) || (10000 < conf.DiskSpaceLimit))
                    conf.DiskSpaceLimit = stdConf.DiskSpaceLimit;
                if (conf.ProcessorCoresInUse != 1)
                    conf.ProcessorCoresInUse = stdConf.ProcessorCoresInUse;
            }
            ConfigManager.SaveConfiguration(conf);
        }

        private void StartBoinc()
        {
            if (!Boinc.BoincIsRunning)
            {
                Boinc.StartClient();
            }
            if (Boinc.IsConnected || ConnectToBoinc())
            {
                ConfigChanger.ApplyConfiguration(ConfigManager.CurrentConfiguration);
                clientEventController.Watcher.ClearState(); //Reset and gather new state. 
                clientEventController.Watcher.RefreshState();

                if (State != ClientState.Idle)
                {
                    //start the client normally
                    clientEventController.StartTimer();
                }
                else
                {
                    //no project to work on
                    ProjectUpdate();
                }
            }
            else
            {
                StartApplicationShutdown();
            }
        }

        /// <summary>
        /// Tries to connect to boinc. This method finishes, when
        /// the connection to boinc is established, or when the
        /// user aborted.
        /// This method is meant to be invoked by the StartBoinc method.
        /// </summary>
        /// <returns>True, if the connection has been established.</returns>
        private bool ConnectToBoinc()
        {
            int numberOfTries = 4;
            bool userWantsToContinue = true;
            while (userWantsToContinue)
            {
                for (int i = 0; i < numberOfTries; i++)
                {
                    try
                    {
                        Boinc.Connect();
                        //if the code reaches this, then the
                        //connection has been established
                        return true;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        //boinc has not started yet
                        if (i < numberOfTries - 1)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            //ask the user if he wants to retry
                            MessageBoxResult result = System.Windows.MessageBox.Show(Resource.BoincNotStarted,
                                "sGrid", MessageBoxButton.YesNo);
                            if (result != MessageBoxResult.Yes)
                            {
                                userWantsToContinue = false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes the notifier which offers some options for
        /// the user.
        /// </summary>
        private void InitNotifier()
        {
            notify = new System.Windows.Forms.NotifyIcon();
            notify.Text = "sGrid";
            notify.Icon = sGrid.Properties.Resources.sGrid;
            notify.Visible = true;

            MenuItem[] items = new MenuItem[] {
                new MenuItem(Resource.Open, (s, e) => Open()),
                new MenuItem("Pause/Start"),
                new MenuItem(Resource.Autostart, (s, e) => this.ToggleAutostart()),
                new MenuItem(Resource.LogOut, (s, e) => this.AskLogOut()),
                new MenuItem("-"),
                new MenuItem(Resource.Exit, (s, e) => this.StartApplicationShutdown())
            };

            notify.ContextMenu = new ContextMenu(items);
            notify.Click += new EventHandler(OnNotifyClick);
        }

        /// <summary>
        /// Occours when the notify icon has been clicked. 
        /// </summary>
        void OnNotifyClick(object sender, EventArgs e)
        {
            this.Open();
            App.BringToForeground(true);
        }

        /// <summary>
        /// Adapts the notifier and the main window to the current
        /// situation.
        /// </summary>
        private void UpdateClientUI()
        {
            MasterWindow.StatusUpdate(clientEventController.Watcher.Results);

            MenuItem[] items = new MenuItem[6];
            notify.ContextMenu.MenuItems.CopyTo(items, 0);

            if (State == ClientState.Calculating)
            {
                //if calculating
                items[1] = new MenuItem(Resource.Pause, (s, e) => this.Pause());
                MasterWindow.buttonPauseStart.Content = Resource.Pause;
            }
            else
            {
                items[1] = new MenuItem(Resource.Start, (s, e) => this.Start());
                MasterWindow.buttonPauseStart.Content = Resource.Start;
            }
            items[1].Enabled = State != ClientState.LoggedOut;
            items[2].Checked = ConfigManager.CurrentConfiguration.Autostart;
            items[3].Enabled = State != ClientState.LoggedOut;

            notify.ContextMenu = new ContextMenu(items);
        }

        #endregion

        #region methods used by the notifier and the main window

        /// <summary>
        /// Opens the current window.
        /// </summary>
        public void Open()
        {
            if (State != ClientState.LoggedOut)
            {
                MasterWindow.Show();
                MasterWindow.WindowState = WindowState.Normal;
            }
            else
            {
                login.Show();
            }
        }

        /// <summary>
        /// Pauses the calculation.
        /// </summary>
        public void Pause()
        {
            Boinc.GetProjects()[0].Suspend();
            clientEventController.StopTimer();
            
            UpdateClientUI();
        }

        /// <summary>
        /// Restarts the calculation by de-suspending the project,
        /// or, if no project is available, tests if the application
        /// should stay in Idle Mode
        /// </summary>
        public void Start()
        {
            if (State == ClientState.Idle)
            {
                TestIfStillIdle(true);
            }
            else
            {
                //start the calculation after being paused
                Boinc.GetProjects()[0].Resume();
                clientEventController.StartTimer();

                UpdateClientUI();
            }
        }

        

        /// <summary>
        /// Changes the autostart settings.
        /// </summary>
        private void ToggleAutostart()
        {
            Configuration newConfig = ConfigManager.CurrentConfiguration;
            newConfig.Autostart = !newConfig.Autostart;

            ConfigChanger.ApplyAutostart(newConfig);
            ConfigManager.SaveConfiguration(newConfig);
            UpdateClientUI();
        }

        /// <summary>
        /// Asks if the user wants to log out.
        /// </summary>
        public void AskLogOut()
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show(Resource.LogOutWarning,
                "sGrid", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                //the user wants to log out
                LogoutAndStartLogin();
            }
        }

        /// <summary>
        /// Perfoms the necessary actions in order to log out the
        /// current user.
        /// </summary>
        public void LogoutAndStartLogin()
        {
            clientEventController.StopTimer();

            //close open windows
            if (MasterWindow.IsEnabled) MasterWindow.Hide();

            //close boinc
            if (Boinc.IsConnected)
            {
                foreach (Project p in Boinc.GetProjects())
                {
                    p.Detach();
                }
                Boinc.QuitClient();
            }
            ConfigManager.RememberUser(null);
            ConfigManager.SaveProject("", "");
            login.InitializeBrowser(true);
            login.Show();
            UpdateClientUI();
        }

        /// <summary>
        /// Performs the necessary actions to shut down the
        /// application.
        /// </summary>
        public void StartApplicationShutdown()
        {
            MasterWindow.Visibility = Visibility.Hidden;
            MasterWindow.Dispatcher.BeginInvoke(new Action<bool>(EndApplicationShutdown), false);
        }

        /// <summary>
        /// Performs the necessary actions to shut down the
        /// application.
        /// </summary>
        public void EndApplicationShutdown(bool dummy)
        {
            clientEventController.StopTimer();
            if (Boinc.IsConnected) Boinc.QuitClient();
            notify.Dispose();
            clientEventController.Close();
            app.Shutdown();
        }

        #endregion

        #region package and project tests

        /// <summary>
        /// Changes the project currently calculated on to a new
        /// one, if the user has chosen a project.
        /// </summary>
        public void ProjectUpdate(ProjectData newProjectData = null)
        {
            clientEventController.StopTimer();
            clientEventController.Watcher.RefreshState();

            if (newProjectData == null) newProjectData = ApiWrapper.GetNewProjectData();
            if (State == ClientState.LoggedOut) return; //if corrupted credential or old client version

            if (clientEventController.Watcher.Projects.Length != 0 && newProjectData == null) return; //We have projects to run on but probably no server connection. Continue. 

            if (newProjectData == null)
            {
                //no new project has been chosen -> idle mode
                foreach (Project proj in Boinc.GetProjects()) proj.Detach();

                //set the values
                ConfigManager.SaveProject("", "");
                UpdateClientUI();
                MasterWindow.Show();

                //inform the user
                AllertThatIdle();
            }
            else
            {
                if ((newProjectData.GridProject.ShortName != ConfigManager.CurrentProjectShortName)
                    || (clientEventController.Watcher.Projects.Length == 0))
                {
                    //detach the old project and attach the new one
                    foreach (Project proj in Boinc.GetProjects())
                    {
                        proj.Detach();
                    }
                     
                    Boinc.AttachToProject(newProjectData.GridProject.WorkspaceUrl,
                        newProjectData.GridToken, newProjectData.GridProject.Name);
                    ConfigManager.SaveProject(newProjectData.GridProject.ShortName, newProjectData.GridProject.Name);

                    UpdateClientUI();
                }
                clientEventController.StartTimer();
            }
        }

        /// <summary>
        /// Tests if the actual mode is still idle and perfoms the
        /// necessary steps if the user chose a new project.
        /// </summary>
        /// <param name="allertUser">True, if the user should be
        /// allerted, if the application remains in idle mode.</param>
        public void TestIfStillIdle(bool allertUser)
        {
            ProjectData newProjectData = ApiWrapper.GetNewProjectData();
            if (State == ClientState.LoggedOut) return; //if corrupted credential or old client version

            if (newProjectData == null)
            {
                //still idle
                if (allertUser) AllertThatIdle();
            }
            else
            {
                //there are actually no projects on the client, but
                //the user chose a new one
                ProjectUpdate(newProjectData);
            }
        }

        private void AllertThatIdle()
        {
            string text;
            if (ApiWrapper.ConnectedToServer)
            {
                text = String.Format(Resource.NoProjectWarning, WebsiteUrl);
            }
            else
            {
                text = Resource.NoConnectionWarning;
            }
            System.Windows.Forms.MessageBox.Show(text, "sGrid", System.Windows.Forms.MessageBoxButtons.OK);

            StartApplicationShutdown();
        }

        #endregion

        #region miscellaneous

        /// <summary>
        /// Initializes the client after a new user has logged in.
        /// </summary>
        /// <param name="data"></param>
        public void PerformLogin(AuthenticationReceivedEventArgs data)
        {
            if ((State == ClientState.LoggedOut) && (data.antiForgeryToken == AntiForgeryToken))
            {
                if (login.IsEnabled) login.Hide();

                ConfigManager.RememberUser(new UserInformation(data.Credential.AuthenticationToken, data.Credential.UserId));
                StartBoinc();
                UpdateClientUI();
                Open();
            }
        }

        #endregion
    }

    enum ClientState
    {
        LoggedOut,
        Idle,
        Paused,
        Calculating
    }
}
