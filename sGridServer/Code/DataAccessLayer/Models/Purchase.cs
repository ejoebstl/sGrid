using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a done purchase and is used for statistical purposes.
    /// </summary>
    public class Purchase
    {
        /// <summary>
        /// Gets or sets the id of the purchase.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the timestamp of the purchase.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the amounts of rewards bought during this purchase. 
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the id of the bought reward.
        /// </summary>
        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        /// <summary>
        /// Gets or sets the id of the user who bought the reward.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the bought reward
        /// </summary>
        public virtual Reward Reward { get; set; }

        /// <summary>
        /// Gets or sets the user who bought the reward.
        /// </summary>
        public virtual User User { get; set; }


        /// <summary>
        /// A default countructor of the purchase.
        /// </summary>
        public Purchase()
        {
            this.Timestamp = DateTime.Now;
        }
    }
}