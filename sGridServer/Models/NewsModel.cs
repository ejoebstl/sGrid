using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    /// <summary>
    /// This class represents a model of a news object.
    /// </summary>
    public class NewsModel
    {
        /// <summary>
        /// Subject of the news object.
        /// </summary>
        public String Subject { get; set; }

        /// <summary>
        /// Message text of the news object.
        /// </summary>
        public String Text { get; set; }
    }
}