using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;
using sGridServer.Code.Security;
using Resource = sGridServer.Resources.Support.Support;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The support interface enables sGrid team members to list support messages. 
    /// An sGrid team member can also reply to a message, since the email address is stored 
    /// (registered users, coin partners and sponsor all have email address, unregistered users have to provide an email address to be able to send a message). 
    /// </summary>
    public class SupportController : Controller
    {
        /// <summary>
        /// Coin partner as a search option.
        /// </summary>
        public const string SearchOptionCoinPartners = "Coin Partners";

        /// <summary>
        /// Sponsors as a search option.
        /// </summary>
        public const string SearchOptionSponsors = "Sponsors";

        /// <summary>
        /// Admins as a search option.
        /// </summary>
        public const string SearchOptionAdmins = "Administrators";

        /// <summary>
        /// Registred users as a search option.
        /// </summary>
        public const string SearchOptionRegistredUsers = "Registred Users";

        /// <summary>
        /// Unregistred users as a search option.
        /// </summary>
        public const string SearchOptionUnregistredUsers = "Unregistred Users";

        /// <summary>
        /// Returns the detail view for the given message.
        /// </summary>
        /// <param name="id">The identifier of the message to get the details for.</param>
        /// <returns>The DetailMessageView for the given message.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult DetailMessage(int id)
        {
            MessageManager manager = new MessageManager();
            Message result = manager.GetMessages().Where(p => p.Id == id).FirstOrDefault();

            return View(result);
        }

        /// <summary>
        /// Returns the ListView containing a list of messages according to the given parameters.
        /// </summary>
        /// <param name="userType">If this parameter is set, only messages which were sended by the users of a specific type are shown. 
        /// Possible values are: Partners, Sponsors, RegistredUsers, UnregisteredUsers, All.</param>
        /// <param name="unresolvedOnly">A bool which specifies whether only unresolved messages have to be shown. 
        /// Possible values are: true, false.</param>
        /// <returns>The ListView according to the given parameters.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult List(String userType, bool unresolvedOnly)
        {
            MessageManager manager = new MessageManager();

            //gets all messages
            IEnumerable<Message> result = manager.GetMessages();

            //chooses messages according to the given user type
            if (userType == SearchOptionRegistredUsers)
            {
                result = result.Where(p => p.Account != null);
            }
            else if (userType == SearchOptionUnregistredUsers)
            {
                result = result.Where(p => p.Account == null);
            }
            else if (userType == SearchOptionSponsors)
            {
                result = result.Where(p => p.Account != null).Where(p => p.Account.UserPermission == SiteRoles.Sponsor);
            }
            else if (userType == SearchOptionCoinPartners)
            {
                result = result.Where(p => p.Account != null).Where(p => p.Account.UserPermission == SiteRoles.CoinPartner);
            }
            else if (userType == SearchOptionAdmins)
            {
                result = result.Where(p => p.Account != null).Where(p => p.Account.UserPermission == SiteRoles.Admin);
            }

            //filters a result list according to the given parameter, whether only resolved or unresolved messages are to show
            result = result.Where(p => p.Resolved != unresolvedOnly);

            //if there are messages, return them
            if (result.Count() != 0)
            {
                return PartialView(result);
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoObjects, Resource.Messages);
                return PartialView();
            }
        }

        /// <summary>
        /// Shows the MessageOverviewView.
        /// </summary>
        /// <returns>The MessageOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult MessageOverview()
        {
            return View();
        }

        /// <summary>
        /// Marks the given message as resolved.
        /// </summary>
        /// <param name="message">The message to resolve.</param>
        /// <returns>An ActionResult indicating either error or success.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ResolveMessage(Message message)
        {
            MessageManager manager = new MessageManager();
            manager.MarkMessageAsResolved(message);

            return RedirectToAction("DetailMessage", new { id = message.Id });
        }
    }
}
