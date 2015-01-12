using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using sGridServer.Code.GridProviders;
using System.Collections.Specialized;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to calculate a
    /// specific number of packages correctly for a given project.
    /// </summary>
    public class CalculatePackagesForProject :AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfPackages.
        /// </summary>
        protected const string NumberOfPackagesName = "NumberOfPackages";

        /// <summary>
        /// This is the string identifying the property ProjectShortName.
        /// </summary>
        protected const string ProjectShortNameName = "ProjectShortName";

        /// <summary>
        /// Gets or sets the number of packages the user has to
        /// calculate to get the achievement.
        /// </summary>
        public int NumberOfPackages { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Project the user has
        /// to calculate packages for.
        /// </summary>
        public String ProjectShortName { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public CalculatePackagesForProject()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfPackagesName);
            list.Add(ProjectShortNameName);
            PropertyNames = list;

            AchievementType = "CalculatePackagesForProject";
            NumberOfPackages = 1;
            ProjectShortName = "";

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
            if (GridProviderManager.ProjectForName(ProjectShortName) == null)
            {
                errorMessage = Resource.WrongProjectError;
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfPackagesName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfPackages.ToString()));
            sb.Append(AndSign);
            sb.Append(ProjectShortNameName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(ProjectShortName));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfPackages = Int32.Parse(nvc[NumberOfPackagesName]);
            ProjectShortName = nvc[ProjectShortNameName];
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            GridProviderManager manager = new GridProviderManager(user);
            
            return (manager.CurrentSummary.ResultCount >= NumberOfPackages);
        }
    }
}