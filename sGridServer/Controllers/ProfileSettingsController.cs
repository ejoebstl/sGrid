using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.GridProviders;
using sGridServer.Models;
using Resource = sGridServer.Resources.ProfileSettings.ProfileSettings;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the user profile settings.
    /// It also provides functions to save user settings and to end a membership of the current user.
    /// </summary>
    public class ProfileSettingsController : Controller
    {   
        /// <summary>
        /// Performs the end membership process.
        /// </summary>
        /// <returns>Redirect to the Index page of the Website.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EndMembership(bool? confirm)
        {
            if (confirm == null)
            {
                return PartialView();
            }
            if (confirm == true)
            {
                //Deactivate the account of the current user
                MemberManager manager = new MemberManager();
                Account current = SecurityProvider.CurrentUser;
                SecurityProvider.LogOut();
                manager.DeactivateAccount(current);
            }
            return RedirectToAction("Index", "Public");
        }
        /// <summary>
        /// Shows the general user settings which can be changed by the user.
        /// </summary>
        /// <returns>A partial GeneralView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult General()
        {
            Account user = SecurityProvider.CurrentUser;
            //Password field is only shown to the users which rgistered with their EMail address.
            bool showPasswordField = (user.IdType == sGridServer.Controllers.EMailIdProviderController.ProviderIdentifier);
            GeneralSettingsData data = new GeneralSettingsData() { Nickname = SecurityProvider.CurrentUser.Nickname, ShowAdditional = showPasswordField };
            
            return PartialView(data);
        }
        /// <summary>
        /// Saves user changes on the general settings.
        /// </summary>
        /// <param name="data">The submitted data.</param>
        /// <returns>Content (Message) to show to the user.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult General(GeneralSettingsData data)
        {
            if (ModelState.IsValid)
            {
                Account current = SecurityProvider.CurrentUser;
                MemberManager manager = new MemberManager();
                //Check if nickname already taken.
                if (!manager.Users.Where(x => x.Nickname.ToLower() == data.Nickname.ToLower()).Any())
                {
                    current.Nickname = data.Nickname;
                    manager.SaveAccount(current);
                }
                else if (current.Nickname != data.Nickname)
                {
                    return Content("Nickname already taken!", "text/html");
                }
                //Check if password was resetted.
                if (data.PasswordOld != null && data.PasswordOld != "" && data.PasswordFirst != "")
                {
                    return Content(SaveChanges(current, data.PasswordOld, data.PasswordFirst, data.PasswordSecond));
                }
                //Return nothing, if all worked fine.
                return Content("", "text/html");
            }
            else
            {
                if (data.Nickname == null | data.Nickname == "")
                {
                    return Content("Nickname invalid", "text/html");
                }
                else
                {
                    return Content("Password length invalid (minimal length = 6)", "text/html");
                }
            }
        }
        /// <summary>
        /// Shows the notification settings which can be changed by the user.
        /// </summary>
        /// <returns>A partial NotificationsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Notifications()
        {
            return PartialView(SecurityProvider.CurrentUser);
        }
        /// <summary>
        /// Saves user changes on the notification settings.
        /// </summary>
        /// <param name="notifyOnAchievementReached">If set, the user has to be notified on reaching an achievement.</param>
        /// <param name="notifyOnCoinBalanceChanged">If set, the user has to be notified on change of coin balance.</param>
        /// <param name="notifyOnProjectChanged">If set, the user has to be notified on changing a project.</param>
        /// <returns>Redirect to the "Notifications" action</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Notifications(bool notifyOnAchievementReached, bool notifyOnCoinBalanceChanged, bool notifyOnProjectChanged)
        {
            User current = SecurityProvider.CurrentUser as User;
            current.NotifyOnAchievementReached = notifyOnAchievementReached;
            current.NotifyOnCoinBalanceChanged = notifyOnCoinBalanceChanged;
            current.NotifyOnProjectChanged = notifyOnProjectChanged;
            SaveChanges(current);
            return RedirectToAction("Notifications");
        }
        /// <summary>
        /// Shows the privacy settings which can be changed by the user.
        /// </summary>
        /// <returns>A partial PrivacyView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Privacy()
        {
            return PartialView(SecurityProvider.CurrentUser);
        }
        /// <summary>
        /// Saves user changes on the privacy settings.
        /// </summary>
        /// <param name="showInHighScore">If set, the user profile can be shown in highscore tables.</param>
        /// <param name="privacyLevel">Sets the privacy level of the profile.</param>
        /// <returns>Redirect to the "Privacy" action</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Privacy(bool showInHighScore, int privacyLevel)
        {
            User current = SecurityProvider.CurrentUser as User;
            current.ShowInHighScore = showInHighScore;
            if (privacyLevel > 0)
            {
                current.Privacy = (PrivacyLevel)privacyLevel;
            }
            SaveChanges(current);
            return RedirectToAction("Privacy");
        }
        /// <summary>
        /// Shows the SettingsView.
        /// </summary>
        /// <returns>The SettingsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Settings()
        {
            return View();
        }
        /// <summary>
        /// Saves user changes. 
        /// </summary>
        /// <param name="user">Edited user</param>
        private void SaveChanges(User user)
        {
            if (SecurityProvider.CurrentUser.Id == user.Id)
            {
                MemberManager manager = new MemberManager();
                manager.SaveAccount(user);
            }
        }
        /// <summary>
        /// Saves user changes if a password was changed. 
        /// </summary>
        /// <param name="user">Edited user</param>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPasswordFirst">New password</param>
        /// <param name="newPasswordSecond">New password (repeated)</param>
        private String SaveChanges(Account user, String oldPassword, String newPasswordFirst, String newPasswordSecond)
        {
            //Message to return to the user.
            String result = "";
            if (SecurityProvider.CurrentUser.Id == user.Id)
            {
                MemberManager manager = new MemberManager();
                if (manager.ValidatePassword(SecurityProvider.CurrentUser, oldPassword))
                {
                    if (newPasswordFirst == newPasswordSecond)
                    {
                        result = "";
                        manager.SetPassword(user, newPasswordFirst);
                    }
                    else
                    {
                        result = Resource.PasswordsNotIdentical;
                    }
                }
                else
                {
                    result = Resource.OldPasswordWrong;
                }             
            }
            return result;
        }
    }
}
