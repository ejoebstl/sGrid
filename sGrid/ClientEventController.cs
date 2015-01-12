using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using System.Timers;
using Boinc;

namespace sGrid
{
    /// <summary>
    /// This class is responsible for registering to events and
    /// handle them.
    /// </summary>
    class ClientEventController
    {
        /// <summary>
        /// The clientController performs the actions requested
        /// through this class.
        /// </summary>
        private ClientController clientController;

        /// <summary>
        /// Throws events if projects and/or results change.
        /// </summary>
        public BoincWatcher Watcher { get; set; }

        /// <summary>
        /// The dispatcher of the main thread. The main thread has
        /// the permission to work on the UI.
        /// </summary>
        private Dispatcher dispatcher
        {
            get { return clientController.MasterWindow.Dispatcher; }
        }

        /// <summary>
        /// The port the listener is listening to.
        /// </summary>
        public const int Port = 45045;

        /// <summary>
        /// The listener listening to incoming requests from the
        /// browser.
        /// </summary>
        private Listener listener;

        /// <summary>
        /// The timer raising an event periodically.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Creates a new instance of thsi class and initializes
        /// the values.
        /// </summary>
        /// <param name="clientController">The clientController
        /// performing the actions.</param>
        /// <param name="boincClient">The Boinc client being watched.</param>
        public ClientEventController(ClientController clientController, BoincClient boincClient)
        {
            this.clientController = clientController;

            SetListener();
            InitializeWatcher(boincClient);
            InitializeTimer();
        }

        /// <summary>
        /// Closes the listener when the application is shut down.
        /// </summary>
        public void Close()
        {
            listener.StopListen();
        }

        /// <summary>
        /// Initializes and starts the listener.
        /// </summary>
        private void SetListener()
        {
            listener = new Listener(Port);

            listener.AuthenticationReceived += new EventHandler<AuthenticationReceivedEventArgs>(OnAuthenticationReceived);
            listener.UpdateRequested += new EventHandler(OnUpdateRequested);

            listener.StartListen();
        }

        /// <summary>
        /// Initisalizes the timer, sets the attributes and
        /// registers to the event.
        /// </summary>
        private void InitializeTimer()
        {
            timer = new Timer();

            timer.AutoReset = true;

            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
        }

        /// <summary>
        /// Initializes the boinc watcher and registers to the
        /// important events.
        /// </summary>
        /// <param name="boincClient">Initialize</param>
        private void InitializeWatcher(BoincClient boincClient)
        {
            Watcher = new BoincWatcher(boincClient);

            Watcher.TaskRemoved += new EventHandler<CollectionModifiedEventArgs<Result>>(OnTaskRemoved);
            Watcher.TaskStateChanged += new EventHandler<TaskStateChangedEventArgs>(OnTaskStateChanged);

            //Watcher.RefreshState();
        }

        /// <summary>
        /// Starts the timer in the correct mode.
        /// (idle mode or normal mode)
        /// </summary>
        public void StartTimer()
        {
            if (clientController.Boinc.IsConnected)
            {
                Watcher.ClearState();
                Watcher.RefreshState();
            }
            if (!timer.Enabled)
            {
                if (clientController.State == ClientState.Idle)
                    timer.Interval = 15000; //15 sec
                else
                    timer.Interval = 2000; //2 sec
                timer.Start();
            }
        }

        /// <summary>
        /// Stops the timer and if necessary perform an additionnal
        /// test.
        /// </summary>
        public void StopTimer()
        {
            if (timer.Enabled)
            {
                timer.Stop();
                if (clientController.State != ClientState.Idle)
                {
                    Watcher.RefreshState();
                }
            }
        }

        /// <summary>
        /// This event handler is triggered, when the user logged in
        /// through the brower and the client requested the
        /// authentication data for the user.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the authentication data.</param>
        private void OnAuthenticationReceived(object sender, AuthenticationReceivedEventArgs data)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(new EventHandler<AuthenticationReceivedEventArgs>(OnAuthenticationReceived), sender, data);
            }
            else
            {
                clientController.PerformLogin(data);
            }
        }

        /// <summary>
        /// This event handler is triggered, when the user requested
        /// a change of the project he is calculating on.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">Placeholder, just in case this event
        /// later needs arguments to be passed.</param>
        private void OnUpdateRequested(object sender, EventArgs data)
        {

            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(new EventHandler<EventArgs>(OnUpdateRequested), sender, data);
            }
            else
            {
                if (clientController.State != ClientState.LoggedOut)
                {
                    clientController.ProjectUpdate();
                }
            }
        }

        /// <summary>
        /// This event handler is triggered by the Timer object
        /// every 20 seconds.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the signal time.</param>
        private void OnTimerElapsed(object sender, ElapsedEventArgs data)
        {
            if (clientController.Boinc.IsConnected)
            {
                if (!dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(new EventHandler<ElapsedEventArgs>(OnTimerElapsed), sender, data);
                }
                else
                {
                    if (clientController.State == ClientState.Idle)
                    {
                        clientController.TestIfStillIdle(false);
                    }
                    else
                    {
                        Watcher.RefreshState();
                    }
                }
            }
        }

        void OnTaskRemoved(object sender, CollectionModifiedEventArgs<Result> data)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(new EventHandler<CollectionModifiedEventArgs<Result>>(OnTaskRemoved), sender, data);
            }
            else
            {
                clientController.ApiWrapper.PackageFinished(data.ModifiedItem.ReceivedTime);
                if (clientController.State == ClientState.LoggedOut) return; //if corrupted credential or old client version

                clientController.ProjectUpdate();
            }
        }

        void OnTaskStateChanged(object sender, TaskStateChangedEventArgs data)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(new EventHandler<TaskStateChangedEventArgs>(OnTaskStateChanged), sender, data);
            }
            else
            {
                clientController.MasterWindow.StatusUpdate(new Result[] { data.NewState });
            }
        }
    }
}
