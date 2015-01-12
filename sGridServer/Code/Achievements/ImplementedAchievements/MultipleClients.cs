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
    /// In order to achieve this, the user must have a certain
    /// number of clients running.
    /// </summary>
    public class MultipleClients : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfClients.
        /// </summary>
        protected const string NumberOfClientsName = "NumberOfClients";

        /// <summary>
        /// Gets or sets the number of clients the user has to run.
        /// </summary>
        public int NumberOfClients { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public MultipleClients()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfClientsName);
            PropertyNames = list;

            AchievementType = "MultipleClients";
            NumberOfClients = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (NumberOfClients < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.NumberOfClients);
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfClientsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfClients.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfClients = Int32.Parse(nvc[NumberOfClientsName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            GridProviderManager manager = new GridProviderManager(user);
            GridProviders.GridProvider provider = manager.CurrentProject.Provider.CreateProvider();

            return (provider.Results.Where(x => x.UserId == user.Id).Select(x => x.DeviceIdentifyer).Distinct().Count() >= NumberOfClients);
        }
    }
}