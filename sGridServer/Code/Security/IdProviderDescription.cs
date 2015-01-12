using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// This class describes an IdProvider and is capable of generating 
    /// urls which point to the corresponding IdProviderController. This way, 
    /// the user interface can invoke authentication or registration of a user
    /// </summary>
    public class IdProviderDescription
    {
        /// <summary>
        /// Gets the name of the id provider controller.
        /// </summary>
        public string ControllerName { get; private set; }

        /// <summary>
        /// Gets the url of the icon associated with this id provider.
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets the display name of this id provider.
        /// </summary>
        public MultiLanguageString ProviderName { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the parameters into their corresponding properties.
        /// </summary>
        /// <param name="controllerName">The name of the id provider controller.</param>
        /// <param name="icon">The url of the icon associated with this id provider.</param>
        /// <param name="providerName">The display name of this id provider.</param>
        public IdProviderDescription(string controllerName, string icon, MultiLanguageString providerName)
        {
            this.ControllerName = controllerName;
            this.IconUrl = icon;
            this.ProviderName = providerName;
        }

        /// <summary>
        /// Returns a url which points to the authentication method of the associated IdProviderController, 
        /// including the given parameters. 
        /// </summary>
        /// <param name="returnUrl">The url the IdProviderController should return to after authentication. </param>
        /// <param name="context">ControllerContext - The System.Web.Mvc.ControllerContext object of the calling controller.
        /// This is needed to generate relative urls.</param>
        /// <returns>A url which points to the authentication method of the associated IdProviderController.</returns>
        public string GetAuthenticationUrl(string returnUrl, ControllerContext context)
        {
            UrlHelper helper = new UrlHelper(context.RequestContext);

            return helper.Action("StartAuthentication", ControllerName, new { returnUrl = returnUrl });
        }

        /// <summary>
        /// Returns a url which points to the authentication method 
        /// of the associated IdProviderController, including the given 
        /// parameters and a token which refers username and password for direct 
        /// login. Username and password are stored in the session and can be gathered
        /// by the target controller using the token. Calling this method is only 
        /// valid for the default id provider, which uses an e-mail address and 
        /// password for login. 
        /// </summary>
        /// <param name="username">The username to send to the target controller.</param>
        /// <param name="password">The password to send to the target controller.</param>
        /// <param name="returnUrl">The url the IdProviderController should return to after authentication. </param>
        /// <param name="context">The System.Web.Mvc.ControllerContext object of the calling controller. This is needed to generate relative urls. </param>
        /// <returns>A url which points to the authentication method of the associated IdProviderController.</returns>
        public string GetAuthenticationUrl(string username, string password, string returnUrl, ControllerContext context)
        {
            //Todo Emi - this is a optional target
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a url which points to the registration method of the associated 
        /// IdProviderController, including the given parameters. 
        /// </summary>
        /// <param name="returnUrl">The url the IdProviderController should return to after registration. </param>
        /// <param name="context">The System.Web.Mvc.ControllerContext object of the calling controller. This is needed to generate relative urls. </param>
        /// <param name="inviterId">The id of the user who sent this registration url.</param>
        /// <returns>A url which points to the registration method of the associated IdProviderController.</returns>
        public string GetRegistrationUrl(string returnUrl, ControllerContext context, int inviterId = -1)
        {
            UrlHelper helper = new UrlHelper(context.RequestContext);

            return helper.Action("StartRegistration", ControllerName, new { returnUrl = returnUrl, inviter = inviterId });
        }

        /// <summary>
        /// Returns a url which points to the password recovery method of the associated IdProviderController, including the given parameters. 
        /// </summary>
        /// <param name="returnUrl">The url the IdProviderController should return after password recovery. </param>
        /// <param name="context">The System.Web.Mvc.ControllerContext of the calling controller. This is needed to generate relative urls. </param>
        /// <returns>A url which points to the password recovery method of the associated IdProviderController.</returns>
        public string GetResetPasswordUrl(string returnUrl, ControllerContext context)
        {
            UrlHelper helper = new UrlHelper(context.RequestContext);

            return helper.Action("ResetPassword", ControllerName, new { returnUrl = returnUrl });
        }
    }
}