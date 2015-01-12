using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for profile detail. 
    /// </summary>
    public class ProfileDetailModel
    {
        /// <summary>
        /// The user associated with the profile detail model.
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// Returns true, if the AddFriendButton should be shown
        /// </summary>
        public bool ShowAddFriendButton { get; set; }
        /// <summary>
        /// Returns true, if the RemoveFriendButton should be shown
        /// </summary>
        public bool ShowRemoveFriendButton { get; set; }
        /// <summary>
        /// Number of friends of the user associated with 
        /// </summary>
        public int FriendsNumber { get; set; }
    }
}