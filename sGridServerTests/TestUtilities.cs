using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.SessionState;
using System.Reflection;

namespace sGridServerTests
{
    /// <summary>
    /// Offers methods which help with testing. 
    /// </summary>
    static class TestUtilities
    {
        /// <summary>
        /// Creates a fake HTTP context for testing. 
        /// </summary>
        /// <returns>A fake HTTP context.</returns>
        public static HttpContext FakeHttpContext()
        {
            HttpRequest httpRequest = new HttpRequest("", "http://ipd.kit.edu/", "");
            StringWriter stringWriter = new StringWriter();
            HttpResponse httpResponce = new HttpResponse(stringWriter);
            HttpContext httpContext = new HttpContext(httpRequest, httpResponce);

            HttpSessionStateContainer sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }
    }
}
