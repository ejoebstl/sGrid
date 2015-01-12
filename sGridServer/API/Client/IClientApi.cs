using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGrid
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    /// <summary>
    /// The contract describing which function can be called by the
    /// client to communicate with the server.
    /// </summary>
    [ServiceContract]
    public interface IClientApi
    {
        /// <summary>
        /// Gets information about the grid project the client
        /// should attach to and also returns authentication data
        /// for the communication with the grid provider.
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <returns>Information and an authentication token for
        /// the project the client should attach to.</returns>
        [OperationContract]
        ProjectData GetCurrentProject(Credential authentication);

        /// <summary>
        /// Gets information and authentication data about the grid
        /// project the client has to authenticate at.
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <param name="projectName">The name identifying the
        /// project the token is wanted for</param>
        /// <returns>Information and authentication tokens for the
        /// project the client has to authenticate to.</returns>
        [OperationContract]
        ProjectData GetTokenForProject(Credential authentication, String projectName);

        /// <summary>
        /// Gets the User object associated with the authentication
        /// credentials. 
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <returns>The User object associated with the Credential.</returns>
        [OperationContract]
        User GetUser(Credential authentication);

        /// <summary>
        /// Notifies the server that the client has finished
        /// calculating a package and sends a StatusMessage.
        /// </summary>
        /// <param name="message">A message giving information
        /// about the client state and the calculated package.</param>
        [OperationContract]
        void WorkUnitFinished(StatusMessage message);

        /// <summary>
        /// Tests if the client is in an consistant state.
        /// That is to say: if the credential is correct
        /// and the clientversion is the newest one.
        /// </summary>
        /// <param name="authentication">The credential to be tested.</param>
        /// <param name="clientversion">The client version actually
        /// used by this specific
        /// client.</param>
        /// <returns>True, if the credential is associated with a user.</returns>
        [OperationContract]
        TestResult TestClient(Credential authentication, int clientversion);
    }

    /// <summary>
    /// Enumeration specifying credential test results.
    /// </summary>
    public enum TestResult
    {
        /// <summary>
        /// The credential was correct.
        /// </summary>
        Correct,
        /// <summary>
        /// The credential was not correct.
        /// </summary>
        WrongCredential,
        /// <summary>
        /// The client was out of date.
        /// </summary>
        OldClientVersion
    }
}
