using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// This class holds arguments for the UserCreated event. 
    /// </summary>
    public class UserCreadedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the created user. 
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and copies the 
        /// parameters into their corresponding properties.
        /// </summary>
        /// <param name="user">The created user. </param>
        public UserCreadedEventArgs(User user)
        {
            this.User = user;
        }
    }
}