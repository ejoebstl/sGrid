using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents an achievement.
    /// </summary>
    public class Achievement
    {
        /// <summary>
        /// Gets or sets the achievement id.
        /// </summary>
        [Key]
        public int AchievementId { get; set; }

        /// <summary>
        /// Gets or sets the type of the achievement.
        /// </summary>
        public String AchievementType { get; set; }

        /// <summary>
        /// Gets or sets if the achievement can be achieved.
        /// An achievement cannot be deleted, but it can be deactivated.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the amount of bonus coins to grant for the achievement.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int BonusCoins { get; set; }

        /// <summary>
        /// Gets or sets the description of the achievement.
        /// </summary>
        public virtual MultiLanguageString Description { get; set; }

        /// <summary>
        /// Gets or sets the parameters which are specific for this achievement type 
        /// and are needed to create a new achievement of this type as url encoded string.
        /// </summary>
        public String ExtendedParameters { get; set; }

        /// <summary>
        /// Gets or sets the url of the icon of the achievement.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Icon { get; set; }

        /// <summary>
        /// Gets or sets the name of the achievement.
        /// </summary>
        public virtual MultiLanguageString Name { get; set; }

        /// <summary>
        /// Gets or sets a list with elements of type ObtainedAchievement
        /// which shows the users who had reached this achievement.
        /// </summary>
        public virtual List<ObtainedAchievement> ObtainedAchievements { get; set; }


        /// <summary>
        /// A default constructor of the achievement.
        /// </summary>
        public Achievement()
        {
            this.AchievementType = "";
            //this.Description = new MultiLanguageString();
            this.ExtendedParameters = "";
            this.Icon = "";
            //this.Name = new MultiLanguageString();
        }
    }
}
