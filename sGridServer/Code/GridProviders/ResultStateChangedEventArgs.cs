using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// Represents event arguments for result state changes. 
    /// </summary>
    public class ResultStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the project for which the result statistics have changed. 
        /// </summary>
        public GridProjectDescription Project { get; private set; }

        /// <summary>
        /// Gets the user associated with the event.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets the result associated with the event.
        /// </summary>
        public CalculatedResult Result { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the 
        /// parameters into their corresponding properties.
        /// </summary>
        /// <param name="user">The user associated with the event.</param>
        /// <param name="project">The project for which the result statistics have changed. </param>
        /// <param name="result">The result associated with the event.</param>
        public ResultStateChangedEventArgs(User user, GridProjectDescription project, CalculatedResult result)
        {
            this.User = user;
            this.Project = project;
            this.Result = result;
        }
    }
}