using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    /// <summary>
    /// Represents the response data for a client login request.
    /// </summary>
    public class ClientLoginResponse
    {
        /// <summary>
        /// The auth token to transfer to the client. 
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// The id of the user associated with the given auth token. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The anti forgery token submitted to us by the client to prevent Cross-Site-Request-Forgery.
        /// </summary>
        public string AntiForgeryToken { get; set; }

        /// <summary>
        /// The port to send the auth token to at the client site. 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// A bool indicating whether the login data is already expired. 
        /// </summary>
        public bool Expired { get; set; }
    }
}