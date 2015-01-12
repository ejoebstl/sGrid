using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using System.Text;
using System.Collections.Specialized;
using sGridServer.Code.Rewards;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements.ImplementedAchievements
{
    /// <summary>
    /// In order to achieve this, the user must buy a certain
    /// amount of rewards from a given CoinPartner.
    /// </summary>
    public class BuyRewardFromPartner : AchievementItem
    {
        /// <summary>
        /// This is the string identifying the property NumberOfRewards.
        /// </summary>
        protected const string NumberOfRewardsName = "NumberOfRewards";

        /// <summary>
        /// This is the string identifying the property PartnerId.
        /// </summary>
        protected const string PartnerIdName = "PartnerId";

        /// <summary>
        /// Gets or sets the number of rewards the user has to buy.
        /// </summary>
        public int NumberOfRewards { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the partner the rewards
        /// must belong to.
        /// </summary>
        public int PartnerId { get; set; }

        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public BuyRewardFromPartner()
        {
            BasicInitialization();

            List<String> list = new List<String>();
            list.Add(BonusCoinsName);
            list.Add(NumberOfRewardsName);
            list.Add(PartnerIdName);
            PropertyNames = list;

            AchievementType = "BuyRewardFromPartner";
            NumberOfRewards = 1;
            PartnerId = 0;

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
            MemberManager memMan = new MemberManager();
            if (!(from cp in memMan.CoinPartners where cp.Id == this.PartnerId select cp.Id).Any())
            {
                errorMessage = Resource.WrongPartnerIdError;
                return false;
            }
            StringBuilder sb = new StringBuilder(NumberOfRewardsName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(NumberOfRewards.ToString()));
            sb.Append(AndSign);
            sb.Append(PartnerIdName);
            sb.Append(EqualitySign);
            sb.Append(HttpUtility.UrlEncode(PartnerId.ToString()));
            ExtendedParameters = sb.ToString();

            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            NameValueCollection nvc = HttpUtility.ParseQueryString(ExtendedParameters);
            NumberOfRewards = Int32.Parse(nvc[NumberOfRewardsName]);
            PartnerId = Int32.Parse(nvc[PartnerIdName]);
        }

        /// <inheritdoc/>
        protected override bool ConditionsSatisfied(DataAccessLayer.Models.User user)
        {
            RewardManager rewMan = new RewardManager();
            int reallyPurchasedRewards = (from p in rewMan.GetPurchases()
                                          where (p.UserId == user.Id)
                                          && (p.Reward.CoinPartnerId == PartnerId)
                                          select p.Amount).Sum(x => x);

            return (reallyPurchasedRewards >= NumberOfRewards);
        }
    }
}