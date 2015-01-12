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
    /// In order to achieve this, the user has to obtain a certain
    /// amount of coins on his coin account.
    /// </summary>
    public class CoinAccountBalance : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property AccountBalance.
        /// </summary>
        protected const string AccountBalanceName = "AccountBalance";

        /// <summary>
        /// Gets or sets the minimum number of coins that must be
        /// on the coin account.
        /// </summary>
        public int AccountBalance { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public CoinAccountBalance()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(AccountBalanceName);
            PropertyNames = list;

            AchievementType = "CoinAccountBalance";
            AccountBalance = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (AccountBalance < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.AccountBalance);
                return false;
            }
            StringBuilder sb = new StringBuilder(AccountBalanceName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(AccountBalance.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            AccountBalance = Int32.Parse(nvc[AccountBalanceName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            return (user.CoinAccount.CurrentBalance >= AccountBalance);
        }
    }
}