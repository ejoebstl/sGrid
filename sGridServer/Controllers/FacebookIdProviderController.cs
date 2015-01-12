using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using System.Security.Cryptography;
using sGridServer.Code.Security;
using Facebook;
using sGridServer.Models;
using Resource = sGridServer.Resources.IdProviders.EMailIdProvider;
using System.Net.Http;
using System.IO;
using System.Net;
using sGridServer.Code.Utilities;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This class is derived from the abstract IdProviderController class and provides authentication, 
    /// registration and password reset for users which want to use their facebook accounts for authentication. 
    /// </summary>
    public class FacebookIdProviderController : IdProviderController
    {
        /// <summary>
        /// Facebook login url format string. 
        /// Parameters: appId, redirectUrl, stateToken, permissionScope
        /// </summary>
        private const string FacebookLoginUrl = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&state={2}&scope={3}";

        /// <summary>
        /// Facebook picture url format string.
        /// Parameters: facebookUserId
        /// </summary>
        private const string FacebookPictureUrl = "http://graph.facebook.com/{0}/picture?width=600&height=600";

        //String literals needed for interaction with facebook. 
        private const string FacebookAccessTokenUrl = "oauth/access_token";
        private const string StateSessionKey = "FacebookLoginSessionState";
        private const string AuthenticationTarget = "authenticate";
        private const string RegistrationTarget = "register";

        //String literals used for session access. 
        private const string AccessTokenSessionKey = "FacebookAccessToken";
        private const string AccessTokenExpireDateSessionKey = "FacebookAccessTokenExpires";

        //Private members for storing facebook application secrets. 
        private string appId;
        private string appSecret;

        /// <summary>
        /// The unique provider identifier for FacebookIdProviderController.
        /// </summary>
        public const string ProviderIdentifier = "Facebook";

        /// <summary>
        /// Gets the Description of this provider. 
        /// </summary>
        public static IdProviderDescription Description
        {
            get
            {     
                return new IdProviderDescription("FacebookIdProvider", "~/Content/images/idProviders/facebook.png", "Facebook");
            }
        }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public FacebookIdProviderController()
        {
            appId = sGridServer.Properties.Settings.Default.FacebookAppId;
            appSecret = sGridServer.Properties.Settings.Default.FacebookAppSecret;
        }

        /// <summary>
        /// Redirects the user to the corresponding page of the facebook API, setting the OAuthLanding method as return url for this facebook API call. 
        /// </summary>
        /// <returns>A redirect action.</returns>
        protected override System.Web.Mvc.ActionResult PerformAuthentication()
        {
            return RedirectToFacebook(AuthenticationTarget);
        }

        /// <summary>
        /// Gets the facebook authentication state token, which is used against cross-site-request-forgery (CSRF). 
        /// </summary>
        private string State
        {
            get
            {
                return Session[StateSessionKey] as string;
            }
            set
            {
                Session[StateSessionKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the access token for the current user. 
        /// </summary>
        private string AccessToken
        {
            get
            {
                return Session[AccessTokenSessionKey] as string;
            }
            set
            {
                Session[AccessTokenSessionKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a timestamp which indicates when the access token expires. 
        /// </summary>
        private DateTime AccessTokenExpires
        {
            get
            {
                DateTime? value = Session[AccessTokenExpireDateSessionKey] as DateTime?;

                return value == null ? DateTime.MinValue : value.Value;
            }
            set
            {
                Session[AccessTokenExpireDateSessionKey] = value;
            }
        }

        /// <summary>
        /// Redirects the user to the corresponding page of the facebook API, setting the OAuthLanding method as return url for this facebook API call. 
        /// </summary>
        /// <returns>A redirect action.</returns>
        protected override System.Web.Mvc.ActionResult PerformRegistration()
        {
            return RedirectToFacebook(RegistrationTarget);
        }

        /// <summary>
        /// Redirects the current user to facebook, to invoke authentication or registration. 
        /// After authentication, facebook redirects the user to the OAuthLanding action method. 
        /// </summary>
        /// <param name="targetAction">The name of the action to return to after the facebook api call finished.</param>
        /// <returns>A redirect to facebook.</returns>
        private ActionResult RedirectToFacebook(string targetAction)
        {
            //Construct target url.
            string targetUrl = Url.Action("OAuthLanding", "FacebookIdProvider", new { redirectTo = targetAction }, Request.Url.Scheme);
          
            //Construct anti CSRF-token. 
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            byte[] token = new byte[64];
            rand.GetBytes(token);

            rand.Dispose();

            State = Convert.ToBase64String(token);

            //Since either FB or ASP url-encoding has problems with '+' or ' ' characters, we have to filter them out. 
            State = State.Replace(' ', '_'); 
            State = State.Replace('+', '_');

            //Generate the facebook login url using the given parameters.
            string dialogUrl = String.Format(FacebookLoginUrl, appId, targetUrl, State, "email");

            //Invoke the login.
            return Redirect(dialogUrl);
        }

        /// <summary>
        /// Redirects the user to the corresponding page of the facebook API. 
        /// </summary>
        /// <returns>A redirect action.</returns>
        protected override System.Web.Mvc.ActionResult PerformResetPassword()
        {
            //TODO Emi
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the sGrid user associated with the given facebook id to the friend list of the user who performs the current registration. 
        /// </summary>
        /// <param name="friendId">The facebook id of the friend to add.</param>
        /// <returns>An ActionResult indicating error or success.</returns>
        [HttpPost]
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult AddFriend(string friendId)
        {
            //Get the friend to generate the request for. 
            User target = MemberManager.Users.Where(x => x.IdType == ProviderIdentifier && x.AccountToken == friendId).FirstOrDefault();

            //If the friend has not been found, fail. 
            if (target == null)
            {
                return Json(false);
            }

            //Add a friend request. 
            MemberManager.AddFriendRequest(SecurityProvider.CurrentUser as User, target);

            return Json(true);
        }

        /// <summary>
        /// If the submitted user data was valid, finishes the registration process and 
        /// calls the ImportFriends action method. If not, shows the FinishRegistrationView again. 
        /// </summary>
        /// <param name="data">The user data shown in the FinishRegistrationView.</param>
        /// <returns>An action result performing one of the operations described above.</returns>
        [HttpPost]
        public ActionResult FinishFacebookRegistration(FacebookRegistrationData data)
        {
            bool error = false;

            //Check the model state. 
            if (!ModelState.IsValid)
            {
                return View(data);
            }

            //Check whether the given mail address is unique. 
            if (MemberManager.Accounts.Where(x => x.EMail == data.EMailAddress).Any())
            {
                ViewBag.EMailMessage = Resource.EMailAlreadyInUse;
                error = true;
            }

            //Check whether the given nickname is unique. 
            if (MemberManager.Accounts.Where(x => x.Nickname == data.Nickname).Any())
            {
                ViewBag.UsernameMessage = Resource.UsernameAlreadyInUse;
                error = true;
            }

            //In case of an error, show the view again. 
            if (error)
            {
                return View(data);
            }

            //Set the return url to an url pointing to the ImportFriends action method, with the current return url as return url parameter. 
            //This lets the base controller execute the ImportFriends method and then returns the user to the original return url. 
            string returnUrl = ReturnUrl;
            ReturnUrl = Url.Action("ImportFriends", new { returnUrl = returnUrl });

            //Create a new user. 
            User user = new User()
            {
                AccountToken = data.Id,
                Nickname = data.Nickname,
                Picture = data.PictureUrl,
                Culture = data.Culture,
                EMail = data.EMailAddress,
                IdType = ProviderIdentifier,
                Active = true
            };

            //Optionally, get the profile picture from facebook. 
            if (data.GetPictureFormFacebook)
            {
                user.Picture = ImageUtil.GetPicturyByUrl(data.PictureUrl,
                    Properties.Settings.Default.ProfilePictureWidth,
                    Properties.Settings.Default.ProfilePictureHeight,
                    Properties.Settings.Default.ProfilePictureStorageContainer);
            }
            else
            {
                user.Picture = GetDefaultProfilePicture();
            }

            //Finish the registration.
            return FinishRegistration(user, true);
        }

        /// <summary>
        /// User with completed facebook registration process.
        /// </summary>
        /// <returns>The ImportFriendsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult ImportFriends(string returnUrl)
        {
            User currentUser = SecurityProvider.CurrentUser as User;

            //If this site was called by a user without facebook authentication, deny access. 
            if (currentUser.IdType != ProviderIdentifier)
            {
                throw new NotImplementedException("Cannot invite facebook friends without facebook token.");
            }

            //If the Access token is not there or expired, we have to re-authenticate the user. 
            //We also have to make sure that the user returns to this page after re-authentication. This is done by setting a appropriate return url. 
            if (AccessToken == null || AccessTokenExpires == null || AccessTokenExpires < DateTime.Now)
            {
                Redirect(Url.Action("StartAuthentication", new { returnUrl = Url.Action("ImportFriends", new { returnUrl = returnUrl }) }));
            }

            //Create a new facebook client. 
            FacebookClient client = new FacebookClient();
            client.AppId = appId;
            client.AppSecret = appSecret;
            client.AccessToken = AccessToken;

            //Gather all facebook friends of the current user. 
            dynamic friends = client.Get("/" + currentUser.AccountToken + "/friends");

            List<FacebookRegistrationData> data = new List<FacebookRegistrationData>();

            foreach (dynamic friend in friends.data)
            {
                string id = friend.id;

                //For each friend, check whether the friend is also registered with sGrid, or if the users are not already befriended...
                User friendUser = MemberManager.Users.Where(x => x.IdType == ProviderIdentifier && x.AccountToken == id).FirstOrDefault();

                if (friendUser != null && !MemberManager.AreFriends(friendUser, currentUser) && MemberManager.GetFriendRequest(currentUser, friendUser) == null)
                {
                    //... If so, add the potential friend to our list. 
                    data.Add(new FacebookRegistrationData()
                    {
                        Nickname = friend.name,
                        Id = id,
                        PictureUrl = "http://graph.facebook.com/" + friend.id + "/picture"
                    });
                }
            }

            //Adjust the return url. 
            ReturnUrl = returnUrl;

            //Show the ImportFriends view. 
            if (data.Count > 0)
            {
                return View(data);
            }
            else
            {
                return Redirect(ReturnUrl);
            }
        }

        /// <summary>
        /// Finishes the import friends action and redirects the user to ReturnUrl.
        /// </summary>
        /// <returns>A redirect.</returns>
        public RedirectResult FinishImportFriends()
        {
            return Redirect(ReturnUrl);
        }

        /// <summary>
        /// Receives the call from facebook and uses the state and code strings to 
        /// gather user data over the facebook API. If the user is a new user, shows 
        /// the FinishRegistrationView with a user object containing the gathered data 
        /// as model. Else, calls FinishAuthentication with the user context of the 
        /// now authenticated user. 
        /// </summary>
        /// <param name="state">A string indicating the authentication state.</param>
        /// <param name="code">A token which can be used to access the facebook API.</param>
        /// <param name="error">An error string.</param>
        /// <param name="redirectTo">The redirect url.</param>
        /// <returns>An ActionResult performing one of the operations described above.</returns>
        public ActionResult OAuthLanding(string state, string code, string redirectTo, string error)
        {
            //If the given state does not equal our stored state, this is CSRF. 
            if (state != State)
            {
                throw new NotImplementedException();
            }

            //If there was an error, su fail. 
            if (error != null)
            {
                throw new NotImplementedException(error);
            }

            //Create a new facebook client
            FacebookClient client = new FacebookClient();
            client.AppId = appId;
            client.AppSecret = appSecret;

            //Fetch the access token from facebook. 
            dynamic token = client.Get(FacebookAccessTokenUrl, new {
                client_id = appId,
                client_secret = appSecret, 
                code = code,
                redirect_uri = Url.Action("OAuthLanding", "FacebookIdProvider", new { redirectTo = redirectTo }, Request.Url.Scheme)
            });

            client.AccessToken = token.access_token;

            AccessToken = token.access_token;
            AccessTokenExpires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(token.expires);

            //Get basic user information.
            dynamic me = client.Get("/me");

            //Try to find an account with the given facebook id. 
            Account current = MemberManager.Accounts.Where(x => x.AccountToken == me.id && x.IdType == ProviderIdentifier).FirstOrDefault();

            if (current != null)
            {
                //If there was an account, we can authenticate the user. 
                return FinishAuthentication(new UserContext(current));
            }
            else
            {
                //If we did not find a matching account, start a registration process.  
                FacebookRegistrationData registrationData = new FacebookRegistrationData()
                {
                    Nickname = me.name,
                    Culture = me.locale,
                    Id = me.id,
                    EMailAddress = me.email,
                    PictureUrl = String.Format(FacebookPictureUrl, me.id),
                    GetPictureFormFacebook = true
                };

                return View("FinishFacebookRegistration", registrationData);
            }
        }

    }
}
