using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a coin partner.
    /// </summary>
    public class CoinPartner : Partner
    {
        /// <summary>
        /// Gets or sets the answer for the security question of the coin partner.
        /// </summary>
        public String SecurityAnswer { get; set; }

        /// <summary>
        /// Gets or sets the security question of the coin partner.
        /// </summary>
        public String SecurityQuestion { get; set; }


        /// <summary>
        /// Gets or sets the list of the rewards which are provided by the coin partner.
        /// </summary>
        public virtual List<Reward> Rewards { get; set; }


        /// <summary>
        /// A default constructor of the coin partner.
        /// </summary>
        public CoinPartner()
        {
            this.SecurityAnswer = "";
            this.SecurityQuestion = "";
            this.UserPermission = SiteRoles.CoinPartner;
        }
    }
}