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
    /// This class is a model class for a password reset. 
    /// </summary>
    public class ResetPasswordData
    {
        /// <summary>
        /// The captcha used for input verification.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The new password of the user.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        /// <summary>
        /// The confirmation of new password of the user.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string PasswordConfirmation { get; set; }
    
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public ResetPasswordData()
        {
            Password = "";
            PasswordConfirmation = "";
            UserId = -1;
        }

    }
}