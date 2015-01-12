using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a single translated string.
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the text value. 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the culture code associated with this translation.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the MultiLanguageString object associated with this translation.
        /// </summary>
        public virtual MultiLanguageString LanguageString { get; set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public Translation()
        {
            Id = -1;
            Text = "";
            Culture = "en-US";
        }
    }
}