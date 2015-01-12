using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user has to spend a certain
    /// amount of coins during a given time span.
    /// </summary>
    public class SpendCoinsFast : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property AmountOfCoins.
        /// </summary>
        protected const string AmountOfCoinsName = "AmountOfCoins";

        /// <summary>
        /// This is the string identifying the property TimeOfShopping.
        /// </summary>
        protected const string TimeOfShoppingName = "TimeOfShopping";

        /// <summary>
        /// Gets or sets the number of coins the user has to spend.
        /// </summary>
        public int AmountOfCoins { get; set; }

        /// <summary>
        /// Gets or sets the time span in minutes during which the
        /// coins have to be spend.
        /// </summary>
        public int TimeOfShopping { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public SpendCoinsFast()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(AmountOfCoinsName);
            list.Add(TimeOfShoppingName);
            PropertyNames = list;

            AchievementType = "SpendCoinsFast";
            AmountOfCoins = 1;
            TimeOfShopping = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (AmountOfCoins < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.AmountOfCoins);
                return false;
            }
            if (TimeOfShopping < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.ActiveTime);
                return false;
            }
            StringBuilder sb = new StringBuilder(AmountOfCoinsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(AmountOfCoins.ToString()));
            sb.Append(AndSign);
            sb.Append(TimeOfShoppingName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(TimeOfShopping.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            AmountOfCoins = Int32.Parse(nvc[AmountOfCoinsName]);
            TimeOfShopping = Int32.Parse(nvc[TimeOfShoppingName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            DateTime limitTime = DateTime.Now - new TimeSpan(0, TimeOfShopping, 0);
            int reallySpendCoins = (from tr in CoinExchange.CoinExchange.GetTransactions(user)
                                    where tr.Timestamp >limitTime
                                    select tr.Value).Sum();
            return (reallySpendCoins >= AmountOfCoins);
        }
    }
}