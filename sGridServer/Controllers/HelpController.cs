using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Models;
using System.IO;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;
using System.Globalization;
using sGridServer.Code.Security;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the help views and sending unregistered users’ messages to the sGrid team.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// Shows the empty ContactView.
        /// </summary>
        /// <returns>The empty ContactView.</returns>
        public ActionResult Contact()
        {
            if (SecurityProvider.Context == null)
            {
                ContactInfo info = new ContactInfo();

                //generate a new captcha
                CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, "HelpContact");
                info.Captcha = captchaGenerator.CreateCaptcha();
                return View(info);
            }
            else
            {
                return RedirectToAction("Contact", "Contact");
            }
        }


        /// <summary>
        /// Returns the ContactView (with an error message in case of an error and a success message in case of success of sending the message) 
        /// and sends the user’s message to the sGrid team.
        /// </summary>
        /// <param name="contactInfo">The message to send to the sGrid Team.</param>
        /// <returns>The ContactView (with an error message in case of an error and a success message in case of success of sending the message).</returns>
        [HttpPost]
        public ActionResult Contact(ContactInfo contactInfo)
        {
            CaptchaGenerator captchaGenerator = new CaptchaGenerator(this.ControllerContext, "HelpContact");

            if (!ModelState.IsValid)
            {
                return View(contactInfo);
            }

            //if the captcha was wrong, reshows a contact form
            if(!(captchaGenerator.ValidateCaptcha(contactInfo.Captcha)))
            {
                ModelState.Clear();
                contactInfo.Captcha = captchaGenerator.CreateCaptcha();
                ViewBag.Message = Resources.Help.Contact.CaptchaIsNotCorrectText;
                return View(contactInfo);
            }

            //get a new message from the contact form
            Message contact = new Message();
            contact.MessageText = contactInfo.Message;
            contact.Subject = contactInfo.Subject;
            contact.EMail = contactInfo.From;

            //save the given message
            MessageManager manager = new MessageManager();
            manager.AddMessage(contact);

            return RedirectToAction("HelpContactConfirm");
        }

        /// <summary>
        /// Shows the view with a success message of sending the message to sGrid team.
        /// </summary>
        /// <returns>The HelpContactConfirmView.</returns>
        public ActionResult HelpContactConfirm()
        {
            return View();
        }
        /// <summary>
        /// Shows the impressum view.
        /// </summary>
        /// <returns>The ImpressumView.</returns>
        public ActionResult Impressum()
        {
            return View();
        }
        /// <summary>
        /// Shows the FAQView, which is responsible for showing the FAQ section for the unregistered users.
        /// </summary>
        /// <returns>The FAQView.</returns>
        public ActionResult FAQ()
        {
            return View();
        }

        /// <summary>
        /// Shows the HelpView, which is responsible for showing the help section for the unregistered users.
        /// </summary>
        /// <returns>The HelpView.</returns>
        public ActionResult Help()
        {
            return View();
        }
    }
}
