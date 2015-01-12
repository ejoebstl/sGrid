using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Rewards;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class helps to pass dates for statistics.
    /// </summary>
    public class StatisticsDatesHelper
    {
        /// <summary>
        /// Sets dates from the "from" and "to" timestamps to the timestamps which could be set depending on the data list.
        /// </summary>
        /// <param name="min">The minimal timestamp, which can be used for statistics.</param>
        /// <param name="max">The maximal timestamp, which can be used for statistics.</param>
        /// <param name="fromDate">The date which has to be set to the minimal valid timestamp for the statistics.</param>
        /// <param name="to">The date which has to be set to the maximal valid timestamp for the statistics.</param>
        public static void SetTimespan(DateTime min, DateTime max, ref DateTime fromDate, ref DateTime to)
        {
            if (fromDate < min)
            {
                fromDate = min;
            }

            if (to > max)
            {
                to = max;
            }

            if ((to - fromDate).Days < 1)
            {
                to = to.AddDays(1);
            }
        }

        /// <summary>
        /// Sets timespan for the user statistics.
        /// </summary>
        /// <param name="userId">The id of the user whom statistics is to be shown.</param>
        /// <param name="fromDate">The first timestamp for the statistics.</param>
        /// <param name="to">The last timestamp for the statistics.</param>
        public static void SetTimespanForUserStatistics(int userId, ref DateTime fromDate, ref DateTime to)
        {
            MemberManager manager = new MemberManager();
            User user = manager.GetAccountById(userId) as User;

            DateTime min = user.RegistrationDate;
            DateTime max = DateTime.Today.AddDays(1);

            SetTimespan(min, max, ref fromDate, ref to);
        }

        /// <summary>
        /// Sets timespan for the reward statistics.
        /// </summary>
        /// <param name="userId">The id of the reward whom statistics is to be shown.</param>
        /// <param name="fromDate">The first timestamp for the statistics.</param>
        /// <param name="to">The last timestamp for the statistics.</param>
        public static void SetTimespanForRewardStatistics(int id, ref DateTime fromDate, ref DateTime to)
        {
            RewardManager manager = new RewardManager();
            Reward reward = manager.GetRewardById(id);

            DateTime min = reward.Begin;
            DateTime max = DateTime.Today.AddDays(1);

            SetTimespan(min, max, ref fromDate, ref to);
        }

        /// <summary>
        /// Sets timespan for the coin partner statistics.
        /// </summary>
        /// <param name="userId">The id of the coin partner whom statistics is to be shown.</param>
        /// <param name="fromDate">The first timestamp for the statistics.</param>
        /// <param name="to">The last timestamp for the statistics.</param>
        public static void SetTimespanForCoinPartnerStatistics(int id, ref DateTime fromDate, ref DateTime to)
        {
            MemberManager manager = new MemberManager();
            CoinPartner partner = manager.GetAccountById(id) as CoinPartner;

            DateTime min = partner.RegistrationDate;
            DateTime max = DateTime.Today.AddDays(1);

            SetTimespan(min, max, ref fromDate, ref to);
        }
    }
}