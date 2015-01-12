using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.Web.SessionState;
using sGridServer.Code.DataAccessLayer;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// The static security provider class provides methods to login
    /// or logout a user and to get the context and permissions of the current user. 
    /// The user context is stored in the session. 
    /// </summary>
    public static class SecurityProvider
    {
        /// <summary>
        /// The key of the stored context object in the session. 
        /// </summary>
        private const string UserSessionKey = "sGridCurrentUserContext";

        /// <summary>
        /// Gets the context of the user who is currently logged in. 
        /// </summary>
        public static UserContext Context
        {
            get
            {
                return HttpContext.Current.Session[UserSessionKey] as UserContext;
            }
        }

        /// <summary>
        /// Gets the account of the user who is currently logged 
        /// in by fetching it from the database. 
        /// </summary>
        public static Account CurrentUser
        {
            get
            {
                if(Context == null)
                    return null;

                SGridDbContext context = new SGridDbContext();

                int id = Context.ID;

                return (from a in context.Accounts where a.Id == id select a).FirstOrDefault();

                //return new SGridDbContext().Accounts.Where(x => x.Id == Context.ID).FirstOrDefault(); - Emi du Homo
            }
        }

        /// <summary>
        /// Logs in the user with the given id.
        /// </summary>
        /// <param name="id">The id of the user to log in.</param>
        public static void LogIn(int id)
        {
            Account account = new SGridDbContext().Accounts.Where(x => x.Id == Context.ID).FirstOrDefault();

            LogIn(new UserContext(account));
        }

        /// <summary>
        /// Logs in the user associated with the given UserContext object.
        /// </summary>
        /// <param name="user">The context of the user to log in.</param>
        public static void LogIn(UserContext user)
        {
            HttpContext.Current.Session[UserSessionKey] = user;
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        public static void LogOut()
        {
            ClearUserContext();
            HttpContext.Current.Session.Abandon();
        }

        /// <summary>
        /// Clears the context of the current user.
        /// </summary>
        public static void ClearUserContext()
        {
            HttpContext.Current.Session[UserSessionKey] = null;
        }
    }
}