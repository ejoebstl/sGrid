using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using sGrid;
using Resource = sGrid.Resources.ClientRes;
using System.Timers;
using Boinc;

namespace sGridClientUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        /// <summary>
        /// The clientController performs the actions requested
        /// through this class.
        /// </summary>
        private ClientController clientController;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Creates a new instance of this class and initializes
        /// the values.
        /// </summary>
        /// <param name="clientController">The clientController
        /// performing the actions.</param>
        internal MainWindow(ClientController clientController)
        {
            this.InitializeComponent();

            this.clientController = clientController;

            Initialize();
        }

        /// <summary>
        /// Initializes the content of the buttons.
        /// </summary>
        private void Initialize()
        {
            Title = Resource.sGrid;

            //initializes the labels
            buttonLogOutLogIn.Content = Resource.LogOut;
            buttonSettings.Content = Resource.Settings;
            buttonExit.Content = Resource.Exit;

            //initializes the hyperlink
            Hyperlink hyperlink = new Hyperlink(new Run(ClientController.WebsiteUrl))
            {
                NavigateUri = new Uri(ClientController.WebsiteUrl)
            };
            hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(hyperlink_RequestNavigate);
            textBlockHyperlink.Inlines.Clear();
            textBlockHyperlink.Inlines.Add(hyperlink);
        }

        /// <summary>
        /// Invokes the sGrid website.
        /// This method event is raised, when a user clicks on the hyperlink.
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="e">The parameters of this event.</param>
        void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(ClientController.WebsiteUrl);
        }

        /// <summary>
        /// Updates the labels and the progression bar in this
        /// window according to the current state of the client.
        /// </summary>
        /// <param name="results">The results the client is
        /// currently working on.
        /// If the client is logged out or idle, results has length
        /// 0, else it has length 1.</param>
        public void StatusUpdate(Result[] results)
        {
            string projName = clientController.ConfigManager.CurrentProjectName;
            ClientState state = clientController.State;

            if ((state == ClientState.LoggedOut) || (state == ClientState.Idle))
            {
                labelProjectName.Content = Resource.ProjectNameUnknown;
                labelStatus.Content = Resource.WorkFetching;
                labelPercentage.Content = "";
                progressBarPercentage.Value = 0;
            }
            else
            {
                labelProjectName.Content = String.Format(Resource.ProjectName, projName);
                if (results.Length == 0)
                {
                    //result has not yet arrived

                    labelPercentage.Content = "";
                    progressBarPercentage.Value = 0;
                    labelStatus.Content = Resource.WorkFetching;
                }
                else
                {
                    Result res = results[0];
                    labelPercentage.Content = String.Format("{0:0.0}", res.FractionDone * 100) + " %";
                    progressBarPercentage.Value = res.FractionDone;

                    if (state == ClientState.Paused)
                        labelStatus.Content = Resource.Paused;
                    else /*if (state == ClientState.Calculating)*/
                        labelStatus.Content = String.Format(Resource.StatusRemainingTime, res.EstimatedCpuTimeRemaining.ToString());
                }
            }
        }

        /// <summary>
        /// This event is triggered, when the PauseStart Button is
        /// clicked.
        /// This button is used to stop and restart the calculation.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void PauseStart_Click(object sender, RoutedEventArgs e)
        {
            if (clientController.State ==ClientState.Calculating)
            {
                //the client is calculating => can be paused
                clientController.Pause();
            }
            else
            {
                //client is not calculating => can be started
                //idle mode => the user wants to start the calculation
                clientController.Start();
            }
        }

        /// <summary>
        /// This event is triggered, when the logOut Button is
        /// clicked.
        /// This button is used to log the current user out.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            clientController.AskLogOut();
        }

        /// <summary>
        /// This event is triggered, when the Settings Button is
        /// clicked.
        /// This button is used to get access to the settings-window,
        /// where the settings can be changed.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Configuration = clientController.ConfigManager.CurrentConfiguration;
            if (settings.ShowDialog() == true)
            {
                clientController.ConfigManager.SaveConfiguration(settings.Configuration);
                clientController.ConfigChanger.ApplyConfiguration(settings.Configuration);
            }
        }

        /// <summary>
        /// This event is triggered, when the Exit Button is
        /// clicked.
        /// This button is used to close the application.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event.</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            clientController.StartApplicationShutdown();
        }

        /// <summary>
        /// This event is triggered, when this window is closed.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event, namely a value indicating if the closing
        /// process should be cancelled.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        /// <summary>
        /// This event is triggered when the window state changes.
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}