using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Models;
using sGridServer.Code.GridProviders;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.Statistics.Statistics;
using sGridServer.Code.Rewards;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing statistics views. 
    /// It provides functions to show sGrid and current user’s statistics and high score tables.
    /// </summary>
    public class StatisticsController : Controller
    {
        /// <summary>
        /// Returns the partial ListRankingView, showing the ranking of all the sGrid users
        /// </summary>
        /// <returns>A partial ListRankingView.</returns>
        public ActionResult AllUserRanking(int? page)
        {
            if (page == null)
            {
                page = 1;
            }
            //gets a highscore list of all sGrid users
            IEnumerable<UserScore> userScores = HighscoreHelper.GetHighscoreList();
            userScores = userScores.Skip(6 * (page.Value - 1)).Take(6);
            //transfers the list with the information for a ranking to the PartialView
            if (userScores.Count() != 0)
            {
                return PartialView("ListRanking", new Tuple<IEnumerable<UserScore>, int, String>(userScores, page.Value, Resource.AllUsers));
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoText, Resource.Ranking);
                return PartialView("ListRanking", new Tuple<IEnumerable<UserScore>, int, String>(null, page.Value, null));
            }
        }
        /// <summary>
        /// Returns the partial view, showing the 10 top ranked users.
        /// </summary>
        /// <returns>A partial ListRaking view</returns>
        public ActionResult GetTopRanked()
        {
            IEnumerable<UserScore> userScores = HighscoreHelper.GetHighscoreList();
            if (userScores != null)
            {
                userScores = userScores.Take(10);
            }
            else
            {
                userScores = new LinkedList<UserScore>();
            }
            return PartialView("ListTopRanked", userScores);
        }
        /// <summary>
        /// Returns the partial view, showing the top 10 projects.
        /// </summary>
        /// <returns>A partial ListTopProjects view</returns>
        public ActionResult GetTopProjects()
        {
            IEnumerable<User> users = new MemberManager().Users;
            IEnumerable<GridProjectDescription> projects = GridProviderManager.RegisteredProviders.SelectMany(p => p.AvailableProjects).OrderByDescending(u => u.CoinsPerResult);
            if (projects != null)
            {
                projects = projects.Take(10);
            }
            else
            {
                projects = new LinkedList<GridProjectDescription>();
            }
            return PartialView("ListTopProjects", projects);
        }
        /// <summary>
        /// Returns the partial view, showing the 10 top ranked users.
        /// </summary>
        /// <returns>A partial ListRaking view</returns>
        public ActionResult GetTopRatedRewards()
        {
            RewardManager manager = new RewardManager();
            IEnumerable<Reward> rewards = manager.GetAllExistingRewards().Where(u => manager.IsActive(u));
            rewards = rewards.OrderByDescending(u => (u.Ratings.Count == 0 ? 0 : u.Ratings.Average(x => x.RatedWith)));
            rewards = rewards.Take(10);
            return PartialView("ListTopRatedRewards", rewards);
        }
        /// <summary>
        /// Returns the partial ListRankingView, showing the ranking of the friends of the corresponding user.
        /// </summary>
        /// <param name="id">The id of the user whose friends are to shown in the ranking.</param>
        /// <returns>A partial ListRankingView.</returns>
        public ActionResult FriendsRanking(int? page, int? id)
        {           
            MemberManager memberManager = new MemberManager();
            
            if (page == null)
            {
                page = 1;
            }

            //result highscore list
            IEnumerable<UserScore> friendsRanking;

            if (id != null && id > 0)
            {
                //gets a user with the given id
                User current = memberManager.GetAccountById(id.Value) as User;
                //chooses only friends from this user for the ranking
                friendsRanking = HighscoreHelper.GetFriendsHighscoreList(current);
            }
            else
            {
                //if the id wasn't set chooses friends from the current user for the ranking
                friendsRanking = HighscoreHelper.GetFriendsHighscoreList(SecurityProvider.CurrentUser as User);
            }

            friendsRanking = friendsRanking.Skip(6 * (page.Value - 1)).Take(6);

            //transfers the list with the information for a ranking to the PartialView
            if (friendsRanking.Count() != 0)
            {
                return PartialView("ListRanking", new Tuple<IEnumerable<UserScore>, int, String>(friendsRanking, page.Value, Resource.FriendsRanking));
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoText, Resource.Ranking);
                return PartialView("ListRanking", new Tuple<IEnumerable<UserScore>, int, String>(null, page.Value, null));
            }
        }

        /// <summary>
        /// Returns the partial ListStatisticsView, showing the statistic for all the sGrid users for the given time span. 
        /// </summary>
        /// <param name="from">The start date of the time span.</param>
        /// <param name="to">The end date of the time span.</param>
        /// <returns>A partial ListStatisticsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult ListGlobalStatistics(DateTime from, DateTime to)
        {
            GridPerformanceData result = GridProviderManager.GlobalSummaryRange(from, to.AddDays(1));
            if (result.ResultCount != 0)
            {
                return PartialView("ListStatistics", result);
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoText, Resource.StatisticsText);
                return PartialView("ListStatistics", null);
            }
        }

        /// <summary>
        /// Returns the partial ListStatisticsView, showing the statistic for the current user for the given time span.
        /// </summary>
        /// <param name="from">The start date of the time span.</param>
        /// <param name="to">The end date of the time span.</param>
        /// <param name="id">The statistic is according to the user with this id.</param>
        /// <returns>A partial ListStatisticsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult ListUserStatistics(DateTime from, DateTime to, int? id)
        {
            MemberManager memberManager = new MemberManager();
            GridProviderManager providerManager;

            if (id == null)
            {
                //if the id wasn't given it creates a new provider manager for the current user
                 providerManager = new GridProviderManager(SecurityProvider.CurrentUser as User);
            }
            else
            {
                //finds the user with the given id and creates a new provider manafer for this user
                User current = memberManager.GetAccountById(id.Value) as User;
                providerManager = new GridProviderManager(current);
            }

            GridProjectDescription project = providerManager.CurrentProject;
            GridPerformanceData data = providerManager.CurrentSummaryRange(from, to.AddDays(1));
            if (data.ResultCount != 0)
            {
                return PartialView("ListStatistics", data);
            }
            else 
            {
                ViewBag.Message = String.Format(Resource.NoText, Resource.StatisticsText);
                return PartialView("ListStatistics");
            }
        }

        /// <summary>
        /// Shows the RankingView.
        /// </summary>
        /// <param name="id">The id of the user, who the ranking has to be made for.</param>
        /// <returns>The RankingView.</returns>
        public ActionResult Ranking(int? id)
        {
            User current;
            MemberManager manager = new MemberManager();

            if (SecurityProvider.Context != null)
            {
                if (id == null && SecurityProvider.Context.UserPermissions == SiteRoles.User)
                {
                    //sets the current user as the user, who the ranking has to be made for
                    current = SecurityProvider.CurrentUser as User;
                    return View(current);
                }
                else if (id != null && SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
                {
                    //gets the user with the given id and sest him as the user, who the ranking has to be made for
                    current = manager.GetAccountById(id.Value) as User;
                    return View(current);
                }
            }
            return View();
        }

        /// <summary>
        /// Shows the UserStatisticsView.
        /// </summary>
        /// <param name="id">The id of the user whose statistic is to shown.</param>
        /// <returns>The UserStatisticsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult UserStatistics(int? id)
        {
            User current;
            MemberManager manager = new MemberManager();

            if (id == null && SecurityProvider.Context.UserPermissions == SiteRoles.User)
            {
                //sets the current user as the user, who the ranking has to be made for
                current = SecurityProvider.CurrentUser as User;
                return View(current);
            }
            else if (id != null && SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
            {
                //gets the user with the given id and sest him as the user, who the ranking has to be made for
                current = manager.GetAccountById(id.Value) as User;
                return View(current);
            }
                
            return View();
        }

        /// <summary>
        /// Returns a GlobalStatistics PartialView.
        /// </summary>
        /// <returns>The PartialView GlobalStatistics.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult GlobalStatistics()
        {
            return PartialView("GlobalStatistics");
        }

        /// <summary>
        /// Returns a MyStatistics PartialView.
        /// </summary>
        /// <param name="id">The id of the user whose performance statistic is to show.</param>
        /// <returns>The PartialView MyStatistics.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult MyStatistics(int? id)
        {
            //if the id was given there is at first the user to find
            if (id == null)
            {
                return PartialView("MyStatistics");
            }
            else
            {
                MemberManager manager = new MemberManager();
                User current = manager.GetAccountById(id.Value) as User;
                return PartialView("MyStatistics", current);
            }
        }

        /// <summary>
        /// Returns a UserStatistics object with parameters which are between the two given timespan.
        /// The UserStatistics object contains the number and the timestamp of the calculated packages.
        /// </summary>
        /// <param name="userId">The id of the user whom statistics are to be shown.</param>
        /// <param name="fromDate">The start date of the time span.</param>
        /// <param name="to">The end time of the time span.</param>
        /// <param name="diagramSettings">The setting of the diagram.</param>
        /// <returns>The View that shows the statistics as a diagram.</returns>
        public ActionResult CalculatedPackages(int userId, DateTime fromDate, DateTime to, Statistics diagramSettings)
        {
            StatisticsDatesHelper.SetTimespanForUserStatistics(userId, ref fromDate, ref to);
            MemberManager manager = new MemberManager();
            User user = manager.GetAccountById(userId) as User;

            IEnumerable<Tuple<DateTime, int>> calculatedPackages = from c in user.CalculatedResults
                                                                   where c.ServerReceivedTimestamp >= fromDate && c.ServerReceivedTimestamp <= to && c.Valid
                                                                   group c by new { c.ServerReceivedTimestamp.DayOfYear, c.ServerReceivedTimestamp.Year } into groups
                                                                   orderby groups.First().ServerReceivedTimestamp descending
                                                                   select new Tuple<DateTime, int>(
                                                                     groups.First().ServerReceivedTimestamp,
                                                                     groups.Count());

            List<Tuple<DateTime, int>> dates = new List<Tuple<DateTime, int>>();
            int i = 0;
            for (DateTime start = fromDate; start <= to; start = start.AddDays(1))
            {
                dates.Add(new Tuple<DateTime, int>(start, ++i));
            }

            IEnumerable<Tuple<DateTime, int>> result = from a in dates
                                                       join c in calculatedPackages on new { a.Item1.Year, a.Item1.DayOfYear } equals new { c.Item1.Year, c.Item1.DayOfYear } into q
                                                       from suba in q.DefaultIfEmpty()
                                                       select new Tuple<DateTime, int>(new DateTime(a.Item1.Year, a.Item1.Month, a.Item1.Day), suba != null ? suba.Item2 : 0);
            
            UserStatistics stats = new UserStatistics(manager.GetAccountById(userId), result, diagramSettings);
            return PartialView("Diagram", stats);
            
        }

        /// <summary>
        /// Returns the statistics, which shows a total runtime by calculating packages of the given user between the two given timestamps.
        /// The UserStatistics object contains the uptime of the user. 
        /// </summary>
        /// <param name="userId">The id of the user whom statistics are to be shown.</param>
        /// <param name="fromDate">The start date of the time span.</param>
        /// <param name="to">The end time of the time span.</param>
        /// <param name="diagramSettings">The settings of the diagram.</param>
        /// <returns>The View with the statistics as a diagram.</returns>
        public ActionResult Uptime(int userId, DateTime fromDate, DateTime to, Statistics diagramSettings)
        {
            StatisticsDatesHelper.SetTimespanForUserStatistics(userId, ref fromDate, ref to);
            MemberManager memberManager = new MemberManager();
            User user = memberManager.GetAccountById(userId) as User;
            GridProviderManager gridProviderManager = new GridProviderManager(user);
           
            IEnumerable<Tuple<DateTime, int>> uptime = from c in gridProviderManager.Results
                                                       where c.Valid && c.ServerReceivedTimestamp >= fromDate && c.ServerReceivedTimestamp <= to
                                                       group c by new { c.ServerReceivedTimestamp.DayOfYear, c.ServerReceivedTimestamp.Year } into groups
                                                           orderby groups.First().ServerReceivedTimestamp descending
                                                           select new Tuple<DateTime, int>(
                                                               groups.First().ServerReceivedTimestamp,
                                                               (int)groups.Sum(g => (g.ServerReceivedTimestamp - g.ServerSentTimestamp).TotalSeconds /3600));

            List<Tuple<DateTime, int>> dates = new List<Tuple<DateTime, int>>();
            int i = 0;
            for (DateTime start = fromDate; start <= to; start = start.AddDays(1))
            {
                dates.Add(new Tuple<DateTime, int>(start, ++i));
            }

            IEnumerable<Tuple<DateTime, int>> result = from a in dates
                                                       join c in uptime on new { a.Item1.Year, a.Item1.DayOfYear } equals new { c.Item1.Year, c.Item1.DayOfYear } into q
                                                       from suba in q.DefaultIfEmpty()
                                                       select new Tuple<DateTime, int>(new DateTime(a.Item1.Year, a.Item1.Month, a.Item1.Day), suba != null ? suba.Item2 : 0);

            UserStatistics stats = new UserStatistics(memberManager.GetAccountById(userId), result, diagramSettings);
            return PartialView("Diagram", stats);
        }

        /// <summary>
        /// Returns statistics of achievements, shich are obtained by the given user between the two given timestamps.
        /// The UserStatistics object contains the number and the timestamp of the obtained achievements. 
        /// </summary>
        /// <param name="userId">The id of the user whom statistics are to be shown.</param>
        /// <param name="fromDate">The start date of the time span.</param>
        /// <param name="to">The end time of the time span.</param>
        /// <param name="diagramSettings">The settings of the diagram.</param>
        /// <returns>The View with the statistics as a diagram.</returns>
        public ActionResult ObtainedAchievements(int userId, DateTime fromDate, DateTime to, Statistics diagramSettings)
        {
            StatisticsDatesHelper.SetTimespanForUserStatistics(userId, ref fromDate, ref to);
            MemberManager manager = new MemberManager();
            User user = manager.GetAccountById(userId) as User;

            IEnumerable<Tuple<DateTime, int>> obtainedAchievements = from c in user.ObtainedAchievements
                                                                     where c.AchievementTimestamp >= fromDate && c.AchievementTimestamp <= to
                                                                     group c by new { c.AchievementTimestamp.DayOfYear, c.AchievementTimestamp.Year } into groups
                                                                     orderby groups.First().AchievementTimestamp descending
                                                                     select new Tuple<DateTime, int>(
                                                                       groups.First().AchievementTimestamp,
                                                                       groups.Count());
                        
            List<Tuple<DateTime, int>> dates = new List<Tuple<DateTime, int>>();
            int i = 0;
            for (DateTime start = fromDate; start <= to; start = start.AddDays(1))
            {
                dates.Add(new Tuple<DateTime, int>(start, ++i));
            }

            IEnumerable<Tuple<DateTime, int>> result = from a in dates
                                                       join c in obtainedAchievements on new { a.Item1.Year, a.Item1.DayOfYear } equals new { c.Item1.Year, c.Item1.DayOfYear } into q
                                                       from suba in q.DefaultIfEmpty()
                                                       select new Tuple<DateTime, int>(new DateTime(a.Item1.Year, a.Item1.Month, a.Item1.Day), suba != null ? suba.Item2 : 0);

            UserStatistics stats = new UserStatistics(manager.GetAccountById(userId), result, diagramSettings);

            return PartialView("Diagram", stats);
        }

        /// <summary>
        /// Returns the statistics of coins, which are spent by users for rewards of the given coin partner between the two given timestamps.
        /// The UserStatistics object contains the number and the timestamp of the spent coins. 
        /// </summary>
        /// <param name="id">The id of the coin partner whom statistics are to be shown.</param>
        /// <param name="fromDate">The start date of the time span.</param>
        /// <param name="to">The end time of the time span.</param>
        /// <param name="diagramSettings">The setting of the diagram.</param>
        /// <returns>The View with the statistics as a diagram.</returns>
        public ActionResult CoinPartnerStatistics(int id, DateTime fromDate, DateTime to, Statistics diagramSettings)
        {
            MemberManager memberManager = new MemberManager();
            RewardManager rewardManager = new RewardManager();
            StatisticsDatesHelper.SetTimespanForCoinPartnerStatistics(id, ref fromDate, ref to);


            IEnumerable<Tuple<DateTime, int>> purchases = from c in rewardManager.GetPurchases()
                                                          where c.Reward.CoinPartnerId == id && c.Timestamp >= fromDate && c.Timestamp <= to
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


            UserStatistics stats = new UserStatistics(memberManager.GetAccountById(id), result, diagramSettings);

            return PartialView("Diagram", stats);
        }
    }
}
