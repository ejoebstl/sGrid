using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace sGridServer.Models
{
    public class Example
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string From { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public string UserType { get; set; }

        public Example()
        {
            UserTypes = new SelectList(new[] { "Admin", "User" });
            CurrentItem = "User";
        }

        public string getExample()
        {
            return "User";
        }

        public SelectList UserTypes { get; set; }
        public string CurrentItem { get; set; }
    }
}