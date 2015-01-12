using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Code.GridProviders;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Models;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller handels client login requests.
    /// </summary>
    public class ClientLoginController : Controller
    {
        //Constants for session access.
        private const string AntiForgeryTokenSessionKey = "ClientLoginControllerAntiForgeryToken";
        private const string PortSessionKey = "ClientLoginControllerPort";
        private const string ExpireDateSessionKey = "ClientLoginControllerExpiration";

        /// <summary>
        /// Returns a boolean indicating whether the login data for the 
        /// current session is expired.
        /// </summary>
        private bool IsExpired
        {
            get
            {
                DateTime? expireDate = (DateTime?)Session[ExpireDateSessionKey];

                return expireDate == null || expireDate.Value < DateTime.Now;
            }
        }

        /// <summary>
        /// Stores or retreives the anti forgery token from or to the session. 
        /// </summary>
        private string AntiForgeryToken
        {
            get
            {
                return (string)Session[AntiForgeryTokenSessionKey];
            }
            set
            {
                Session[AntiForgeryTokenSessionKey] = value;
            }
        }

        /// <summary>
        /// Stores or retreives the client port from or to the session. 
        /// </summary>
        private int Port
        {
            get
            {
                return ((int?)Session[PortSessionKey]).Value;
            }
            set
            {
                Session[PortSessionKey] = new Nullable<int>(value);
            }
        }

        /// <summary>
        /// Clears the login data for the 
        /// current session.
        /// </summary>
        private void ClearState()
        {
            Session[ExpireDateSessionKey] = null;
            Session[AntiForgeryTokenSessionKey] = null;
            Session[PortSessionKey] = null;
        }

        /// <summary>
        /// If a user is logged in, returns the index view, else returns a redirect
        /// to the Login action method. 
        /// </summary>
        /// <param name="token">The anti-forgery-token to use.</param>
        /// <param name="port">The port where the client is listening for.</param>
        /// <param name="disableLayout">If set to true, the layout of all 
        /// login related view is disabled using the session.</param>
        /// <param name="resetSession">If true, abadons the user's session before continuing.</param>
        /// <returns>One of the results described above.</returns>
        public ActionResult Index(string token, int? port, bool disableLayout = false, bool resetSession = false)
        {
            if (resetSession)
            {
                SecurityProvider.ClearUserContext();
            }

            if (disableLayout)
            {
                Session["ClientLayout"] = "";
            }


            //If we have no token or no port and our data in 
            //the session is expired, we have to fail.
            if (token != null && port != null)
            {
                Port = port.Value;
                AntiForgeryToken = token;

                Session[ExpireDateSessionKey] = new Nullable<DateTime>(DateTime.Now.AddMinutes(10));
            }
            else if (IsExpired)
            {
                throw new InvalidOperationException();
            }

            //If we have no user, we have to force a login.
            if (!(SecurityProvider.CurrentUser is User))
            {
                return RedirectToAction("Login");
            }

            //If all is fine, return a view. 
            return View();
        }

        /// <summary>
        /// Shows the login view or returns a redirect, 
        /// if a controller name is given. 
        /// </summary>
        /// <returns>One of the results described above.</returns>
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Gets the authentication information for the current user.
        /// </summary>
        /// <returns>The authentication information, encoded as JSON.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public JsonResult GetAuthToken()
        {
            User current = SecurityProvider.CurrentUser as User;

            if (IsExpired)
            {
                //If our data is expired, return a failure. 
                ClearState();
                return Json(new ClientLoginResponse()
                {
                    Expired = true
                });
            }
            else
            {
                //Else, return the auth data. 
                return Json(new ClientLoginResponse()
                {
                    AntiForgeryToken = this.AntiForgeryToken,
                    Port = this.Port,
                    Id = current.Id,
                    AuthToken = current.AuthenticationToken,
                    Expired = false
                });
            }
        }
    }
}
