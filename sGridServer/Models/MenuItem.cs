using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    /// <summary>
    /// This model class represents an item in a menu.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// The target property of the link as defined in the HTML 
        /// specification of the anchor (a) element.
        /// </summary>
        public string LinkTarget { get; set; }

        /// <summary>
        /// The link text. 
        /// </summary>
        public string LinkText { get; set; }

        /// <summary>
        /// The url of the hyperlink, or an empty string 
        /// if the menu item should not be rendered as link.
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public MenuItem()
        {
            LinkText = "";
            LinkTarget = "_self";
            LinkUrl = "";
        }
    }
}