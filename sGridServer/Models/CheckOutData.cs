using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// The CheckOutData class describes a checkout process.
    /// </summary>
    public class CheckOutData
    {
        /// <summary>
        /// A message to show to the user during checkout process.
        /// </summary>
        public String CheckOutMessage { get; set; }
        /// <summary>
        /// The amount of reward items to buy.
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// The reward to buy.
        /// </summary>
        public Reward Reward { get; set; }
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public CheckOutData()
        {
            this.Reward = null;
            this.Quantity = 0;
            this.CheckOutMessage = "";
        }
    }
}