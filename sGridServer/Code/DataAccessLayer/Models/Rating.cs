using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a reward rating done by a user.
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// Gets or sets the id of an element in the rating database set.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the rating of the reward.
        /// </summary>
        [Range(1, 5)]
        public int RatedWith { get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the rating operation happened.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// Gets or sets the id of the rated reward.
        /// </summary>
        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        /// <summary>
        /// Gets or sets the id of the user who rated a reward.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user who rated a reward.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets the rated reward.
        /// </summary>
        public virtual Reward Reward { get; set; }


        /// <summary>
        /// A default constructor of the rating.
        /// </summary>
        public Rating()
        {
            this.Timestamp = DateTime.Now;
        }
    }
}