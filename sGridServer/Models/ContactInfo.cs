using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using sGridServer.Code.Utilities;

namespace sGridServer.Models
{
    /// <summary>
    /// This class represents a contact formular for unregistred users.
    /// </summary>
    public class ContactInfo : ContactData
    {
        /// <summary>
        /// The e-mail address of sender
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public String From { get; set; }

        /// <summary>
        /// The Captcha for the contact formular. 
        /// </summary>
        [Required]
        public Captcha Captcha { get; set; }

        /// <summary>
        /// onstructor of this class.
        /// </summary>
        public ContactInfo()
        {
            this.From = "";
            this.Message = "";
            this.Subject = "";
        }
    }
}