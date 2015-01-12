using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for sponsor configuration. 
    /// </summary>
    public class SponsorConfigurationModel
    {
        /// <summary>
        /// The user associated with the configuration model.
        /// </summary>
        public Sponsor Account { get; set; }
    }
}