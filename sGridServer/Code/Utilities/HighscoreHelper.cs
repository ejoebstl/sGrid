using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides hishscore list and highscore placings.
    /// </summary>
    public class HighscoreHelper
    {
        /// <summary>
        /// Creates and return an ordered enumeration of the users and the
        /// packages they calculated during a certain time.
        /// </summary>
        /// <param name="fromDate">The packages have to be
        /// calculated after this date.</param>
        /// <param name="toDate">The packages have to be
        /// calculated before this date.</param>
        /// <returns>An enumeration of Users, ordered by the number
        /// of packages thex calculated.</returns>
        public static IEnumerable<UserScore> GetHighscoreList(DateTime fromDate, DateTime toDate)
        {
            MemberManager manager = new MemberManager();

            //for each user a UserScore is created with the user himself and the number of packages he calculated
            return (from u in manager.Users
                    where u.ShowInHighScore
                    select new UserScore(u.CalculatedResults.Count(x => x.Valid && (fromDate < x.ServerReceivedTimestamp) && (x.ServerReceivedTimestamp < toDate)), u))
                .OrderByDescending(x => x.CalculatedPackages);
        }

        /// <summary>
        /// Creates and return an ordered enumeration of the users and the
        /// packages they calculated.
        /// </summary>
        /// <returns>An enumeration of Users, ordered by the number
        /// of packages thex calculated.</returns>
        public static IEnumerable<UserScore> GetHighscoreList()
        {
            return GetHighscoreList(DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Creates and return an ordered enumeration of the user's
        /// friends and the packages they calculated.
        /// </summary>
        /// <returns>An enumeration of the user and his friends,
        /// ordered by the number of packages they calculated.</returns>
        public static IEnumerable<UserScore> GetFriendsHighscoreList(User user)
        {
            MemberManager manager = new MemberManager();

            //create a list with the user and his friends
            List<User> oneUserList = new List<User>();
            oneUserList.Add(user);
            IEnumerable<User> userAndFriends = manager.GetFriends(user).Where(x => x.ShowInHighScore).Union(oneUserList);

            //for each user a UserScore is created with the user himself and the number of packages he calculated
            return (from u in userAndFriends
                    where u.Id==user.Id
                    select new UserScore(u.CalculatedResults.Count(x => x.Valid), u))
                .OrderByDescending(x => x.CalculatedPackages);
        }

        /// <summary>
        /// Return the place a user has in a specific Highscore.
        /// </summary>
        /// <param name="user">The user to rank.</param>
        /// <param name="fromDate">The packages have to be
        /// calculated after this date.</param>
        /// <param name="toDate">The packages have to be
        /// calculated before this date.</param>
        /// <param name="onlyFriends">Indicates that the user is
        /// only compared to his users.</param>
        /// <returns>The placement of the user.</returns>
        public static int GetPlacementInHighscore(User user, DateTime fromDate, DateTime toDate, bool onlyFriends = false)
        {
            MemberManager manager = new MemberManager();
            int calcRes = user.CalculatedResults.Count(x => x.Valid && (fromDate < x.ServerReceivedTimestamp) && (x.ServerReceivedTimestamp < toDate));

            //gets the users to compare with
            IEnumerable<User> otherUsers;
            if (onlyFriends)
            {
                otherUsers = manager.GetFriends(user);
            }
            else
            {
                otherUsers = manager.Users;
            }

            //counts the users who have calculated more than the given user
            return otherUsers.Count(u => u.ShowInHighScore
                && (u.CalculatedResults.Count(x => x.Valid && (fromDate < x.ServerReceivedTimestamp) && (x.ServerReceivedTimestamp < toDate)) > calcRes)) + 1;
        }

        /// <summary>
        /// Return the place a user has in a specific Highscore.
        /// </summary>
        /// <param name="user">The user to rank.</param>
        /// <param name="onlyFriends">Indicates that the user is
        /// only compared to his users.</param>
        /// <returns>The placement of the user.</returns>
        public static int GetPlacementInHighscore(User user, bool onlyFriends = false)
        {
            return GetPlacementInHighscore(user, DateTime.MinValue, DateTime.MaxValue, onlyFriends);
        }
    }
}
