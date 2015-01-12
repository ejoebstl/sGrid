using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// The GrantParameters class provides a method to modify the 
    /// amount of granted coins for a project, for instance with bonuses. 
    /// </summary>
    public static class GrantParameters
    {
        /// <summary>
        /// Is called by all GridProvider instances to apply modifications on the 
        /// calculated amount of coins to grant. Currently, the only modification 
        /// is a bonus multiplier based on the number of friends, which participate on
        /// the same project. 
        /// </summary>
        /// <param name="u">The user who this grant is for.</param>
        /// <param name="grant">The amount of coins the caller plans to grant.</param>
        /// <param name="project">The project for which the user has done a result.</param>
        /// <returns>The modified grant amount.</returns>
        public static int ModifyGrant(User u, int grant, GridProjectDescription project)
        {
            MemberManager manager = new MemberManager();

            //Get the count of friends which are working on the same project.
            int count = manager.GetFriends(u).Where(n => n.AttachedProjects.Where(x => x.Current && x.ShortName == project.ShortName).Any()).Count();

            //Calculate the multiplier. Multiplier is always from interval [1, 2). For x = 10, multiplier is 1,5. 
            float multiplier = 2 - (1 / ((count / 10) + 1));

            return (int)(grant * multiplier);
        }
    }
}