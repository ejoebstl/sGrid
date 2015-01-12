using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Rewards;
using sGridServer.Code.Utilities;
using sGridServer.Properties;
using System.Drawing;
using Resource = sGridServer.Resources.Support.Support;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The partner support user interface enables sGrid team members to view and edit rewards and banners provided by coin partners or sponsors.
    /// </summary>
    public class PartnerSupportController : Controller
    {
        /// <summary>
        /// Returns the detail view for the banner of the given sponsor.
        /// </summary>
        /// <param name="sponsorId">The identifier of the sponsor who owns the banner to show the details for.</param>
        /// <returns>The BannerDetailView for the given banner.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult BannerDetail(int sponsorId)
        {
            MemberManager memberManager = new MemberManager();
            Sponsor current = memberManager.Sponsors.Where(p => p.Id == sponsorId).FirstOrDefault();
            return View(current);
        }

        /// <summary>
        /// Shows the BannerOverviewView.
        /// </summary>
        /// <returns>The BannerOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult BannerOverview()
        {
            return View();
        }

        /// <summary>
        /// Shows the EditBannerView.
        /// </summary>
        /// <param name="id">The id of sponsor who owns the banner to edit.</param>
        /// <returns>The EditBannerView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult EditBanner(int id)
        {
            MemberManager memberManager = new MemberManager();
            Sponsor sponsor = memberManager.Sponsors.Where(s => s.Id == id).FirstOrDefault();
            return View(sponsor);
        }

        /// <summary>
        /// Returns the ListBannerView according to the given parameters.
        /// </summary>
        /// <param name="unapprovedOnly">This parameter specifies whether only unapproved banners have to be shown. 
        /// Possible values are: true, false.</param>
        /// <returns>The ListBannerView according to the given parameter.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ListBanner(bool unapprovedOnly)
        {
            MemberManager memberManager = new MemberManager();
            IEnumerable<Sponsor> result = memberManager.Sponsors.Where(p => p.Approved != unapprovedOnly);

            //if there are banners according to the given parameter they has to be shown,
            //otherwise it shows a message that there are no banners according to the given parameter
            if (result.Count() != 0)
            {
                return PartialView(result);
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoObjects, Resource.Banners);
                return PartialView();
            }
        }

        /// <summary>
        /// Returns the ListRewardsView according to the given parameters.
        /// </summary>
        /// <param name="unapprovedOnly">This parameter specifies whether only unapproved rewards have to be shown.
        /// Possible values are: true, false.</param>
        /// <returns>The ListRewardsView according to the given parameters.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ListRewards(bool unapprovedOnly)
        {
            RewardManager rewardManager = new RewardManager();
            IEnumerable<Reward> result = rewardManager.GetAllExistingRewards().Where(p => p.Approved != unapprovedOnly);

            //if there are rewards according to the given parameter they have to be shown,
            //otherwise it shows a message that there are no rewards according to the given parameter
            if (result.Count() != 0)
            {
                return PartialView(result);
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoObjects, Resource.Rewards);
                return PartialView();
            }
        }

        /// <summary>
        /// Returns the detail view for the given reward.
        /// </summary>
        /// <param name="id">The identifier of the reward to get the details for.</param>
        /// <returns>The DetailRewardView for the given reward.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult RewardDetail(int id)
        {
            return RedirectToAction("DetailReward", "RewardConfiguration", new { id = id });
        }

        /// <summary>
        /// Shows the RewardsOverviewView.
        /// </summary>
        /// <returns>The RewardsOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult RewardsOverview()
        {
            return View();
        }

        /// <summary>
        /// Uploaded a banner of the given sponsor
        /// </summary>
        /// <param name="sponsor">The sponsor whom banner has to be uploaded.</param>
        /// <param name="file">The file which will replace the old banner.</param>
        /// <returns>The BannerDetailView of the given sponsor's banner.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult UploadBanner(Sponsor sponsor, HttpPostedFileBase file)
        {
            MemberManager memberManager = new MemberManager();
            Sponsor currentSponsor = memberManager.GetAccountById(sponsor.Id) as Sponsor;

            //checks if any file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                BlobStorage storage = new BlobStorage(Settings.Default.PartnerStorageContainer);

                //if the sponsor already has a banner it has to be deleted
                if (currentSponsor.Banner != "")
                {
                    storage.RemoveBlob(currentSponsor.Banner);
                }

                //reads, resizes and saves an uploaded image in the blob storage
                System.IO.Stream newBanner = ImageUtil.ResizeImage(file.InputStream, Settings.Default.BannerWidth, Settings.Default.BannerHeight, Color.White);
                currentSponsor.Banner = storage.StoreBlob(newBanner);
            }

            //sets a show frequency of banner and save changes in account of the sponsor whom banner was uploaded
            currentSponsor.ShowFrequency = sponsor.ShowFrequency;
            memberManager.SaveAccount(currentSponsor);

            return RedirectToAction("BannerDetail", new { sponsorId = currentSponsor.Id });
        }

        /// <summary>
        /// Approves a banner of the given sonsor.
        /// </summary>
        /// <param name="sponsor">The sponsor whom banner is to approve.</param>
        /// <returns>The BannerDetailView of the given sponsor's banner</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ApproveBanner(Sponsor sponsor)
        {
            MemberManager manager = new MemberManager();
            Sponsor current = manager.GetAccountById(sponsor.Id) as Sponsor;

            //approves banner and saves changes
            current.Approved = true;
            manager.SaveAccount(current);

            return RedirectToAction("BannerDetail", new { sponsorId = sponsor.Id });
        }

        /// <summary>
        /// Approves a reward.
        /// </summary>
        /// <param name="id">The id of the reward to approve.</param>
        /// <returns>The DetailRewardView of the given reward.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult ApproveReward(int id)
        {
            RewardManager manager = new RewardManager();
            Reward reward = manager.GetRewardById(id);

            //approves the given reward
            manager.ApproveReward(reward);
            return RedirectToAction("RewardDetail", new { id = id });
        }
    }
}

