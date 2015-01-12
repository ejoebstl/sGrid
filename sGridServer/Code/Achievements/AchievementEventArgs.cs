using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Achievements
{
    /// <summary>
    /// This class represents the arguments of the
    /// AchievementsObtainedEvent.
    /// </summary>
    public class AchievementEventArgs : EventArgs 
    {
        /// <summary>
        /// Gets the achievement which has been obtained.
        /// </summary>
        public Achievement Achievement { get; private set; }

        /// <summary>
        /// Gets or user who has obtained the achievement.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores all given
        /// parameters into the corresponding properties.
        /// </summary>
        /// <param name="achievement">The achievement which has been
        /// obtained.</param>
        /// <param name="user">The user who has obtained the achievement.</param>
        public AchievementEventArgs(Achievement achievement, User user)
        {
            this.Achievement = achievement;
            this.User = user;
        }
    }
}