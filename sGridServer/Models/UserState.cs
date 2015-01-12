using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This model class represents various properties of the current user. 
    /// </summary>
    public class UserState
    {
        /// <summary>
        /// The coin balance of the current user.
        /// </summary>
        public int CoinBalance { get; set; }

        /// <summary>
        /// The id of the current user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The next unshown achievement to display, or null if there is nothing to show to the user.
        /// </summary>
        public NextAchievementState NextAchievement { get; set; }

        /// <summary>
        /// The name of the current user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public UserState()
        {
            CoinBalance = 0;
            Id = -1;
            NextAchievement = null;
            Username = "";
        }
    }

    /// <summary>
    /// This model class represents various properties of the next obtained achievement to show. 
    /// </summary>
    public class NextAchievementState
    {
        /// <summary>
        /// The id of the achievement.
        /// </summary>
        public int AchievementId { get; set; }

        /// <summary>
        /// The type of the achievement. 
        /// </summary>
        public string AchievementType { get; set; }

        /// <summary>
        /// The amount of gathered bonus coins. 
        /// </summary>
        public int BonusCoins { get; set; }

        /// <summary>
        /// The description of the achievement. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The icon of the achievment. 
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The name of the achievement. 
        /// </summary>
        public string Name { get; set; }
    }
}