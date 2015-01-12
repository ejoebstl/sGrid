using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using System.Net.Mail;
using Resource = sGridServer.Resources.Friends.Friends;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The friends user interface enables users to invite or remove friends, answer friend requests and view their friends.
    /// </summary>
    public class FriendsController : Controller
    {
        /// <summary>
        /// Accepts the given user as a friend of a current user. 
        /// </summary>
        /// <param name="user">The user associated with the friend request.</param>
        /// <returns>Redirect to the Requests action.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult AcceptRequest(int user)
        {
            MemberManager manager = new MemberManager();
            User current = SecurityProvider.CurrentUser as User;
            User userToAccept = manager.GetAccountById(user) as User;

            //add friendship between users above
            manager.RegisterFriendship(current, userToAccept);
            return RedirectToAction("Requests");
        }

        /// <summary>
        /// Declines the friendship request of the given user. 
        /// </summary>
        /// <param name="user">The user associated with the friend request.</param>
        /// <returns>Redirect to the Requests action.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult DeclineRequest(int user)
        {
            MemberManager manager = new MemberManager();
            User current = SecurityProvider.CurrentUser as User;
            User userToDecline = manager.GetAccountById(user) as User;

            //reject friendship request from the given user and delete a request
            manager.RejectFriendRequest(userToDecline, current);
            return RedirectToAction("Requests");
        }

        /// <summary>
        /// Shows the FriendsOverviewView
        /// </summary>
        /// <param name="id">The id of the user associated with the friends overview view (optional).</param>
        /// <returns>The FriendsOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult FriendsOverview(int? id)
        {

            MemberManager manager = new MemberManager();
            User current;

            //returns the friends overview of the current user or from the user currently viewed by admin
            if (id == null && SecurityProvider.Context.UserPermissions == SiteRoles.User)
            {
                current = SecurityProvider.CurrentUser as User;
                return View(current);
            }
            else if (id != null && SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
            {
                current = manager.GetAccountById(id.Value) as User;
                return View(current);
            }
            else
            {
                return View();
            }
        }

        /// <summary>
        /// Shows the InviteView containing a form to invite a person to become a member.
        /// </summary>
        /// <returns>The InviteView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult Invite()
        {
            return PartialView();
        }

        
        /// <summary>
        /// Sends a generated invite message to the provided email address, returns user to the friends overview page
        /// </summary>
        /// <param name="eMailAddress">The email address associated with the invitation.</param>
        /// <returns>An ActionResult, redirecting the user to the friends overview page in case of success.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult Invite(String eMailAddress)
        {
            //checks if the given String is an e-mail address
            try
            {
                MailAddress a = new MailAddress(eMailAddress);
            }
            catch
            {
                ViewBag.Message = Resource.WrongEmailToInvite;
                return View(eMailAddress);
            }

            MemberManager Manager = new MemberManager();
            User Current = SecurityProvider.CurrentUser as User;

            //invitation url
            String url = Manager.GetInvitationUrl(Current, this.ControllerContext);
            //subject of the letter
            String subject = Resource.InviteEmailSubject;
            //body of the letter
            String body = String.Format(Resource.InviteEmailBody, Current.Nickname, url);

            //packs the Strings above to the one mail message
            MailMessage message = new MailMessage(sGridServer.Properties.Settings.Default.NotificationMailFromAddress, eMailAddress, subject, body);
            SmtpClient mailer = new SmtpClient("localhost");

            //send an invitation and shows friends
            mailer.Send(message);
            return RedirectToAction("FriendsOverview");
        }

        /// <summary>
        /// Gets the partial FriendsView containing a search box and list of friends.
        /// </summary>
        /// <param name="id">The id of the currently viewed user.</param>
        /// <returns>The partial FriendsView</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Friends(int? id)
        {
            MemberManager manager = new MemberManager();
            User current;

            //returns the friends list of the current user or from admin given user
            if (SecurityProvider.Context.UserPermissions == SiteRoles.User)
            {
                current = SecurityProvider.CurrentUser as User;
                return PartialView(current);
            }
            else if (SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
            {
                current = manager.GetAccountById(id.Value) as User;
                return PartialView(current);
            }
            else
            {
                return PartialView();
            }
        }

        /// <summary>
        /// Returns the partial ListView containing a list of friends according to the given parameter.
        /// </summary>
        /// <param name="searchName">If this parameter is set, only friends which names match the given string are returned.</param>
        /// <param name="id">The id of the user whom friends are to shown.</param>
        /// <returns>The partial ListView according to the given parameter.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult List(String searchName, int? id)
        {
            MemberManager manager = new MemberManager();
            User current;

            if (id != null && SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
            {
                current = manager.GetAccountById(id.Value) as User;
            }
            else
            {
                current = SecurityProvider.CurrentUser as User;
            }

            //gets all friends of the user above
            IEnumerable<User> result = manager.GetFriends(current);

            //if user have friends they are to return, else he gets a message shown 
            if (result.Count() != 0)
            {
                //if a search name was given it finds friends, which contain a search combination in the nickname
                if (searchName != null && searchName != "")
                {
                    result = result.Where(p => p.Nickname.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
                }

                return PartialView(result);
            }
            else
            {
                ViewBag.Message = Resource.NoFriends;
                return PartialView();
            }
        }

        /// <summary>
        /// Shows a message that has to be confirmed to remove the friendship between the given and the current user. 
        /// If the user does not confirm the message, the partial view will be closed.
        /// </summary>
        /// <returns>The partial RemoveFriendView. </returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult RemoveFriend()
        {
            return PartialView();
        }

        /// <summary>
        /// Removes the friendship relationship between the given and the current user.
        /// </summary>
        /// <param name="userId">The id of the user whose friendship with the current user has to be removed.</param>
        /// <returns>Redirect to the FriendsOverview action.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult RemoveFriendAction(int userId)
        {
            MemberManager manager = new MemberManager();
            User current = SecurityProvider.CurrentUser as User;
            User toRemove = manager.GetAccountById(userId) as User;

            //remove a friendship relation between users above
            manager.RemoveFriendship(current, toRemove);
            return RedirectToAction("FriendsOverview");
        }

        /// <summary>
        /// Sends a friendship request from a current user to the given user.
        /// </summary>
        /// <param name="userId">The id of the user associated with the friendship request.</param>
        /// <returns>Redirect to the ProfileDetail view of the user to add.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult RequestFriendship(int userId)
        {
            MemberManager manager = new MemberManager();
            User current = SecurityProvider.CurrentUser as User;
            User toAdd = manager.GetAccountById(userId) as User;

            if (manager.GetFriendRequest(toAdd, current) != null)
            {
                AcceptRequest(userId);
            }
            else
            {
                //sends a friendship request from the current user to the user toAdd
                manager.AddFriendRequest(current, toAdd);
            }

            //returns a ProfileDetailView of the user toAdd
            return RedirectToAction("ProfileDetail", "Profile", new { id = userId });
        }

        /// <summary>
        /// Shows the RequestsView showing all pending friend requests.
        /// </summary>
        /// <returns>The partial RequestsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult Requests()
        {
            MemberManager manager = new MemberManager();
            User current = SecurityProvider.CurrentUser as User;

            //gets a list of users which sent a friendship request to the current user
            IEnumerable<User> result = manager.GetFriendRequests(current).Where(u => !u.Rejected).Select(u => u.Requester);

            //if there are friendship request, show them 
            if (result.Count() != 0)
            {
                return PartialView(result);
            }
            else
            {
                ViewBag.Message = Resource.NoRequests;
                return PartialView();
            }
        }

    }
}
