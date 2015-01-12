using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using sGrid.sGridClientApi;

namespace sGrid
{
    /// <summary>
    /// This class listens at localhost for browser signals and
    /// acts in case an appropriate event occurs.
    /// </summary>
    class Listener
    {
        /// <summary>
        /// A bool indicating whether the listener is actually
        /// listening on the port.
        /// </summary>
        private bool isRunning;

        /// <summary>
        /// The thread is responsible for the asynchronous
        /// behaviour of the listener.
        /// </summary>
        private Thread worker;

        /// <summary>
        /// The listener listening for incoming messages.
        /// </summary>
        private HttpListener listener;

        /// <summary>
        /// Object used for locks.
        /// </summary>
        private Object o;

        /// <summary>
        /// The prefix for the port to listen on.
        /// </summary>
        private const string Prefix = "http://localhost:{0}/";

        /// <summary>
        /// The key for the action that is stored in the
        /// request.QueryString.
        /// </summary>
        private const string ActionKey = "action";

        /// <summary>
        /// The key for the id of the credential.
        /// </summary>
        private const string CredentialIdKey = "credentialId";

        /// <summary>
        /// The key for the authentication token of the credential.
        /// </summary>
        private const string CredentialAuthTokenKey = "credentialAuthToken";

        /// <summary>
        /// The key for the anti-forgery token.
        /// </summary>
        private const string AntiForgeryTokenKey = "antiForgeryToken";

        /// <summary>
        /// The string identifying that the action is an
        /// authentication.
        /// </summary>
        private const string AuthenticationIdentifier = "auth";

        /// <summary>
        /// The string identifying that the action is an update.
        /// </summary>
        private const string UpdateIdentifier="update";

        /// <summary>
        /// This event is raised when the client receives
        /// credentials from the browser.
        /// </summary>
        public event EventHandler<AuthenticationReceivedEventArgs> AuthenticationReceived;

        /// <summary>
        /// This event is raised when the user has decided to
        /// change the project to work on and to interrupt the
        /// calculation of the package which is currently being
        /// calculated.
        /// </summary>
        public event EventHandler UpdateRequested;

        /// <summary>
        /// Gets the port the Listener object is listening at.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="port">The port the Listener object is
        /// listening at.</param>
        public Listener(int port)
        {
            this.Port = port;
            this.isRunning = false;
            o = new Object();
        }

        /// <summary>
        /// The run method of the thread.
        /// It blocks while listening at the port for an incoming
        /// message.
        /// </summary>
        private void Run()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(String.Format(Prefix, Port));
            listener.Start();

            while (isRunning)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                }
                catch (HttpListenerException)
                {
                    //This exception might occur if the listener is
                    //shut down.
                    break;
                }
                catch (System.IO.IOException)
                {
                    //This exception might occur if the listener is
                    //shut down.
                    break;
                }
                //This lock ensures that the listener will only be
                //interrupted, if it is waiting for a context.
                lock(o)
                {
                    if (!isRunning)
                    {
                        break;
                    }
                    //getting a new request
                    HttpListenerRequest request = context.Request;
                    string action = request.QueryString[ActionKey];

                    if (action == AuthenticationIdentifier)
                    {
                        //the browser sended authentication data
                        Credential credential = new Credential()
                        {
                            AuthenticationToken = request.QueryString[CredentialAuthTokenKey],
                            UserId = Int32.Parse(request.QueryString[CredentialIdKey])
                        };
                        String antiForgeryToken = request.QueryString[AntiForgeryTokenKey];
                        //the data has been extracted, now the event is raised
                        if (AuthenticationReceived != null)
                        {
                            AuthenticationReceived(this, new AuthenticationReceivedEventArgs(credential, antiForgeryToken));
                        }
                    }
                    else if (action == UpdateIdentifier)
                    {
                        //the browser sended an project update request and now the event is raised
                        if (UpdateRequested != null)
                        {
                            UpdateRequested(this, EventArgs.Empty);
                        }
                    }
                    context.Response.OutputStream.Close();
                }
            }
            listener.Stop();
        }

        /// <summary>
        /// Starts to listen for browser calls on localhost.
        /// </summary>
        public void StartListen()
        {
            if (worker != null)
            {
                //todo beschreibung
                throw new InvalidOperationException("The listener is already listening.");
            }
            isRunning = true;
            worker = new Thread(Run);
            worker.Start();
        }

        /// <summary>
        /// Stops listening for calls.
        /// </summary>
        public void StopListen()
        {
            if (worker == null)
            {
                //todo beschreibung
                throw new InvalidOperationException("The listener is not listening.");
            }
            isRunning = false;
            if (listener.IsListening)
            {
                lock (o)
                {
                    listener.Stop();
                }
            }
            worker.Join();
            worker = null;
        }
    }
}
