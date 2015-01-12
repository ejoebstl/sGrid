using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the id of the message.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the email address of the sender of the message. 
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public String EMail { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        public String MessageText { get; set; }


        /// <summary>
        /// Gets or sets a bool indicating whether the message was resolved by the sGrid team.
        /// </summary>
        public bool Resolved { get; set; }

        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        public String Subject{ get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the message was submitted.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Gets or sets the id of the account associated with the message.
        /// </summary>
        [ForeignKey("Account")]
        public int? AccountId { get; set; }


        /// <summary>
        /// Gets or sets the account associated with the message. 
        /// </summary>
        public virtual Account Account { get; set; }


        /// <summary>
        /// A default constructor of the message.
        /// </summary>
        public Message()
        {
            this.EMail = "";
            this.MessageText = "";
            this.Subject = "";
            this.Timestamp = DateTime.Now;
        }
    }
}