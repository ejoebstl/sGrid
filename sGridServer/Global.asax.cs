using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using sGridServer.Code.GridProviders;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.GridProviders.WorldCommunityGrid;

namespace sGridServer
{
    /// <summary>
    /// Main application class. 
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Executed on application start.
        /// </summary>
        protected void Application_Start()
        {
            //MVC specific initialization
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Start up language manager.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(LanguageManager).TypeHandle);

            //Start up grid provider manager. 
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(GridProviderManager).TypeHandle);

            //Register id providers
            IdProviderManager.RegisterIdProvider(sGridServer.Controllers.EMailIdProviderController.Description);
            IdProviderManager.RegisterIdProvider(sGridServer.Controllers.FacebookIdProviderController.Description);

            //Start up notification mailer. 
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(NotificationMailer).TypeHandle);
        }

        /// <summary>
        /// Executed when the request state is initialized. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args.</param>
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            LanguageManager.SetThreadCulture();

            if (Properties.Settings.Default.ApplicationPasscode != "")
            {
                if ((HttpContext.Current == null || HttpContext.Current.Session == null || (string)HttpContext.Current.Session["temporary_passcode"] != Properties.Settings.Default.ApplicationPasscode) &&
                    System.IO.Path.GetFileName(Request.Url.LocalPath) != "EnterPasscode" && !System.IO.Path.GetFileName(Request.Url.LocalPath).Contains('.'))
                {
                    Response.Redirect("~/Public/EnterPasscode?returnUrl=" + HttpUtility.UrlEncode(Request.Url.ToString()), true);
                }
            }
        }

    }
}
