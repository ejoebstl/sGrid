using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a friendship between users
    /// </summary>
    public class Friendship
    {
        /// <summary>
        /// Gets or sets the id of the friendship.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets a boolean indicating whether the second user has been invited by the first user.
        /// </summary>
        public bool WasInvited { get; set; }


        /// <summary>
        /// Gets or sets the id of the second user in this friendship relation.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the id of the first user in this friendship relation.
        /// </summary>
        [ForeignKey("Friend")]
        public int FriendId { get; set; }


        /// <summary>
        /// Gets or sets the second user in this friendship relation.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets the first user in this friendship relation.
        /// </summary>
        public virtual User Friend { get; set; }
    }
}