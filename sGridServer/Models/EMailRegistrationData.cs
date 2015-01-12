using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for registrations. 
    /// </summary>
    public class EMailRegistrationData
    {
        /// <summary>
        /// The captcha used for input verification.
        /// </summary>
        public Captcha Captcha { get; set; }

        /// <summary>
        /// The data of the user to register.
        /// </summary>
        public User UserData { get; set; }

        /// <summary>
        /// The password of the user to register.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        /// <summary>
        /// The password confirmation of the user to register.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string PasswordConfirmation { get; set; }
    
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public EMailRegistrationData()
        {
            UserData = new User();
            Password = "";
        }

    }
}