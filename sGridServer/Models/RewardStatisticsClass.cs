using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    public class RewardStatisticsClass
    {
        public Reward Reward { get; set; }
        public IEnumerable<Tuple<DateTime, int>> StatisticsInfo { get; set; }
        public Statistics DiagramSettings { get; set; }


        public RewardStatisticsClass(Reward reward, IEnumerable<Tuple<DateTime, int>> statisticsInfo, Statistics diagramSettings)
        {
            this.Reward = reward;
            this.StatisticsInfo = statisticsInfo;
            this.DiagramSettings = diagramSettings;
        }
    }
}