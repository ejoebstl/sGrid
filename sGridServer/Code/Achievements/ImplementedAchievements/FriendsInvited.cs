using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.Security;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to invite a certain
    /// number of friends.
    /// </summary>
    public class FriendsInvited : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfFriends.
        /// </summary>
        protected const string NumberOfFriendsName = "NumberOfFriends";

        /// <summary>
        /// Gets or sets the number of friends which have to be
        /// invited.
        /// </summary>
        public int NumberOfFriends { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public FriendsInvited()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfFriendsName);
            PropertyNames = list;

            AchievementType = "FriendsInvited";
            NumberOfFriends = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (NumberOfFriends < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.NumberOfFriends);
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfFriendsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfFriends.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfFriends = Int32.Parse(nvc[NumberOfFriendsName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            MemberManager manager = new MemberManager();

            return (manager.GetInvitedFriends(user).Count() >= NumberOfFriends);
        }
    }
}