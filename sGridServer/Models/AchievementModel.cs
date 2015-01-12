using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class simplifies the presentation of an achievement by the
    /// view. It contains the data that can be changed by the user and
    /// it stores the type of the achievement.
    /// </summary>
    public class AchievementModel
    {
        /// <summary>
        /// A list containing the identifying string, the human readable
        /// name and the value of every property of the achievement.
        /// </summary>
        public List<AchievementProperty> Properties { get; set; }

        /// <summary>
        /// The name of the achievement.
        /// </summary>
        public MultiLanguageString Name { get; set; }

        /// <summary>
        /// The description of the achievement.
        /// </summary>
        public MultiLanguageString Description { get; set; }

        /// <summary>
        /// True, if the achievement can be obtained.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The icon of the achievement.
        /// </summary>
        public String Icon { get; set; }

        /// <summary>
        /// The type of the achievement described by String.
        /// </summary>
        public String AchievementType { get; set; }

        /// <summary>
        /// The id of the achievement, if the model represents an
        /// achievement to edit, or -1 if a new achievement has to
        /// be created.
        /// </summary>
        public int AchievementId { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public AchievementModel()
        {
            this.Properties = new List<AchievementProperty>();
        }
    }

    /// <summary>
    /// This class represents a propery of the reward that can be
    /// shown and changed in the reward view.
    /// </summary>
    public class AchievementProperty
    {
        /// <summary>
        /// The identifying string of this property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The human readable name of this property.
        /// </summary>
        public string ShowName { get; set; }

        /// <summary>
        /// The value of this property.
        /// </summary>
        public string Value { get; set; }
    }
}