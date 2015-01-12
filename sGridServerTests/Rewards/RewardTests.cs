using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Rewards;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServerTests.Rewards
{
    [TestClass]
    public class RewardTests : SGridServerTest
    {
        /// <summary>
        /// Creates a dummy user for testing.
        /// <remarks>Cannot used transaction based rollback here, since we are using multi-threading.</remarks>
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        //Creates a standart reward (a dummy)
        private RewardItem SetStdItem(RewardManager man, string str)
        {
            RewardItem item = man.GetRewardFromType(man.GetAllRewardTypes().First());
            //ein DummyReward

            item.Amount = 3;
            item.Begin = DateTime.Now.AddDays(-1);
            item.CoinPartnerId = TestCoinPartnerA.Id;
            item.Cost = 10;
            item.Description = "blablub";
            item.ShortDescription = "b";
            item.End = DateTime.Now.AddDays(2);
            item.Name = str;
            item.URL = "www.test.de";

            return item;
        }

        /// <summary>
        /// Test if the creation of the rewards functions.
        /// </summary>
        [TestMethod]
        public void TestRewardCreation()
        {
            RewardManager man = new RewardManager();
            RewardItem item = SetStdItem(man, "cr1");

            string s;

            //test all the properties
            item.Amount = -1;
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.Amount = 2;

            item.Begin = DateTime.Now.AddDays(3);
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.Begin = DateTime.Now.AddDays(-1);

            item.CoinPartnerId = 0;
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.CoinPartnerId = TestCoinPartnerA.Id;

            item.Cost = 0;
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.Cost = 10;

            item.End = DateTime.Now.AddDays(-2);
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.End = DateTime.Now.AddDays(2);

            item.URL = "free%&/())/&§$%&%";
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.URL = "www.test.de";

            //create rew
            item.PrepareForSaving(out s);
            Assert.IsTrue((s == null) || (s == ""));
            int a = man.GetAllExistingRewards().Count();
            man.CreateReward(item);
            Assert.AreEqual(a + 1, man.GetAllExistingRewards().Count());

            Reward rew = man.GetAllExistingRewards().Where(x => x.Name.Text == item.Name.Text).Single();
            Assert.AreEqual(rew.Amount, item.Amount);
            Assert.IsTrue(rew.Begin - item.Begin < new TimeSpan(12, 0, 0));
            Assert.AreEqual(rew.CoinPartnerId, item.CoinPartnerId);
            Assert.AreEqual(rew.Cost, item.Cost);
            Assert.IsTrue(rew.End - item.End < new TimeSpan(12, 0, 0));
            Assert.AreEqual(rew.Description.Text, item.Description.Text);
            Assert.AreEqual(rew.ShortDescription.Text, item.ShortDescription.Text);
            Assert.AreEqual(rew.URL, item.URL);
        }
        
        /// <summary>
        /// Test if the rewards are created with an approved boolean
        /// set as false.
        /// </summary>
        [TestMethod]
        public void TestApprovalAtRewards()
        {
            RewardManager man = new RewardManager();
            RewardItem i1 = SetStdItem(man, "ap1");
            i1.Approved = true;
            RewardItem i2 = SetStdItem(man, "ap2");
            i2.Approved = false;

            string s;

            Assert.IsTrue(i1.PrepareForSaving(out s));
            man.CreateReward(i1);
            Assert.IsTrue(i2.PrepareForSaving(out s));
            man.CreateReward(i2);

            Reward rew1 = man.GetAllExistingRewards().Where(x => x.Name.Text == i1.Name.Text).Single();
            Reward rew2 = man.GetAllExistingRewards().Where(x => x.Name.Text == i2.Name.Text).Single();
            Assert.IsFalse(rew1.Approved);
            Assert.IsFalse(rew2.Approved);

            man.ApproveReward(rew1);
            rew1 = man.GetAllExistingRewards().Where(x => x.Name.Text == i1.Name.Text).Single();
            rew2 = man.GetAllExistingRewards().Where(x => x.Name.Text == i2.Name.Text).Single();
            Assert.IsTrue(rew1.Approved);
            Assert.IsFalse(rew2.Approved);

            man.ApproveReward(rew2);
            rew1 = man.GetAllExistingRewards().Where(x => x.Name.Text == i1.Name.Text).Single();
            rew2 = man.GetAllExistingRewards().Where(x => x.Name.Text == i2.Name.Text).Single();
            Assert.IsTrue(rew1.Approved);
            Assert.IsTrue(rew2.Approved);
        }

        /// <summary>
        /// Test if the rewards can be bought.
        /// </summary>
        [TestMethod]
        public void TestRewardPurchase()
        {
            RewardManager man = new RewardManager();
            RewardItem item = SetStdItem(man, "pu1");
            string s;
            item.PrepareForSaving(out s);
            man.CreateReward(item);
            Reward rew = man.GetAllExistingRewards().Where(x => x.Name.Text == item.Name.Text).Single();
            man.ApproveReward(rew);

            sGridServer.Code.CoinExchange.CoinExchange ce = new sGridServer.Code.CoinExchange.CoinExchange(TestUserA);
            Achievement ach = new Achievement();
            ach.Name = "";
            ce.Grant(ach, 20);//achievement only needed for alerting the user
            sGridServer.Code.CoinExchange.ICoinTransaction tr = ce.BeginBuy(rew);
            tr.EndBuy();
        }

        /// <summary>
        /// Cleans up the dummy user and it's transactions. 
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
