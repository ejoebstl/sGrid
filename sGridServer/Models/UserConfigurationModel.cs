using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for user configuration. 
    /// </summary>
    public class UserConfigurationModel
    {
        /// <summary>
        /// The user associated with the configuration model.
        /// </summary>
        public User Account { get; set; }
        /// <summary>
        /// The site role of the user
        /// </summary>
        public String SiteRole { get; set; }
        /// <summary>
        /// If set, the field for the user password is shown
        /// </summary>
        public bool ShowPasswordField { get; set; }
        /// <summary>
        /// The new password of the user
        /// </summary>
        [DataType(DataType.Password)]
        [MinLength(6)]
        public String NewPassword { get; set; }
        /// <summary>
        /// The new password of the user (confirm)
        /// </summary>
        [DataType(DataType.Password)]
        [MinLength(6)]
        public String NewPasswordConfirm { get; set; }
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public UserConfigurationModel()
        {
            SiteRole = "";
            ShowPasswordField = false;
            NewPassword = "";
            NewPasswordConfirm = "";
        }
    }
}