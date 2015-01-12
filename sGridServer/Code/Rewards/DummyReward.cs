using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Code.Rewards
{
    /// <summary>
    /// This is a test class, in order to test rewards.
    /// </summary>
    public class DummyReward : RewardItem
    {
        /// <summary>
        /// Creates an instance of this class and initializes the
        /// properties.
        /// </summary>
        public DummyReward()
        {
            BasicInitialization();

            List<string> list = new List<string>();
            list.Add(AmountName);
            list.Add(BeginName);
            list.Add(CostName);
            list.Add(EndName);
            list.Add(URLName);
            PropertyNames = list;

            RewardType = "DummyReward";

            string s;
            CreateExtendedParametersString(out s);
        }

        /// <inheritdoc/>
        protected override bool CreateExtendedParametersString(out string errorMessage)
        {
            //no extended parameters
            this.ExtendedParameters = "";
            errorMessage = "";
            return true;
        }

        /// <inheritdoc/>
        protected override void CreateParametersFromString()
        {
            //no extended parameters
        }

        /// <inheritdoc/>
        public override string ObtainReward(DataAccessLayer.Models.Purchase purchase)
        {
            return URL;
        }
    }
}