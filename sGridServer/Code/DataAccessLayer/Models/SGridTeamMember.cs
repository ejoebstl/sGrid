using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents an sGrid team member. 
    /// </summary>
    public class SGridTeamMember : Account
    {
        /// <summary>
        /// A default constructor of the sGrid team member.
        /// </summary>
        public SGridTeamMember()
        {
            this.UserPermission = SiteRoles.Admin;
        }
    }
}