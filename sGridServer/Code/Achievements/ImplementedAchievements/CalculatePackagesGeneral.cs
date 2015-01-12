using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.GridProviders;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to calculate a
    /// specific number of packages correctly.
    /// </summary>
    public class CalculatePackagesGeneral : AchievementItem
    { 
        /// <summary>
        /// This is the string identifying the property NumberOfPackages.
        /// </summary>
        protected const string NumberOfPackagesName = "NumberOfPackages";

        /// <summary>
        /// Gets or sets the number of packages the user has to
        /// calculate to get the achievement.
        /// </summary>
        public int NumberOfPackages { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public CalculatePackagesGeneral()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfPackagesName);
            PropertyNames = list;

            AchievementType = "CalculatedPackagesGeneral";
            NumberOfPackages = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (NumberOfPackages < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.NumberOfPackages);
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfPackagesName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfPackages.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfPackages = Int32.Parse(nvc[NumberOfPackagesName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            GridProviderManager manager = new GridProviderManager(user);

            return (manager.CurrentSummary.ResultCount >= NumberOfPackages);
        }
    }
}