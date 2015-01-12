using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents an error.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Gets or sets the id of the error.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the description of the error.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the stack trace of the error.
        /// </summary>
        public String Stacktrace { get; set; }

        /// <summary>
        /// Gets or sets a timestamp for the error.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }


        /// <summary>
        /// A default constructor of the error.
        /// </summary>
        public Error()
        {
            this.Description = "";
            this.Stacktrace = "";
            this.Timestamp = DateTime.Now;
        }
    }
}