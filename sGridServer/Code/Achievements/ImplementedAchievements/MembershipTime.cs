using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to be a member of
    /// sGrid for a certain time span.
    /// </summary>
    public class MembershipTime : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property ActiveTime.
        /// </summary>
        protected const string ActiveTimeName = "ActiveTime";

        /// <summary>
        /// Gets or sets the time in days for which the user has
        /// to be a registered member.
        /// </summary>
        public int ActiveTime { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public MembershipTime()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(ActiveTimeName);
            PropertyNames = list;

            AchievementType = "MembershipTime";
            ActiveTime = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (ActiveTime < 0)
            {
                //if active time is 0, then the achievement is
                //distributed for joining sGrid
                errorMessage = String.Format(Resource.HasToBeNonnegative, Resource.ActiveTime);
                return false;
            }
            StringBuilder sb = new StringBuilder(ActiveTimeName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(ActiveTime.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            ActiveTime = Int32.Parse(nvc[ActiveTimeName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            return (DateTime.Now - new TimeSpan(ActiveTime, 0, 0, 0) < user.RegistrationDate);
        }
    }
}