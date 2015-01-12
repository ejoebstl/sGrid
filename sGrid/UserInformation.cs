using System;
using System.Collections.Generic;
using System.Text;

namespace sGrid
{
    /// <summary>
    /// This class represents user credentials. 
    /// </summary>
    [Serializable]
    public class UserInformation
    {
        /// <summary>
        /// Gets the auth token of the user.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Empty constructor, so this class can be de-serialzed. 
        /// </summary>
        public UserInformation()
        {

        }

        /// <summary>
        /// Creates a new instance of this class and stores the given parameters. 
        /// </summary>
        /// <param name="authToken">The auth token of the user.</param>
        /// <param name="userId">The id of the user.</param>
        public UserInformation(string authToken, int userId)
        {
            this.AuthToken = authToken;
            this.UserId = userId;
        }
    }
}
