using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class simplifies the presentation of a reward by the view.
    /// It contains the data that can be changed by the user, it
    /// stores the type of the reward and it stores the id of the reward.
    /// </summary>
    public class RewardModel
    {
        /// <summary>
        /// A list containing the identifying string, the human readable
        /// name and the value of every property of the reward, except the
        /// DateTime-properties.
        /// </summary>
        public List<RewardProperty<string>> StringProperties { get; set; }

        /// <summary>
        /// A list containing the identifying string, the human readable
        /// name and the value of every DateTime-property of the reward.
        /// </summary>
        public List<RewardProperty<DateTime>> DateTimeProperties { get; set; }

        /// <summary>
        /// The name of the reward.
        /// </summary>
        public MultiLanguageString Name { get; set; }

        /// <summary>
        /// The description of the reward.
        /// </summary>
        public MultiLanguageString Description { get; set; }

        /// <summary>
        /// A brief description explaining the reward.
        /// </summary>
        public MultiLanguageString ShortDescription { get; set; }

        /// <summary>
        /// The icon of the reward.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The type of the reward described by String.
        /// </summary>
        public string RewardType { get; set; }

        /// <summary>
        /// The id of the reward, if this model represents a reward to be
        /// edited, or -1, if the reward is to be created and therefore
        /// has not yet an id.
        /// </summary>
        public int RewardId { get; set; }

        /// <summary>
        /// This id identifies the coin partner of the reward. This
        /// id is only used if an admin wants to create a new reward
        /// for a specific coin partner.
        /// </summary>
        public int CoinPartnerId { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public RewardModel()
        {
            this.StringProperties = new List<RewardProperty<string>>();
            this.DateTimeProperties = new List<RewardProperty<DateTime>>();
        }
    }

    /// <summary>
    /// This class represents a propery of the reward that can be
    /// shown and changed in the reward view.
    /// </summary>
    public class RewardProperty<propertyType>
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
        public propertyType Value { get; set; }
    }
}