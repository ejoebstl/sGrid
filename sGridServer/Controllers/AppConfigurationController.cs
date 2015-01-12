using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using System.ComponentModel.DataAnnotations;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using sGridServer.Models;
using sGridServer.Properties;
using System.Drawing;
using Resource = sGridServer.Resources.AppConfiguration.AppConfiguration;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The App Configuration user interface enables sGrid team members to configure news items 
    /// which are shown on the dashboards of the sGrid users. 
    /// sGrid team members can also view the system errors.
    /// </summary>
    public class AppConfigurationController : Controller
    {
        /// <summary>
        /// Shows the ConfigureNewsView containing a form to create news.
        /// </summary>
        /// <returns>The ConfigureNewsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ConfigureNews()
        {
                NewsModel NewsModel = new NewsModel();
                return View(NewsModel);
        }

        /// <summary>
        /// Configures news which are shown on the dashboard of every sGrid user. 
        /// </summary>
        /// <param name="NewsModel">The news item to configure.</param>
        /// <param name="file">The news picture.</param>
        /// <returns>NewsConfirm View in case of success, returns to the ConfigureNews, otherwise.</returns>
        [HttpPost]
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ConfigureNews(NewsModel NewsModel, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            { 
                return View(NewsModel);
            } 
            else 
            {
                //news to add
                News newsToAdd = new News();
                newsToAdd.Subject = NewsModel.Subject;
                newsToAdd.Text = NewsModel.Text;

                //save news picture
                if (file != null && file.ContentLength > 0)
                {
                    BlobStorage storage = new BlobStorage("NewsContainer");

                    //resize image and save
                    System.IO.Stream image = ImageUtil.ResizeImage(file.InputStream, Settings.Default.NewsImageWidth, Settings.Default.NewsImageHeight, Color.White);
                    newsToAdd.Image = storage.StoreBlob(image);
                }

                NewsManager manager = new NewsManager();

                //add news to the database
                manager.SaveNews(newsToAdd);
                return View("NewsConfirm");
            }
        }

        /// <summary>
        /// Shows a list of system errors.
        /// </summary>
        /// <returns>The ErrorView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ShowErrors()
        {
            IEnumerable<Error> result = sGridServer.Code.Utilities.ErrorLogger.GetErrors();
            if (result.Count() != 0)
            {
                return View(result);
            }
            else
            {
                ViewBag.Message = Resource.NoErrors;
                return View();
            }
        }
    }
}
