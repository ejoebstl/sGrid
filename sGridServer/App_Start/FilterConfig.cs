using System.Web;
using System.Web.Mvc;

namespace sGridServer
{
    /// <summary>
    /// auto-generated class
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers the filters.
        /// </summary>
        /// <param name="filters">The filters to add.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}