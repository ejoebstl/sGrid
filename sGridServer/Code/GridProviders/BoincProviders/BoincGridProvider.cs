using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Xml;

namespace sGridServer.Code.GridProviders.BoincProviders
{
    /// <summary>
    /// Provides a basic interface with Boinc based Grid Projects, using the default mechanisms of boinc. 
    /// <remarks>
    /// This provider is not complete, it lacks the validation of results, which can only be done with the help of a web-callback from 
    /// the grid project. 
    /// </remarks>
    /// </summary>
    public abstract class BoincGridProvider : GridProvider
    {
        /// <summary>
        /// Gets the project (workspace) Url associated with this provider. 
        /// </summary>
        public Uri ProjectUrl { get; protected set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="description">The GridProviderDescription associated with this instance.</param>
        protected BoincGridProvider(GridProviderDescription description) : base(description)
        {
            ProjectUrl = new Uri(description.WorkspaceUrl);
        }

        /// <summary>
        /// The Web-RPC call for account creation. 
        /// <remarks>
        /// Documentation by Boinc: http://boinc.berkeley.edu/trac/wiki/WebRpc.
        /// </remarks>
        /// </summary>
        private const string createAcountCall = "createAccount.php?email_addr={0}&passwd_hash={1}&user_name={2}";

        /// <inheritdoc />
        protected override DataAccessLayer.Models.GridProviderAuthenticationData RegisterUserWithProjectServer(DataAccessLayer.Models.User u)
        {
            // Generate a new random number, which can be used as password for registration. 
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[32];
            rng.GetBytes(data);
            rng.Dispose();

            string randomPassword = Convert.ToBase64String(data);
           
            // Create a digest, containing of username and password, like Boinc wishes to have. 
            string digest = u.EMail.ToLower() + randomPassword;

            // Hash the digest, according to Boinc specification. 
            byte[] digestBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(digest);
            MD5 hasher = MD5.Create();

            hasher.Initialize();
            byte[] hashBytes = hasher.ComputeHash(digestBytes);
            string hash = BitConverter.ToString(digestBytes).Replace("-", "");

            // Fromat the API-Call
            string apiCall = String.Format(createAcountCall, u.EMail, hash, u.Nickname);

            Uri apiCallUri = new Uri(ProjectUrl, apiCall);

            // Call the RPC and load the response. 
            XmlDocument reply = new XmlDocument();
            reply.Load(apiCallUri.ToString());

            // If there was an error, throw. 
            CheckForError(reply);

            // Else, gather the received access token. 
            string accessToken = "";

            if (reply.Name == "account_out")
            {
                accessToken = reply["authenticator"].Value;
            } 
            else
            {
                throw new InvalidOperationException("Invalid reply: Expected account_out");
            }

            // Return the result.
            return new DataAccessLayer.Models.GridProviderAuthenticationData()
            {
                AuthenticationInfo = accessToken, //TODO - Check whether this is correct. 
                ProviderId = this.Description.Id,
                User = u,
                UserId = u.Id,
                UserName = u.Nickname,
                UserToken = accessToken,
                Password = randomPassword
            };
        
        }

        /// <summary>
        /// Checkes if the given nodes corresponds to an error, and throws an exception containing the error description, if so.
        /// </summary>
        /// <param name="node">The node to check.</param>
        private void CheckForError(XmlNode node)
        {
            if (node.Name == "Error")
            {
                throw new InvalidOperationException("The api call resulted in an error: " +
                    node["error_num"].Value + " - " + node["error_string"].Value);
            }
        }

        /// <inheritdoc />
        protected override void RemoveUserFromProjectServer(DataAccessLayer.Models.User u, DataAccessLayer.Models.GridProviderAuthenticationData data)
        {
            throw new NotImplementedException("Un-Registering a user is not supported by Boinc Projects");
        }
    }
}