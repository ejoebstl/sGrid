using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace sGrid
{
    /// <summary>
    /// This class is used to identify a client at the server.
    /// </summary>
    [DataContract]
    public class Credential
    {
        /// <summary>
        /// Gets the String representing the key needed for
        /// authentication.
        /// </summary>
        [DataMember]
        public String AuthenticationToken { get; private set; }

        /// <summary>
        /// Gets the identifier of the user.
        /// </summary>
        [DataMember]
        public int UserId { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the
        /// given parameters into the corresponding properties.
        /// </summary>
        /// <param name="authentication">The String representing
        /// the key needed for authentication.</param>
        /// <param name="userId">Gets the identifier of the user.</param>
        public Credential(String authentication, int userId)
        {
            this.AuthenticationToken = authentication;
            this.UserId = userId;
        }
    }
}