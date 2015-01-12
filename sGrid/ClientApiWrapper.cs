using System;
using System.Collections.Generic;
using System.Text;
using sGrid.sGridClientApi;
using Resource = sGrid.Resources.ClientRes;

namespace sGrid
{
    /// <summary>
    /// This class wraps the ClientApi, so that the ClientController
    /// can use the connection to the server without special case
    /// treatment.
    /// This class tests the connection and the credential of the
    /// user.
    /// </summary>
    class ClientApiWrapper
    {
        //Todo: test if this class is necessary
        /// <summary>
        /// The clientController using this wrapper to connect to the
        /// server.
        /// </summary>
        private ClientController clientController;

        /// <summary>
        /// The clientApi represents the connection to the server and the
        /// clientApi is wrapped by this class.
        /// </summary>
        private ClientApiClient clientApi;

        /// <summary>
        /// True, if the last try to connect to the server was successful.
        /// </summary>
        public bool ConnectedToServer { get; set; }

        /// <summary>
        /// Creates a new instance of this class and initializes the
        /// values.
        /// </summary>
        /// <param name="clientController">The clientController using this
        /// class</param>
        public ClientApiWrapper(ClientController clientController)
        {
            ConnectedToServer = false;
            this.clientController = clientController;
            clientApi = new ClientApiClient();
        }

        /// <summary>
        /// Tests if the connection has been established and if the
        /// credential is correct.
        /// </summary>
        /// <returns>True, if the connection is usable.</returns>
        private bool ConnectionTest()
        {
            TestResult result;
            if (clientController.State == ClientState.LoggedOut)
                throw new ArgumentNullException("This function should only be used if a user is logged in.");

            try
            {
                result = clientApi.TestClient(MakeCredential(),
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major);

            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                //if there is no internet connection
                ConnectedToServer = false;
                return false;
            }
            catch (TimeoutException)
            {
                //if the internet connection is erroneous
                ConnectedToServer = false;
                return false;
            }

            ConnectedToServer = true;
            switch (result)
            {
                case TestResult.OldClientVersion:
                    System.Windows.Forms.MessageBox.Show(Resource.OldResourceWarning,
                        "sGrid", System.Windows.Forms.MessageBoxButtons.OK);
                    //avoid that the user can calculate on the client
                    clientController.LogoutAndStartLogin();
                    return false;
                case TestResult.WrongCredential:
                    clientController.LogoutAndStartLogin();
                    return false;
                default: //case TestResult.Correct:
                    return true;
            }
        }

        /// <summary>
        /// Creates and returns a credential for the user.
        /// </summary>
        /// <returns>The credential of the current user.</returns>
        private Credential MakeCredential()
        {
            Credential credential = new Credential();
            credential.AuthenticationToken = clientController
                .ConfigManager.CurrentUser.AuthToken;
            credential.UserId = clientController
                .ConfigManager.CurrentUser.UserId;
            return credential;
        }

        /// <summary>
        /// Gets the project the client should start calculating on.
        /// </summary>
        /// <returns></returns>
        public ProjectData GetNewProjectData()
        {
            if (!ConnectionTest()) return null;

            return clientApi.GetCurrentProject(MakeCredential());
        }

        /// <summary>
        /// Indicates to the server that the client has finished calculating.
        /// </summary>
        /// <param name="reveiveTime"></param>
        public void PackageFinished(DateTime reveiveTime)
        {
            if (!ConnectionTest()) return;

            //a package has finished calculating
            StatusMessage message = new StatusMessage();
            message.Authentication = MakeCredential();
            message.ProjectShortName = clientController.ConfigManager.CurrentProjectShortName;
            message.StartTime = reveiveTime;

            clientApi.PackageFinished(message);
        }
    }
}
