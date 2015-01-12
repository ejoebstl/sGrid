using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for account configuration. 
    /// </summary>
    public class AccountConfigurationModel
    {
         /// <summary>
        /// The user associated with the configuration model.
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// The site role of the user
        /// </summary>
        public String SiteRole { get; set; }
        /// <summary>
        /// Creates a new instance of this class with a given user.
        /// </summary>
        /// <param name="user">Provided user to associate this model with.</param>
        public AccountConfigurationModel(Account user)
        {
            Account = user;
            SiteRole = user.UserPermission.ToString();
        }
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public AccountConfigurationModel()
        {
            SiteRole = "";
        }
    }
}