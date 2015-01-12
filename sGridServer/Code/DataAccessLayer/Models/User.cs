using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This represents a user.
    /// </summary>
    public class User : Account
    {
        /// <summary>
        /// Gets or sets the list with the attached projects,
        /// which are projects a user is currently working on and has worked on.
        /// </summary>
        public virtual List<AttachedProject> AttachedProjects { get; set; }

        /// <summary>
        /// Gets or sets the list of the authentication information for gird providers.
        /// </summary>
        public virtual List<GridProviderAuthenticationData> DataForGridProvider { get; set; }

        /// <summary>
        /// Gets or sets a list of the calculated results by this user.
        /// </summary>
        public virtual List<CalculatedResult> CalculatedResults { get; set; }

        /// <summary>
        /// Gets or sets the list of the obtained achievements by this user.
        /// </summary>
        public virtual List<ObtainedAchievement> ObtainedAchievements { get; set; }

        /// <summary>
        /// Gets or sets the list of the reward ratings which was made by this user.
        /// </summary>
        public virtual List<Rating> Ratings { get; set; }

        /// <summary>
        /// Gets or sets the list of the purchases where this user had bought rewards.
        /// </summary>
        public virtual List<Purchase> Purchases { get; set; }


        /// <summary>
        /// A default constructor of the user.
        /// </summary>
        public User()
        {
            this.UserPermission = SiteRoles.User;
        }
    }
}