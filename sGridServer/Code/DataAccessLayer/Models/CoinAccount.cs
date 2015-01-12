using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a coin account. 
    /// </summary>
    public class CoinAccount
    {
        /// <summary>
        /// Gets or sets the id of the coin account.
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the current balance of the coin account.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CurrentBalance { get; set; }

        /// <summary>
        /// Gets or sets the amount of all coins granted to this account.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalGrant { get; set; }

        /// <summary>
        /// Gets or sets the amount of all coins expended from this account.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalSpent { get; set; }

    }
}