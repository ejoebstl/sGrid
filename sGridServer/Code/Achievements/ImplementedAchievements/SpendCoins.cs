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
    /// In order to achieve this, the user has to spend a certain
    /// amount of coins on rewards.
    /// </summary>
    public class SpendCoins : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property AmountOfCoins.
        /// </summary>
        protected const string AmountOfCoinsName = "AmountOfCoins";

        /// <summary>
        /// Gets or sets the number of coins the user has to spend.
        /// </summary>
        public int AmountOfCoins { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public SpendCoins()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(AmountOfCoinsName);
            PropertyNames = list;

            AchievementType = "SpendCoins";
            AmountOfCoins = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc />
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (AmountOfCoins < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.AmountOfCoins);
                return false;
            }
            StringBuilder sb = new StringBuilder(AmountOfCoinsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(AmountOfCoins.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc />
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            AmountOfCoins = Int32.Parse(nvc[AmountOfCoinsName]);
        }

        /// <inheritdoc />
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            return (user.CoinAccount.TotalSpent >= AmountOfCoins);
        }
    }
}