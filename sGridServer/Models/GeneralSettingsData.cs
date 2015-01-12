using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Models
{
    /// <summary>
    /// The GeneralSettingsData class represents a data used for general settings of the user.
    /// </summary>
    public class GeneralSettingsData
    {
        /// <summary>
        /// The Nickname of the user.
        /// </summary>
        [MinLength(1)]
        [Required]
        public String Nickname { get; set; }
        /// <summary>
        /// Is true, if thee additional password changing fields should be shown
        /// </summary>
        public bool ShowAdditional { get; set; }
        /// <summary>
        /// The password of the user.
        /// </summary>
        [DataType(DataType.Password)]
        public String PasswordOld { get; set; }
        /// <summary>
        /// The password of the user.
        /// </summary>
        [MinLength(6)]
        [DataType(DataType.Password)]
        public String PasswordFirst { get; set; }
        /// <summary>
        /// The password of the user.
        /// </summary>
        [MinLength(6)]
        [DataType(DataType.Password)]
        public String PasswordSecond { get; set; }
    }
}