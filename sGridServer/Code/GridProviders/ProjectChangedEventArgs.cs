using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// This class holds arguments for the 
    /// GridProviderManager.ProjectChanged event.
    /// </summary>
    public class ProjectChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a bool indicating whether the user attached to or detached from the project. 
        /// </summary>
        public bool IsAttach { get; private set; }

        /// <summary>
        /// Gets the project the user detached from or attached to. 
        /// </summary>
        public GridProjectDescription Project { get; private set; }

        /// <summary>
        /// Gets the user who attached or detached to a project.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Creates a new instance of this class using the given parameters.
        /// </summary>
        /// <param name="user">The user who attached or detached to a project.</param>
        /// <param name="project">The project the user detached from or attached to. </param>
        /// <param name="isAttach">A bool indicating whether the user attached to or detached from the project. </param>
        public ProjectChangedEventArgs(User user, GridProjectDescription project, bool isAttach)
        {
            this.User = user;
            this.Project = project;
            this.IsAttach = isAttach;
        }
    }
}