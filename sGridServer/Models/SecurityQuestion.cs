using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    /// <summary>
    /// A model class which represents a security question. 
    /// </summary>
    public class SecurityQuestion
    {
        /// <summary>
        /// The answer given to the security question.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// The security question to show.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public SecurityQuestion()
        {
            Question = "";
            Answer = "";
        }
    }
}