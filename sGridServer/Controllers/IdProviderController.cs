using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using System.Net;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The abstract class IdProviderController provides the base for all other id provider classes. 
    /// It provides methods to start and finish authentication, registration and password recovery. 
    /// The implementation of the core procedures is left to the sub classes.
    /// </summary>
    public abstract class IdProviderController : Controller
    {
        /// <summary>
        /// A member manager associated with this IdProvider object.
        /// </summary>
        protected MemberManager MemberManager { get; set; }

        /// <summary>
        /// The session key of the inviter.
        /// </summary>
        private const string InviterSessionkey = "IdProviderControllerInvitingUser";

        /// <summary>
        /// The session key of the return url. 
        /// </summary>
        private const string ReturnUrlSessionKey = "IdProviderControllerReturnUrl";

        /// <summary>
        /// Gets the inviting user during registration process. 
        /// Returns null if there is no inviter. 
        /// </summary>
        protected User Inviter
        {
            get
            {
                if (Session[InviterSessionkey] != null)
                {
                    return (User)MemberManager.GetAccountById((int)Session[InviterSessionkey]);
                }
                else
                {
                    return null;
                }
            }
            private set
            {
                Session[InviterSessionkey] = value.Id;
            }
        }

        /// <summary>
        /// Gets the return url which was set when the authentication, 
        /// registration or password recovery processes started. 
        /// </summary>
        protected string ReturnUrl
        {
            get
            {
                return (string)Session[ReturnUrlSessionKey];
            }
            set
            {
                Session[ReturnUrlSessionKey] = value;
            }
        }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        protected IdProviderController()
        {
            MemberManager = new MemberManager();
        }

        /// <summary>
        /// Stores the return url and calls PerformResetPassword. 
        /// </summary>
        /// <param name="returnUrl">The url to return to after the password reset procedure finished. </param>
        /// <returns>The ActionResult object returned by the PerformResetPassword method. </returns>
        public ActionResult ResetPassword(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return PerformResetPassword();
        }

        /// <summary>
        /// Stores the return url and calls PerformAuthentication.
        /// </summary>
        /// <param name="returnUrl">The url to return to after the authentication procedure finished. </param>
        /// <returns>The ActionResult object returned by the PerformAuthentication method. </returns>
        public ActionResult StartAuthentication(string returnUrl)
        {
            ReturnUrl = returnUrl;

            return PerformAuthentication();
        }

        /// <summary>
        /// Stores the return url and calls PerformRegistration. 
        /// If inviter is set, inviter is also stored. 
        /// </summary>
        /// <param name="returnUrl">The url to return to after the registration procedure finished. </param>
        /// <param name="inviter">The id of the user who invited the user who is currently registering himself or null, if there is no inviter.</param>
        /// <returns>The ActionResult object returned by the PerformRegistration method. </returns>
        public ActionResult StartRegistration(string returnUrl, int? inviter)
        {
            ReturnUrl = returnUrl;

            if (inviter != null && inviter.Value != -1)
            {
                //If there was an inviter, remember him. 

                User account = MemberManager.GetAccountById(inviter.Value) as User;

                if (account != null)
                {
                    Inviter = account;
                }

            }

            return PerformRegistration();
        }

        /// <summary>
        /// Copies the default profile picture into the blob storage and returns the storage url. 
        /// </summary>
        /// <returns>The url to the stored picture in the blob storage.</returns>
        public static string GetDefaultProfilePicture()
        {
            return ImageUtil.GetPicturyByUrl(Properties.Settings.Default.DefaultProfilePicture,
                Properties.Settings.Default.ProfilePictureWidth,
                Properties.Settings.Default.ProfilePictureHeight,
                Properties.Settings.Default.ProfilePictureStorageContainer);
        }

        /// <summary>
        /// This method finishes the authentication for the user described by the given user context 
        /// by logging on the user and by navigating to the page specified by return url.
        /// </summary>
        /// <param name="user">The user context to log on.</param>
        /// <returns>An action result which redirects the browser to the page specified by return url. </returns>
        protected ActionResult FinishAuthentication(UserContext user)
        {
            if (!MemberManager.GetAccountById(user.ID).Active)
            {
                return Redirect(Url.Action("Inactive", "IdProviderHelper"));
            }
            SecurityProvider.LogIn(user);

            return Redirect(ReturnUrl);
        }

        /// <summary>
        /// This method finishes the password recovery process by calling 
        /// FinishAuthentication with the given user context. 
        /// </summary>
        /// <param name="user">The user context of the user who finished password recovery. </param>
        /// <returns>The action result which is returned by FinishAuthentication.</returns>
        protected ActionResult FinishPasswordReset(UserContext user)
        {
            SecurityProvider.LogIn(user);

            return Redirect(ReturnUrl);
        }

        /// <summary>
        /// This method finishes the registration process by storing the given user 
        /// object into the database. If autoLogin is set to true, a UserContext 
        /// object is created and FinishAuthentication is called to login the user. 
        /// </summary>
        /// <param name="user">The new user to store</param>
        /// <param name="autoLogin">A bool specifying if the user should be logged in. </param>
        /// <param name="password">The password to assign to the user to create.</param>
        /// <returns>An action result which redirects the browser to the page specified by the return url.</returns>
        protected ActionResult FinishRegistration(User user, bool autoLogin, string password = null)
        {
            MemberManager.CreateUser(user);
            if (password != null)
            {
                MemberManager.SetPassword(user, password);
            }
            User inviter = Inviter;
            if (inviter != null)
            {
                MemberManager.RegisterFriendship(inviter, user, true);
            }

            if (autoLogin)
            {
                return FinishAuthentication(new UserContext(user));
            }
            else
            {
                return Redirect(ReturnUrl);
            }
        }

        /// <summary>
        /// Starts the registration process, as defined in deriving classes. 
        /// </summary>
        /// <returns>An action result.</returns>
        protected abstract ActionResult PerformAuthentication();

        /// <summary>
        /// Starts the registration process, as defined in deriving classes. 
        /// </summary>
        /// <returns>An action result.</returns>
        protected abstract ActionResult PerformRegistration();

        /// <summary>
        /// Starts the password recovery procedure, as defined in deriving classes. 
        /// </summary>
        /// <returns>An action result.</returns>
        protected abstract ActionResult PerformResetPassword();
    }
}
