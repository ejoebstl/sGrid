using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a news item. 
    /// </summary>
    public class News
    {
        /// <summary>
        /// Gets or sets the id of the news item.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the image of a news.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Image { get; set; }

        /// <summary>
        /// Gets or sets the subject of a news.
        /// </summary>
        public virtual MultiLanguageString Subject { get; set; }

        /// <summary>
        /// Gets or sets the text of a news.
        /// </summary>
        public virtual MultiLanguageString Text { get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the news was published.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// A default constructor of the news item.
        /// </summary>
        public News()
        {
            this.Image = "";
            //this.Subject = new MultiLanguageString();
            //this.Text = new MultiLanguageString();
            this.Timestamp = DateTime.Now;
            this.Id = -1;
        }
    }
}