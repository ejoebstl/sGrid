using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Rewards;
using sGridServer.Code.GridProviders;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the admin dashboard views.
    /// New messages, unapproved banners and rewards and performance overviews are shown.
    /// </summary>
    public class AdminDashboardController : Controller
    {
        /// <summary>
        /// Shows the AdminDashboardView.
        /// </summary>
        /// <returns>The Admin DashboardView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult AdminDashboard()
        {
            return View();
        }
        /// <summary>
        /// Returns the partial NewMessagesView showing the newest pending messages to the sGrid team.
        /// </summary>
        /// <returns>A partial NewMessagesView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult NewMessages()
        {
            MessageManager manager = new MessageManager();
            IEnumerable<Message> allMessages = manager.GetMessages();
            List<Message> shownMessages = new List<Message>();
            //Add unresolved messages to the list.
            foreach (Message current in allMessages)
            {
                if (!current.Resolved)
                {
                    shownMessages.Add(current);
                }
            }
            return PartialView(shownMessages);
        }
        /// <summary>
        /// Returns the partial NewMessagesView showing the newest pending messages from sponsors and coin partners to the sGrid team.
        /// </summary>
        /// <returns>A partial NewMessagesView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult NewPartnerMessages()
        {
            MessageManager manager = new MessageManager();
            IEnumerable<Message> allMessages = manager.GetMessages();
            List<Message> shownMessages = new List<Message>();
            //Add unresolved messages from coin partners and sponsors to the list.
            foreach (Message current in allMessages)
            {
                if (!current.Resolved && (current.Account is Partner))
                {
                    shownMessages.Add(current);
                }
            }
            return PartialView(shownMessages);
        }
        /// <summary>
        /// Returns a partial view, which shows a performance overview of the entire grid.
        /// </summary>
        /// <returns>The partial PerformanceOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult PerformanceOverview()
        {
            GridPerformanceData summary = GridProviderManager.GlobalSummary;
            return PartialView(summary);
        }
        /// <summary>
        /// Returns the partial UnapprovedBannersView showing the newest banners which are pending validation.
        /// </summary>
        /// <returns>The partial UnapprovedBannersView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult UnapprovedBanners()
        {
            MemberManager manager = new MemberManager();
            IEnumerable<Sponsor> sponsors = manager.Sponsors;
            List<Sponsor> banners = new List<Sponsor>();
            //Add sponsor to the list, if his or her banner is unapproved.
            foreach (Sponsor sponsor in sponsors)
            {
                if (!sponsor.Approved)
                {
                    banners.Add(sponsor);
                }
            }
            return PartialView(banners);
        }
        /// <summary>
        /// Returns the partial UnapprovedRewardsView showing the newest rewards pending validation from the coin partners.
        /// </summary>
        /// <returns>The partial UnapprovedRewardsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult UnapprovedRewards()
        {
            RewardManager manager = new RewardManager();
            IEnumerable<Reward> allRewards = manager.GetAllExistingRewards();
            List<Reward> shownRewards = new List<Reward>();
            //Add unapproved rewards to the list.
            foreach (Reward item in allRewards)
            {
                if (!item.Approved)
                {
                    shownRewards.Add(item);
                }
            }
            return PartialView(shownRewards);
        }
    }
}
