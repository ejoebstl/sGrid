using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The user help interface shows user help and user FAQ pages.
    /// This controller is responsible for managing the user help views.
    /// </summary>
    public class UserHelpController : Controller
    {
        /// <summary>
        /// Shows the UserFAQView, which is responsible for showing the FAQ section for registered users. 
        /// </summary>
        /// <returns>The UserFAQView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult UserHelp()
        {
            return View();
        }

        /// <summary>
        /// Shows the UserHelpView, which is responsible for showing the help section for registered users.
        /// </summary>
        /// <returns>The UserHelpView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult UserFAQ()
        {
            return View();
        }

    }
}
