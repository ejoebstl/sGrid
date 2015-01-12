using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.DataAccessLayer.Models;
using System.IO;
using sGridServer.Models;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the public views.
    /// </summary>
    public class PublicController : Controller
    {
        /// <summary>
        /// Shows the Index page of the website
        /// </summary>
        /// <returns>The IndexView</returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Shows the Client Download page
        /// </summary>
        /// <returns>The ClientDownloadView</returns>
        public ActionResult ClientDownload()
        {
            return View();
        }

        /// <summary>
        /// Shows the Join view.
        /// </summary>
        /// <returns>The Join view.</returns>
        public ActionResult Join()
        {
            return View();
        }

        /// <summary>
        /// Shows the enter passcode view.
        /// </summary>
        /// <returns>The enter passcode view.</returns>
        public ActionResult EnterPasscode(string passcode, string returnUrl)
        {
            if (passcode == null || passcode == "")
            {
                return View((object)returnUrl);
            }
            else
            {
                Session["temporary_passcode"] = passcode;
                return Redirect(returnUrl);
            }
        }
    }
}
