using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for reward detail. 
    /// </summary>
    public class RewardDetailModel
    {
        /// <summary>
        /// The reward associated with the reward detail model.
        /// </summary>
        public Reward Reward { get; set; }
        /// <summary>
        /// Returns true, if the user can rank this reward.
        /// </summary>
        public bool ShowAddRanking { get; set; }
        /// <summary>
        /// User ranking for this reward.
        /// </summary>
        public int UserRanking { get; set; }
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RewardDetailModel()
        {
            Reward = new Reward();
            ShowAddRanking = false;
            UserRanking = 0;
        }
    }
}