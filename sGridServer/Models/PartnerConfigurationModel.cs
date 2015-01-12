using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// This class is a model class for partner configuration. 
    /// </summary>
    public class PartnerConfigurationModel
    {
        /// <summary>
        /// The user associated with the configuration model.
        /// </summary>
        public CoinPartner Account { get; set; }
    }
}