using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a reward.
    /// </summary>
    public class Reward
    {
        /// <summary>
        /// Gets or sets the id of the reward.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets an integer indicating the number of available items of this reward. 
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets a bool indicating whether a reward was approved by sGrid team.
        /// </summary>
        public bool  Approved { get; set; }

        /// <summary>
        /// Gets or sets a timestamp which specifies from when the reward should be available.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Begin { get; set; }

        /// <summary>
        /// Gets or sets the costs of the reward in coins.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Cost { get; set; }

        /// <summary>
        /// Gets or sets the description of the reward.
        /// </summary>
        public virtual MultiLanguageString Description { get; set; }

        /// <summary>
        /// Gets or sets a timestamp which specifies until when the reward should be available.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the parameters which are specific for this reward type 
        /// and are needed to create a new reward of this type as url encoded string.
        /// </summary>
        public String ExtendedParameters { get; set; }

        /// <summary>
        /// Gets or sets the name of the reward.
        /// </summary>
        public virtual MultiLanguageString Name { get; set; }

        /// <summary>
        /// Gets or sets the url of the picture of the reward.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Picture { get; set; }

        /// <summary>
        /// Gets or sets the type of the reward.
        /// </summary>
        public String RewardType { get; set; }

        /// <summary>
        /// Gets or sets the short description of the reward.
        /// </summary>
        public virtual MultiLanguageString ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the url of the reward.
        /// </summary>
        [DataType(DataType.Url)]
        public String URL { get; set; }


        /// <summary>
        /// Gets or sets the id of the coin partner which provides the reward.
        /// </summary>
        [ForeignKey("CoinPartner")]
        public int CoinPartnerId { get; set; }


        /// <summary>
        /// Gets or sets the coin partner which provides the reward.
        /// </summary>
        public virtual CoinPartner CoinPartner { get; set; }

        /// <summary>
        /// Gets or sets the list of purchases where the reward was bought.
        /// </summary>
        public virtual List<Purchase> Purchases { get; set; }

        /// <summary>
        /// Gets or sets the list of rating where the reward was rated by users.
        /// </summary>
        public virtual List<Rating> Ratings { get; set; }


        /// <summary>
        /// A default constructor of the reward.
        /// </summary>
        public Reward()
        {
            this.Begin = DateTime.Now;
            this.End = DateTime.Now;
            this.ExtendedParameters = "";
            this.Picture = "";
            this.RewardType = "";
            this.URL = "";
        }
    }
}
