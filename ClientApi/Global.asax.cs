using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using sGridServer.Code.Utilities;
using sGridServer.Code.GridProviders;

namespace sGrid
{
    /// <summary>
    /// Main application class. 
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Executed on application start.
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            //Start up language manager.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(LanguageManager).TypeHandle);

            //Start up grid provider manager. 
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(GridProviderManager).TypeHandle);
        }
    }
}