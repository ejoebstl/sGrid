using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;

namespace sGridServer.Controllers
{
    /// <summary>
    /// Provides methods which help with login and registration procedures.
    /// </summary>
    public class IdProviderHelperController : Controller
    {
        /// <summary>
        /// Key for the inviter id contained within the session. 
        /// </summary>
        private const string InviterSessionkey = "IdProviderHelperInviter";

        /// <summary>
        /// Gets or sets the id of the invitor.
        /// </summary>
        private int InviterId
        {
            get
            {
                int? value = (int?)Session[InviterSessionkey];
                return value != null ? value.Value : -1;
            }
            set
            {
                Session[InviterSessionkey] = value;
            }
        }

        /// <summary>
        /// Shows a view showing all available login methods. 
        /// </summary>
        /// <param name="returnUrl">The url to return to, after the login process finished, or null, to auto-generate the url.</param>
        /// <returns>A view.</returns>
        public PartialViewResult LoginView(string returnUrl = null)
        {
            if (returnUrl == null)
            {
                returnUrl = this.Request.Url.ToString();
            }
            return PartialView((object)returnUrl);
        }

        /// <summary>
        /// Shows a view which notifies the user that his account is inactivated.
        /// </summary>
        /// <returns>A view.</returns>
        public ViewResult Inactive()
        {
            return View();
        }

        /// <summary>
        /// Shows a view showing all available registration methods. 
        /// </summary>
        /// <param name="returnUrl">The url to return to, after the registration process finished, or null, to auto-generate the url.</param>
        /// <returns>A view.</returns>
        public PartialViewResult RegisterView(string returnUrl = null)
        {
            if (returnUrl == null)
            {
                returnUrl = this.Request.Url.ToString();
            }
            return PartialView((object)returnUrl);
        }

        /// <summary>
        /// Shows the invitation landing view.
        /// </summary>
        /// <param name="inviterId">The id of the invitor.</param>
        /// <returns>A view.</returns>
        public ViewResult InvitationLanding(int inviterId)
        {
            MemberManager manager = new MemberManager();

            InviterId = inviterId;

            return View(manager.GetAccountById(inviterId));
        }

        /// <summary>
        /// Starts a login action.
        /// </summary>
        /// <param name="controllerName">The name of the controller to perform the login with.</param>
        /// <param name="returnUrl">The return url to return after login.</param>
        /// <returns>A redirect to the login page.</returns>
        public ActionResult Login(string controllerName, string returnUrl)
        {
            return Redirect(IdProviderManager.GetRegisteredProviders().
                Where(x => x.ControllerName == controllerName).First().
                GetAuthenticationUrl(returnUrl, this.ControllerContext));
        }

        /// <summary>
        /// Starts a registration action.
        /// </summary>
        /// <param name="controllerName">The name of the controller to perform the registration with.</param>
        /// <param name="returnUrl">The return url to return after registration.</param>
        /// <returns>A redirect to the registration page.</returns>
        public ActionResult Register(string controllerName, string returnUrl)
        {
            if (InviterId != -1)
            {
                returnUrl = Url.Action("UserDashboard", "UserDashboard");
            }
            return Redirect(IdProviderManager.GetRegisteredProviders().
                Where(x => x.ControllerName == controllerName).First().
                GetRegistrationUrl(returnUrl, this.ControllerContext, InviterId));
        }

    }
}
