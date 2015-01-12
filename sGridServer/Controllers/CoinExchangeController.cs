using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Models;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Rewards;
using sGridServer.Code.CoinExchange;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.CoinExchange.CoinExchange;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the coin exchange views and conducting buys and searches.
    /// It also provides functions to rate or report a reward. Users can also view the number coins they have
    /// earned or spent during a given time span.
    /// </summary>
    public class CoinExchangeController : Controller
    {
        /// <summary>
        /// Creates a new instance of CheckOutData and returns the partial CheckoutView for the given reward.
        /// </summary>
        /// <param name="id">The identifier of the reward to check out.</param>
        /// <returns>The partial CheckoutView for the given reward.</returns>
        public ActionResult Checkout(int id)
        {
            RewardManager manager = new RewardManager();
            Reward item = manager.GetRewardById(id);
            if (manager.IsActive(item))
            {
                CheckOutData data = new CheckOutData() { Quantity = 1, Reward = item };
                return PartialView(data);
            }
            throw new HttpException(404, Resource.RewardUnavailible);
        }
        /// <summary>
        /// Returns the CoinStatisticsView showing the number of coins user has earned and spent during the given time span.
        /// </summary>
        /// <returns>The CoinStatisticsView for the given reward.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult CoinStatistics()
        {
            return PartialView();
        }
        /// <summary>
        /// Gets number of earned and spent coins for the user in a given timespan.
        /// </summary>
        /// <param name="from">The start date of the time span.</param>
        /// <param name="to">The end date of the time span.</param>
        /// <param name="id">The id of the user to get the statistics for.</param>
        /// <returns>The content to show to the user in the view.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult GetStatistics(DateTime from, DateTime to, int? id)
        {
            User current;
            /*Set current to the user, given by id, if current user is admin (since they are able to view statistics for other users).
            Set user to current user, otherwise.*/
            if (id != null && id > 0 && SecurityProvider.CurrentUser is SGridTeamMember)
            {
                current = (new MemberManager()).GetAccountById(id.Value) as User;
            } 
            else
            {
                current = SecurityProvider.CurrentUser as User;
            }
            int spent = 0;
            int got = 0;
            if (current != null)
            {
                //get number of coins spent in a given timespan (user "current" is a source).
                spent = CoinExchange.GetTransactions(current).Where(u => ((u.Timestamp > from) && (u.Timestamp < to)) && u.SourceId == current.CoinAccountId).Sum(x => x.Value);
                //get number of coins earned in a given timespan (user "current" is a destination).
                got = CoinExchange.GetTransactions(current).Where(u => ((u.Timestamp > from) && (u.Timestamp < to)) && u.DestinationId == current.CoinAccountId).Sum(x => x.Value);
            }
            return Content(Resource.Spent + ": " + spent + "|" + @Resource.Got + ": " + got, "text/html");
        }
        /// <summary>
        /// Returns the partial ListView showing rewards that match the given parameters.
        /// </summary>
        /// <param name="sortOption">A String which specifies the attribute the rewards should be sorted by. </param>
        /// <param name="sortOrder">A String which specifies the sorting order.</param>
        /// <param name="searchName">If this parameter is set, only rewards which names match the given string are returned.</param>
        /// <param name="partnerId">If this parameter is set to any other value than -1, only rewards belonging to the partner with the given id are returned.</param>
        /// <returns>The ListView according to the given parameters.</returns>
        public ActionResult List(String sortOption, String sortOrder, String searchName, int? partnerId)
        {
            RewardManager manager = new RewardManager();
            IEnumerable<Reward> items = manager.GetAllExistingRewards().Where(u => u.Amount > 0);
            //Only active rewards should be shown.
            items = items.Where(u => manager.IsActive(u));
            User current = SecurityProvider.CurrentUser as User;
            string currentLanguage = LanguageManager.CurrentLanguage.Code;
            if (searchName != null && searchName != "")
            {
                //get items, which translation of the name that contains the given string.
                items = items.Where(u => u.Name.Translations.Where(x => x.Culture == currentLanguage).FirstOrDefault().Text.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
            }
            if (partnerId != null && partnerId >= 0)
            {
                //get items, which were provided by a given partner.
                items = items.Where(u => u.CoinPartnerId == partnerId.Value);
            }
            //Sort items by Rating, Popularity, Cost or Name (default) in ascending or descending order.
            if (sortOption == "Rating")
            {
                if (sortOrder == "Descending")
                {
                    //Set rating of the reward to 0, if no rating exists and sort by rating (descending order). 
                    items = items.OrderByDescending(u => (u.Ratings.Count == 0 ? 0 : u.Ratings.Average(x => x.RatedWith)));
                }
                else
                {
                    //Set rating of the reward to 0, if no rating exists and sort by rating (ascending order). 
                    items = items.OrderBy(u => (u.Ratings.Count == 0 ? 0 : u.Ratings.Average(x => x.RatedWith)));
                }
            }
            else if (sortOption == "Popularity")
            {
                if (sortOrder == "Descending")
                {
                    //Sort items by popularity (descending order).
                    items = items.OrderByDescending(u => u.Purchases.Sum(x => x.Amount));
                }
                else
                {
                    //Sort items by popularity (ascending order).
                    items = items.OrderBy(u => u.Purchases.Sum(x => x.Amount));
                }
            }
            else if (sortOption == "Cost")
            {
                if (sortOrder == "Descending")
                {
                    //Sort items by cost (descending order).
                    items = items.OrderByDescending(u => u.Cost);
                }
                else
                {
                    //Sort items by cost (ascending order).
                    items = items.OrderBy(u => u.Cost);
                }
            }
            else
            {
                //Default sorting by name (desdending or ascending order).
                if (sortOrder == "Descending")
                {
                    items = items.OrderByDescending(u => u.Name.Translations.Where(x => x.Culture == currentLanguage).First().Text);
                }
                else
                {
                    items = items.OrderBy(u => u.Name.Translations.Where(x => x.Culture == currentLanguage).First().Text);
                }
            }
            return PartialView(items);
        }
        /// <summary>
        /// Performs the checkout according to the given CheckOutData object.
        /// </summary>
        /// <param name="data">The CheckOutData object which carries the information needed to perform the check out.</param>
        /// <returns>Content to show to the user indicating error or success.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult PerformCheckout(CheckOutData data)
        {
            User current = SecurityProvider.CurrentUser as User;

            CoinExchange exchange = new CoinExchange(current);

            RewardManager rewardManager = new RewardManager();
            data.Reward = rewardManager.GetRewardById(data.Reward.Id);
            //Begin the buy
            ICoinTransaction transaction = exchange.BeginBuy(data.Reward, data.Quantity);
            //Try to buy the reward
            try
            {
                if (transaction.CanBuy)
                {
                    //Reward can be bought, thank the user and provide the url the user can use.
                    Purchase purchase = transaction.EndBuy();
                    string url = rewardManager.ReceiveReward(purchase);
                    return Content(Resource.PurchaseDone + "<a href = \"" + url + "\">" + url + "</a>", "text/html");
                }
            }
            //Buying process has to be ended.
            finally
            {
                transaction.CancelBuy();
            }
            //Provide message explaining what went wrong.
            if (!transaction.HasEnoughCoins)
            {
                return Content(Resource.NotEnoughCoins, "text/html");
            }
            else if (!transaction.RewardAvailable)
            {
                return Content(Resource.NotEnoughRewards, "text/html");
            }
            
            return Content(Resource.UnknownError, "text/html"); 
        }
        /// <summary>
        /// Performs a rating operation on the given reward.
        /// </summary>
        /// <param name="id">The identifier of the reward to perform the rating operation on.</param>
        /// <param name="rating">The rating to set for the reward.</param>
        /// <returns>Content to show to the user.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        [HttpPost]
        public ActionResult RateReward(int id, int rating)
        {
            RewardManager manager = new RewardManager();
            Reward reward = manager.GetRewardById(id);
            if (manager.IsActive(reward))
            {
                User user = SecurityProvider.CurrentUser as User;
                manager.RateReward(reward, user, rating);
                return Content(Resource.ThankYou);
            }
            throw new HttpException(404, Resource.RewardUnavailible);
        }
        /// <summary>
        /// Shows the view to report the given reward.
        /// </summary>
        /// <param name="rewardId">The rewardId corresponding to the reward to report.</param>
        /// <returns>Redirect to ShowContactForm action with provided message and subject with the given reward.</returns>
        public ActionResult ReportReward(int rewardId)
        {
            RewardManager manager = new RewardManager();
            Reward item = manager.GetRewardById(rewardId);
            if (manager.IsActive(item))
            {
                //Autofill ShowContactForm
                string currentLanguage = LanguageManager.CurrentLanguage.Code;
                String rewardName = item.Name.Translations.Where(x => x.Culture == currentLanguage).First().Text;
                String subject = String.Format(Resource.Report, rewardName);
                String message = String.Format(Resource.ReportMessage, rewardName);
                return RedirectToAction("ShowContactForm", "Contact", new { message = message, subject = subject });
            }
            throw new HttpException(404, Resource.RewardUnavailible);
        }
        /// <summary>
        /// Returns the partial ListView, showing the rewards purchased by the current user.
        /// </summary>
        /// <param name="id">The identifier of the user to get the rewards (purchases) list for.</param>
        /// <returns>The partial ListMyRewardsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult ListMyRewards(int? id)
        {
            User current = SecurityProvider.CurrentUser as User;
            int currentId = current.Id;
            if (id != null && (SecurityProvider.CurrentUser as SGridTeamMember != null))
            {
                current = (new MemberManager()).GetAccountById(id.Value) as User;
                currentId = id.Value;
            }
            RewardManager manager = new RewardManager();
            //Get a purchase list to show bought rewards and time the rewards were bought.
            IEnumerable<Purchase> items = new List<Purchase>();
            if (current != null)
            {
                items = manager.GetPurchases().Where(u => u.UserId == currentId);
            }
            return View("MyRewardList", items);
        }
        /// <summary>
        /// Returns the detailed view of the given reward.
        /// </summary>
        /// <param name="id">The identifier of the reward to get the details for.</param>
        /// <returns>The DetailsView for the given reward.</returns>
        public ActionResult Detail(int id)
        {
            RewardManager manager = new RewardManager();
            Reward item = manager.GetRewardById(id);
            if (manager.IsActive(item))
            {
                Account account = SecurityProvider.CurrentUser;
                bool showAddRankingControl = true;
                int userRanking = 0;
                if (account != null && account is User)
                {
                    User current = account as User;
                    //Get rating of the current user for this reward.
                    List<Rating> ratingList = current.Ratings;
                    foreach (Rating ratingItem in ratingList)
                    {
                        if (ratingItem.RewardId == id)
                        {
                            userRanking = ratingItem.RatedWith;
                            //Set showAddRankingControl to false, since the user has alread rated this reward.
                            showAddRankingControl = false;
                            break;
                        }
                    }
                }
                else
                {
                    //Set showAddRankingControl to false, since only registered users are able to rate a reward.
                    showAddRankingControl = false;
                }
                RewardDetailModel model = new RewardDetailModel() { Reward = item, ShowAddRanking = showAddRankingControl, UserRanking = userRanking };
                return View(model);
            }
            throw new HttpException(404, Resource.RewardUnavailible);
        }
        /// <summary>
        /// Shows the RewardOverviewView.
        /// </summary>
        /// <returns>The RewardOverviewView.</returns>
        public ActionResult RewardOverview()
        {
            //Get the list of coin partners to shown in the view.
            IEnumerable<CoinPartner> partner = (new MemberManager()).CoinPartners;
            return View(partner);
        }
    }
}
