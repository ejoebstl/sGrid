using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The about user interface shows the pages about sGrid and what we do, sGrid award system and grid computing in general.
    /// </summary>
    public class AboutController : Controller
    {
        /// <summary>
        /// Shows the view of the website page which shortly describes the sGrid team and the project 
        /// and gives an overview about how to use sGrid system.
        /// </summary>
        /// <returns>The AboutUsView.</returns>
        public ActionResult AboutUs()
        {
            return PartialView();
        }

        /// <summary>
        /// Shows the view with the information about the sGrid award system.
        /// </summary>
        /// <returns>The AwardSystemView.</returns>
        public ActionResult AwardSystem()
        {
            return PartialView();
        }

        /// <summary>
        /// Shows the AboutView.
        /// </summary>
        /// <returns>The AboutView.</returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Shows the GridComputingView which provides information about grid computing in general.
        /// </summary>
        /// <returns>The GridComputigView.</returns>
        public ActionResult GridComputing()
        {
            return PartialView();
        }

        /// <summary>
        /// Shows the view with the history of the sGrid project.
        /// </summary>
        /// <returns>The SGridHistoryView.</returns>
        public ActionResult SGridHistory()
        {
            return PartialView();
        }

    }
}
