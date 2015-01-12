using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Models
{
    /// <summary>
    /// A model class used for password recovery.
    /// </summary>
    public class PasswordRecoveryMailData
    {
        /// <summary>
        /// The e-mail address of the user.
        /// </summary>
        /// 
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Address { get; set; }

        /// <summary>
        /// The captcha used for input verification.
        /// </summary>
        public Captcha Captcha { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public PasswordRecoveryMailData()
        {
            Address = "";
        }
    }
}