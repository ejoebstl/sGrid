using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.GridProviders;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to repeatedly send
    /// in the result of a package faster than the average answer
    /// time.
    /// </summary>
    public class FastCalculator : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property CountOfFastCalculations.
        /// </summary>
        protected const string CountOfFastCalculationsName = "CountOfFastCalculations";

        /// <summary>
        /// Gets or sets the number of times the user has to send
        /// in the answer faster than the average.
        /// </summary>
        public int CountOfFastCalculations { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public FastCalculator()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(CountOfFastCalculationsName);
            PropertyNames = list;

            AchievementType = "FastCalculator";
            CountOfFastCalculations = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (CountOfFastCalculations < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.CountOfFastCalculations);
                return false;
            }
            StringBuilder sb = new StringBuilder(CountOfFastCalculationsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(CountOfFastCalculations.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            CountOfFastCalculations = Int32.Parse(nvc[CountOfFastCalculationsName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            GridProviderManager manager = new GridProviderManager(user);
            int averageCalcTime = manager.CurrentProject.AverageCalculationTime;
            int reallyFastCalculations = (from result in GridProviders.GridProvider.GetResults(user, manager.CurrentProject)
                                          where (result.ServerReceivedTimestamp - result.ServerSentTimestamp).TotalMinutes <= averageCalcTime
                                          select result).Count();

            return (reallyFastCalculations >= CountOfFastCalculations);
        }
    }
}