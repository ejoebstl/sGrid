using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a friend request. 
    /// </summary>
    public class FriendRequest
    {
        /// <summary>
        /// Gets or sets the id of the friend request.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets a boolean indicating whether the friendship request was rejected.
        /// </summary>
        public bool Rejected { get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the friendship request was sent.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Gets or sets the account of the user who sent the friendship request.
        /// </summary>
        [ForeignKey("Requester")]
        public int RequesterId { get; set; }

        /// <summary>
        /// Gets or sets the account id of the user who is the receiver of the friendship request.
        /// </summary>
        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; }


        /// <summary>
        /// Gets or sets the user who is the receiver of the friendship request.
        /// </summary>
        public virtual User Receiver { get; set; }

        /// <summary>
        /// Gets or sets the user who sent the friendship request.
        /// </summary>
        public virtual User Requester { get; set; }


        /// <summary>
        /// A default constructor of the friend request.
        /// </summary>
        public FriendRequest()
        {
            this.Timestamp = DateTime.Now;
        }
    }
}