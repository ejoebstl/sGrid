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
    /// In order to achieve this, the user has to get a certain
    /// number of achievements.
    /// </summary>
    public class AchievementsObtained : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfAchievements.
        /// </summary>
        protected const string NumberOfAchievementsName = "NumberOfAchievements";

        /// <summary>
        /// Gets or sets the number of achievements that have to be
        /// obtained.
        /// </summary>
        public int NumberOfAchievements { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public AchievementsObtained()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfAchievementsName);
            PropertyNames = list;

            AchievementType = "AchievementsObtained";
            NumberOfAchievements = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (NumberOfAchievements < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.NumberOfAchievements);
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfAchievementsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfAchievements.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfAchievements = Int32.Parse(nvc[NumberOfAchievementsName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            AchievementManager manager = new AchievementManager();

            return (manager.GetAchievements(user).Count() >= NumberOfAchievements);
        }
    }
}