using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a sponsor.
    /// </summary>
    public class Sponsor : Partner
    {
        /// <summary>
        /// Gets or sets a bool indicating whether the sponsor banner was approved by sGrid team.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// Gets or sets the url of the banner of the sponsor.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Banner { get; set; }

        /// <summary>
        /// Gets or sets the show frequency of the sponsor banner.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ShowFrequency { get; set; }


        /// <summary>
        /// A default constructor of the sponsor.
        /// </summary>
        public Sponsor()
        {
            this.Banner = "";
            this.UserPermission = SiteRoles.Sponsor;
        }
    }
}