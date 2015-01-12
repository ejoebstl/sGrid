using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents achievements which have been obtained by users.
    /// </summary>
    public class ObtainedAchievement
    {
        /// <summary>
        /// Gets or sets the id of an element in the obtained achievement database set.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the achievement was obtained.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime AchievementTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a bool indicating whether the obtained achievement has already been shown to the user.
        /// </summary>
        public bool AlreadyShown { get; set; }


        /// <summary>
        /// Gets or sets the id of the obtained achievement. 
        /// </summary>
        [ForeignKey("Achievement")]
        public int AchievementId { get; set; }

        /// <summary>
        /// Gets or sets the account id of the user who obtained the achievement.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets a user who obtained the achivement.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets the obtained achievement.
        /// </summary>
        public virtual Achievement Achievement { get; set; }


        /// <summary>
        /// A default constructor of the obtained achivement.
        /// </summary>
        public ObtainedAchievement()
        {
            this.AchievementTimestamp = DateTime.Now;
        }
    }
}
