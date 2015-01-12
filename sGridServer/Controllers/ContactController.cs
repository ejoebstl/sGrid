using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The contact user interface enables registered users, coin partners and sponsors to contact the sGrid team in case of questions.
    /// </summary>
    public class ContactController : Controller
    {
        /// <summary>
        /// Shows the empty ContactView.
        /// </summary>
        /// <returns>The empty ContactView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Shows a contact form with the message and subject autofilled.
        /// </summary>
        /// <param name="message">The message to fill a message box.</param>
        /// <param name="subject">The subject to fill a subject box.</param>
        /// <returns>The contact formular autofilled with the given message and subject.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult ShowContactForm(string message, string subject)
        {
            ContactData data = new ContactData() { Message = message, Subject = subject };
            return View("Contact", data);
        }

        /// <summary>
        /// Returns the ContactView (with an error message in case of an error and a success message in case of success of sending the message) 
        /// and sends the registered user’s, coin partner’s or sponsor’s message to the sGrid team.
        /// </summary>
        /// <param name="messageClass">The message to send to the sGrid team.</param>
        /// <returns>The ContactView (with an error message in case of an error and a success message in case of success of sending the message).</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Contact(ContactData messageClass)
        {
            if (!ModelState.IsValid)
            {
                return View(messageClass);
            }

            //get new message from the model
            Message contact = new Message();
            contact.MessageText = messageClass.Message;
            contact.Subject = messageClass.Subject;
            contact.AccountId = SecurityProvider.CurrentUser.Id;
            contact.EMail = SecurityProvider.CurrentUser.EMail;

            //save a message
            MessageManager manager = new MessageManager();
            manager.AddMessage(contact);

            return RedirectToAction("ContactConfirm");
        }

        /// <summary>
        /// Shows the view with a success message of sending the message to sGrid team.
        /// </summary>
        /// <returns>The ContactConfirmView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult ContactConfirm()
        {
            return View();
        }
    }
}
