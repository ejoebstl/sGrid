using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace sGridServer
{
    /// <summary>
    /// Auto generated web api configuration file. 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the api routes. 
        /// </summary>
        /// <param name="config">The configuration to register the API routes to.</param>
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
