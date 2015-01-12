using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// This class wraps a user and his current permissions for quick access. 
    /// </summary>
    [Serializable]
    public class UserContext
    {
        /// <summary>
        /// The id of the user this context belongs to.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The name of the user this context belongs to.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The permissions of the user this context belongs to.
        /// </summary>
        public SiteRoles UserPermissions { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and copies the given parameters.
        /// </summary>
        /// <param name="id">The id of the user this context belongs to.</param>
        /// <param name="name">The name of the user this context belongs to.</param>
        /// <param name="userPermission">The permissions of the user this context belongs to.</param>
        public UserContext(int id, string name, SiteRoles userPermission)
        {
            this.ID = id;
            this.Name = name;
            this.UserPermissions = userPermission;
        }

        /// <summary>
        /// Creates a new instance of this class and copies the given parameters.
        /// </summary>
        /// <param name="user">The account object to gather id, name and permissions from.</param>
        public UserContext(Account user)
            : this(user.Id, user.Nickname, (SiteRoles)user.UserPermission)
        {

        }
    }
}