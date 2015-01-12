using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// This class is a method attribute to use for authentication. 
    /// </summary>
    public class SGridAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute 
    {
        /// <summary>
        /// Gets or sets the permissions which are needed to access
        /// the corresponding action method. 
        /// </summary>
        public SiteRoles RequiredPermissions { get; set; }

        /// <summary>
        /// Is called by the super class System.Web.Mvc.AuthorizeAttribute when 
        /// authentication is requested. The method checks whether the 
        /// user stored in the session has sufficient permissions. 
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A bool indicating whether the current user has sufficient permissions. </returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            Account current = SecurityProvider.CurrentUser;

            return current != null && current.Active && (current.UserPermission & RequiredPermissions) > 0;
        }
    }
}