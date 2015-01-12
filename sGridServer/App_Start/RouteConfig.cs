using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace sGridServer
{
    /// <summary>
    /// Auto generated route config file.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers default routes.
        /// </summary>
        /// <param name="routes">The route collection to register the routes to.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.IgnoreRoute("API/Client/ClientApi.svc/{*pathInfo}");
            RouteTable.Routes.IgnoreRoute("API/Grid/GridPartnerAPI.svc/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Public", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}