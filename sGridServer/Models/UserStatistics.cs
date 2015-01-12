using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class represents required information about the user for the statistics.
    /// </summary>
    public class UserStatistics
    {
        /// <summary>
        /// The user associated with the UserStatistics object.
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Required Information of the user fore the statistics.
        /// </summary>
        public IEnumerable<Tuple<DateTime, int>> StatisticsInfo { get; set; }

        public Statistics DiagramSettings { get; set; }

        /// <summary>
        /// The constructor of the class.
        /// </summary>
        /// <param name="Account">Account of the user whom statistics has to be shown.</param>
        /// <param name="StatisticsInfo">Required Information of the user for the statistics.</param>
        public UserStatistics(Account Account, IEnumerable<Tuple<DateTime, int>> StatisticsInfo, Statistics diagramSettings)
        {
            this.Account = Account;
            this.StatisticsInfo = StatisticsInfo;
            this.DiagramSettings = diagramSettings;
        }
    }
}