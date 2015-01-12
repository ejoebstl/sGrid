using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to get to a certain
    /// position in a high score table.
    /// </summary>
    public class HighscorePlacing : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property MinimalLimit.
        /// </summary>
        protected const string MinimalLimitName = "MinimalLimit";

        /// <summary>
        /// This is the string identifying the property HighscorePlacement.
        /// </summary>
        protected const string HighscorePlacementName = "HighscorePlacement";

        /// <summary>
        /// Gets or sets the limit in days after which the results have to
        /// be send in in order to be used in the calculation of the
        /// highscore. 
        /// A value of 0 identifies the eternal highscore.
        /// </summary>
        public int MinimalLimit { get; set; }

        /// <summary>
        /// Gets or sets the minimum placement that has to be reached.
        /// </summary>
        public int HighscorePlacement { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public HighscorePlacing()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(MinimalLimitName);
            list.Add(HighscorePlacementName);
            PropertyNames = list;

            AchievementType = "HighscorePlacing";
            MinimalLimit = 0;
            HighscorePlacement = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (MinimalLimit < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.MinimalLimit);
                return false;
            }
            if (HighscorePlacement < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.HighscorePlacement);
                return false;
            }
            StringBuilder sb = new StringBuilder(MinimalLimitName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(MinimalLimit.ToString()));
            sb.Append(AndSign);
            sb.Append(HighscorePlacementName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(HighscorePlacement.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            MinimalLimit = Int32.Parse(nvc[MinimalLimitName]);
            HighscorePlacement = Int32.Parse(nvc[HighscorePlacementName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            if (!user.ShowInHighScore)
            {
                return false;
            }

            DateTime fromDate;
            if (MinimalLimit == 0)
            {
                fromDate = DateTime.MinValue;
            }
            else
            {
                fromDate = DateTime.Now - new TimeSpan(MinimalLimit, 0, 0, 0);
            }

            return (HighscoreHelper.GetPlacementInHighscore(user, fromDate, DateTime.MaxValue) <= HighscorePlacement);
        }
    }
}