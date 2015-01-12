using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// This class holds arguments for the FriendshipAdded event. 
    /// </summary>
    public class FriendshipAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the first user of the friendship relation.
        /// </summary>
        public User A { get; private set; }

        /// <summary>
        /// Gets the second user of the friendship relation.
        /// </summary>
        public User B { get; private set; }

        /// <summary>
        /// Gets a bool specifying whether user b was invited by user a. 
        /// </summary>
        public bool WasInvited { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and copies the parameters into their corresponding properties.
        /// </summary>
        /// <param name="a">The first user of the friendship relation.</param>
        /// <param name="b">The second user of the friendship relation.</param>
        /// <param name="wasInvited">A bool specifying whether user b was invited by user a. </param>
        public FriendshipAddedEventArgs(User a, User b, bool wasInvited)
        {
            this.A = a;
            this.B = b;
            this.WasInvited = wasInvited;
        }
    }
}