using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using System.IO;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using sGridServer.Properties;
using System.Drawing;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the partnership views.
    /// It also provides functions to upload a logo, edit information and logo of the coin partner.
    /// </summary>
    public class PartnershipController : Controller
    {
        /// <summary>
        /// Redirects to the EditImage with the logo as a type.
        /// </summary>
        /// <param name="id">The id of the partner whose logo has to be changed.</param>
        /// <returns>Redirects to the EditImage method to change partner’s logo.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditLogo(int? id)
        {
            return RedirectToAction("EditImage", new { type = "logo", id = id });

        }
        /// <summary>
        /// Saves changes on the partner’s profile information.
        /// </summary>
        /// <param name="nickname">The edited nickname.</param>
        /// <param name="link">The edited link.</param>
        /// <param name="description">The edited description.</param>
        /// <returns>Redirect to the partner dashboard.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditPartnershipInformation(MultiLanguageString description, String nickname, String link, String securityQuestion, String securityAnswer)
        {
            Partner current = SecurityProvider.CurrentUser as Partner;
            //Stores changes on the following properties: nickname, description, link.
            if (description != null && description != "")
            {
                current.Description = description;
            }
            if (nickname != null && nickname != "")
            {
                current.Nickname = nickname;
            }
            if (link != null && link != "")
            {
                current.Link = link;
            }
            if (current is CoinPartner && securityQuestion != null && securityQuestion != "" && securityAnswer != null && securityAnswer != "")
            {
                (current as CoinPartner).SecurityQuestion = securityQuestion;
                (current as CoinPartner).SecurityAnswer = securityAnswer;
            }
            MemberManager manager = new MemberManager();
            manager.SavePartner(current);
            if (current is Sponsor)
            {
                return RedirectToAction("SponsorDashboard");
            }
            else
            {
                return RedirectToAction("PartnershipDashboard");
            }
        }
        /// <summary>
        /// Shows EditPartnershipInformation view.
        /// </summary>
        /// <param name="id">The id of the partner whose information has to be edited.</param>
        /// <returns>The EditPartnershipInformation view.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin | SiteRoles.Sponsor)]
        public ActionResult PartnershipInformationEditView(int? id)
        {
            //Shows edit information view for the current partner.
            //If current user is an admin, shows the editpartnershipinformation view of the partner with a given id.
            Account account = SecurityProvider.CurrentUser;
            Partner partner = null;
            if (account != null && account is Partner)
            {
                partner = account as Partner;
            }
            else if (account != null && account is SGridTeamMember && id != null)
            {
                Account partnerAccount = (new MemberManager()).GetAccountById(id.Value);
                partner = (partnerAccount != null) ? (partnerAccount as Partner) : null;
            }
            return PartialView("EditPartnershipInformation", partner);
        }
        /// <summary>
        /// Shows the PartnershipDashboardView.
        /// </summary>
        /// <param name="id">The id of the currently viewed partner.</param>
        /// <returns>The PartnershipDashboardView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult PartnershipDashboard(int? id)
        {
            //Shows dashboard for the current partner. If current user is an admin, shows the dashboard of the partner with a given id. 
            Account account = SecurityProvider.CurrentUser;
            CoinPartner partner = null;
            if (account != null && account is CoinPartner)
            {
                partner = account as CoinPartner;
            }
            else if (account != null && account is SGridTeamMember && id != null)
            {
                Account partnerAccount = (new MemberManager()).GetAccountById(id.Value);
                partner = (partnerAccount != null) ? (partnerAccount as CoinPartner) : null;
            }
            return View(partner);
        }
        /// <summary>
        /// Shows the PartnershipFAQView.
        /// </summary>
        /// <returns>The PartnershipFAQView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult PartnershipFAQ()
        {
            return View();
        }
        /// <summary>
        /// Shows the PartnershipHelpView.
        /// </summary>
        /// <returns>The PartnershipHelpView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult PartnershipHelp()
        {
            return View();
        }
        /// <summary>
        /// Redirects to the EditImageView to change the banner of a sponsor.
        /// </summary>
        /// <returns>ActionResult, redirecting the user to the EditImage action method, using “banner” as type.</returns>
        [HttpPost]
        public ActionResult EditBanner(int? id)
        {
            return RedirectToAction("EditImage", new { type = "banner", id = id });
        }
        /// <summary>
        /// Shows the EditImageView for a specific image type.
        /// </summary>
        /// <param name="type">The type of the image to be edited. Possible values are: banner or logo.</param>
        /// <param name="id">The id of the partner whose image has to be edited.</param>
        /// <returns>EditImageView to edit a specific image type (banner or logo).</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin)]
        public ActionResult EditImage(String type, int? id)
        {
            if (id == null)
            {
                id = SecurityProvider.Context.ID;
            }
            Partner partner = (new MemberManager()).GetAccountById(id.Value) as Partner;
            
            return PartialView(new Tuple<String, Partner>(type, partner));
        }
        /// <summary>
        /// Displays the dashboard of a sponsor.
        /// </summary>
        /// <param name="id">The id of the currently viewed sponsor.</param>
        /// <returns>The SponsorDashboardView.</returns>
        [SGridAuthorize(RequiredPermissions = (SiteRoles.Sponsor | SiteRoles.Admin))]
        public ActionResult SponsorDashboard(int? id)
        {
            //Shows dashboard for the current sponsor. If current user is an admin, shows the dashboard of the sponsor with a given id. 
            Account account = SecurityProvider.CurrentUser;
            Sponsor sponsor = null;
            if (account != null && account is Sponsor)
            {
                sponsor = account as Sponsor;
            }
            else if (account != null && account is SGridTeamMember && id != null)
            {
                Account partnerAccount = (new MemberManager()).GetAccountById(id.Value);
                sponsor = (partnerAccount != null) ? (partnerAccount as Sponsor) : null;
            }
            return View(SecurityProvider.CurrentUser as Sponsor); 
        }
        /// <summary>
        /// Shows the SponsorHelpView.
        /// </summary>
        /// <returns>The SponsorHelpView.</returns>
        [SGridAuthorize(RequiredPermissions = (SiteRoles.Sponsor | SiteRoles.Admin))]
        public ActionResult SponsorHelp()
        {
            return View();
        }
        /// <summary>
        /// Uploads a new image, if the upload is successful, the old image (banner or logo) is replaced with the new one.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type">The type of the image (banner or logo).</param>
        /// <param name="id">The id of the partner (can be a coin partner or a sponsor).</param>
        /// <returns>Sponsor or coin partner dashboard, depending on the user type.</returns>
        [SGridAuthorize(RequiredPermissions = (SiteRoles.CoinPartner | SiteRoles.Sponsor | SiteRoles.Admin))]
        public ActionResult UploadImage(HttpPostedFileBase file, String type, int id)
        {
            String partnerType = "sponsor";
            MemberManager manager = new MemberManager();
            Partner partner = manager.GetAccountById(id) as Partner;
            if (partner as CoinPartner != null)
            {
                partnerType = "partner";
            }
            if (file != null && file.ContentLength > 0)
            {
                BlobStorage storage = new BlobStorage("PartnerContainer");                
                
                if (partner != null)
                {
                    //Banner upload
                    if (type == "banner")
                    {
                        Sponsor sponsor = partner as Sponsor;
                        //Remove current banner, if such exist.
                        if (sponsor.Banner != "")
                        {
                            storage.RemoveBlob(sponsor.Banner);
                        }
                        Stream picture = ImageUtil.ResizeImage(file.InputStream, Settings.Default.BannerWidth, Settings.Default.BannerHeight, Color.White);
                        sponsor.Banner = storage.StoreBlob(picture);
                    }
                    //Logo upload
                    else
                    {
                        
                        //Remove current logo, if such exist.
                        if (partner.Logo != "")
                        {
                            storage.RemoveBlob(partner.Logo);
                        }
                        Stream picture = ImageUtil.ResizeImage(file.InputStream, Settings.Default.ProfilePictureWidth, Settings.Default.ProfilePictureHeight, Color.White);
                        partner.Logo = storage.StoreBlob(picture);
                    }

                    manager.SaveAccount(partner);
                }
            }
            if (partnerType == "sponsor") 
            {
                return View("SponsorDashboard", partner as Sponsor);
            }
            else
            {
                return View("PartnershipDashboard", partner as CoinPartner );
            }
        }
    }
}
