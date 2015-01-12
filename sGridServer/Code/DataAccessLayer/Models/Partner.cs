using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a partner.
    /// </summary>
    public class Partner : Account
    {
        /// <summary>
        /// Gets or sets the description of the partner.
        /// </summary>
        public virtual MultiLanguageString Description { get; set; }

        /// <summary>
        /// Gets or sets the link to the website of the partner.
        /// </summary>
        [DataType(DataType.Url)]
        public String Link { get; set; }

        /// <summary>
        /// Gets or sets the logo url of the partner’s logo.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Logo { get; set; }


        /// <summary>
        /// A default constructor of the partner.
        /// </summary>
        public Partner()
        {
            //this.Description = new MultiLanguageString();
            this.Link = "";
            this.Logo = "";
        }
    }
}