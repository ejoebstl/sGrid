using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.Rewards;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user must buy a certain
    /// amount of rewards.
    /// </summary>
    public class BuyRewardsGeneral : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfRewards.
        /// </summary>
        protected const string NumberOfRewardsName = "NumberOfRewards";

        /// <summary>
        /// Gets or sets the number of rewards the user has to buy.
        /// </summary>
        public int NumberOfRewards { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public BuyRewardsGeneral()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfRewardsName);
            PropertyNames = list;

            AchievementType = "BuyRewardsGeneral";
            NumberOfRewards = 1;

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            if (NumberOfRewards < 1)
            {
                errorMessage = String.Format(Resource.HasToBePositive, Resource.NumberOfRewards);
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfRewardsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfRewards.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {   
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfRewards = Int32.Parse(nvc[NumberOfRewardsName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            RewardManager manager = new RewardManager();
            int reallyPurchasedRewards = (from p in manager.GetPurchases()
                                          where p.UserId == user.Id
                                          select p.Amount).Sum(x => x);

            return (reallyPurchasedRewards >= NumberOfRewards);
        }
    }
}