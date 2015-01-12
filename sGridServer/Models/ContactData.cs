using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Models
{
    /// <summary>
    /// This class represents the data to contact sGrid team.
    /// </summary>
    public class ContactData
    {
        /// <summary>
        /// Subject of the message
        /// </summary>
        public String Subject { get; set; }

        /// <summary>
        /// Text of Message
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// Constructor of this class.
        /// </summary>
        public ContactData()
        {
            this.Subject = "";
            this.Message = "";
        }
    }
}