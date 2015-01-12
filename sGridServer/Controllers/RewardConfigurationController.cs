using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Rewards;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.RewardConfiguration.RewardConfiguration;
using sGridServer.Models;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the reward configuration views.
    /// It also provides a function to initiate the creation of a reward.
    /// </summary>
    public class RewardConfigurationController : Controller
    {
        /// <summary>
        /// Shows the overview of the coin partner rewards.
        /// </summary>
        /// <returns>The RewardsOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult RewardsOverview()
        {
            RewardManager manager = new RewardManager();

            //gets all rewards
            IEnumerable<Reward> rewards = manager.GetAllExistingRewards().Where(r => r.CoinPartnerId == SecurityProvider.CurrentUser.Id);

            if (rewards.Count() != 0)
            {
                return View(rewards);
            }
            else
            {
                ViewBag.Message = Resource.NoRewards;
                return View();
            }
        }

        /// <summary>
        /// Shows the detail view for the given reward.
        /// </summary>
        /// <param name="id">The integer identifier of the reward.</param>
        /// <returns>The DetailRewardView for the given reward.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult DetailReward(int id)
        {
            RewardManager manager = new RewardManager();

            //finds reward and converts it to the RewardIten object 
            Reward currentReward = manager.GetRewardById(id);

            return View(currentReward);
        }

        /// <summary>
        /// Shows the CreateView for creating rewards.
        /// </summary>
        /// <returns>The CreateView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        [HttpGet]
        public ActionResult Create()
        {
            RewardManager manager = new RewardManager();
            List<SelectListItem> rewardTypes = new List<SelectListItem>();

            //gets all existing reward types
            IEnumerable<String> types = manager.GetAllRewardTypes();

            //add types to the list
            foreach (String s in types)
            {
                rewardTypes.Add(new SelectListItem() { Text = s, Value = s });
            }

            return View("Create", rewardTypes);
        }

        /// <summary>
        /// Gets the url for the corresponding reward editor from the RewardManager class and redirects the user to the corresponding controller. 
        /// </summary>
        /// <param name="type">The type of the reward to be created.</param>
        /// <returns>An ActionResult redirecting the user to the corresponding controller. </returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult Create(String type)
        {
            if ((type == null) || (type == ""))
            {
                return RedirectToAction("Create");
            }
            RewardManager manager = new RewardManager();

            if (SecurityProvider.Context.UserPermissions == SiteRoles.CoinPartner)
            {
                //shows the RewardsOverviewView for a coin partner with his rewards after creating a new reward
                return Redirect(manager.GetCreateUrl(this.Url.Action("RewardsOverview"), type, this.ControllerContext));
            }
            else
            {
                //shows the RewardsOverviewView for an admin with all existing rewards after creating a new reward for a coin partner
                return Redirect(manager.GetCreateUrl(Url.Action("RewardsOverview", "PartnerSupport"), type, this.ControllerContext));
            }
        }

        /// <summary>
        /// Gets the url for the corresponding reward editor from the RewardManager class and redirects the user to the corresponding controller. 
        /// </summary>
        /// <param name="id">The id of the reward to edit. </param>
        /// <returns>An ActionResult redirecting the user to the corresponding controller. </returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Admin)]
        public ActionResult Edit(int id)
        {
            RewardManager manager = new RewardManager();
            Reward reward= manager.GetRewardById(id);
            return Redirect(manager.GetEditUrl(this.Url.Action("DetailReward", new { id = id }), reward, this.ControllerContext));
        }

        public ActionResult RewardStatistics(int id, DateTime fromDate, DateTime to, Statistics diagramSettings)
        {
            RewardManager manager = new RewardManager();
            StatisticsDatesHelper.SetTimespanForRewardStatistics(id, ref fromDate, ref to);

            IEnumerable<Tuple<DateTime, int>> purchases = from c in manager.GetPurchases()
                                                          where c.RewardId == id && c.Timestamp >= fromDate && c.Timestamp <= to
                                                          group c by new { c.Timestamp.DayOfYear, c.Timestamp.Year } into groups
                                                          orderby groups.First().Timestamp descending
                                                          select new Tuple<DateTime, int>(
                                                            groups.First().Timestamp,
                                                            groups.Sum(g => g.Reward.Cost * g.Amount));


            List<Tuple<DateTime, int>> dates = new List<Tuple<DateTime, int>>();
            int i = 0;
            for (DateTime start = fromDate; start <= to; start = start.AddDays(1))
            {
                dates.Add(new Tuple<DateTime, int>(start, ++i));
            }

            IEnumerable<Tuple<DateTime, int>> result = from a in dates
                                                       join c in purchases on new { a.Item1.Year, a.Item1.DayOfYear } equals new { c.Item1.Year, c.Item1.DayOfYear } into q
                                                       from suba in q.DefaultIfEmpty()
                                                       select new Tuple<DateTime, int>(new DateTime(a.Item1.Year, a.Item1.Month, a.Item1.Day), suba != null ? suba.Item2 : 0);

            RewardStatisticsClass stats = new RewardStatisticsClass(manager.GetRewardById(id), result, diagramSettings);

            return PartialView("Diagram", stats);
        }


    }
}
