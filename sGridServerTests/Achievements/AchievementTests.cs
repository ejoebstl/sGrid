using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Achievements;
using sGridServer.Code.Achievements.ImplementedAchievements;

namespace sGridServerTests.Achievements
{
    [TestClass]
    [Ignore]
    public class AchievementTests : SGridServerTest
    {
        /// <summary>
        /// Sets up some accounts for testing.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        //Creates a standart Achievement
        private CoinsEarned SetStdItem(AchievementManager man, string str, int x)
        {
            CoinsEarned item = (CoinsEarned)man.GetAchievementFromType("CoinsEarned");

            item.AmountOfCoins = x;
            item.BonusCoins = 10;
            item.Description = "blib";
            item.Name = str;

            return item;
        }

        /// <summary>
        /// Tests, if achievements can be created.
        /// </summary>
        [TestMethod]
        public void TestBasicAchievementCreation()
        {
            AchievementManager man = new AchievementManager();
            CoinsEarned item = SetStdItem(man, "ba1", 30);

            string s;

            item.AmountOfCoins = 0;
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.AmountOfCoins = 30;

            item.BonusCoins = -1;
            Assert.IsFalse(item.PrepareForSaving(out s));
            Assert.IsTrue((s != "") && (s != null));
            item.BonusCoins = 10;

            //create rew
            item.PrepareForSaving(out s);
            Assert.IsTrue((s == null) || (s == ""));
            Assert.IsFalse((item.ExtendedParameters == null) || (item.ExtendedParameters == ""));
            int a = man.GetAllExistingAchievements().Count();
            man.CreateAchievement(item);
            Assert.AreEqual(a + 1, man.GetAllExistingAchievements().Count());

            Achievement ach = man.GetAllExistingAchievements().Where(x => x.Name.Text == item.Name.Text).Single();
            Assert.AreEqual(ach.BonusCoins, item.BonusCoins);
            Assert.AreEqual(ach.Description.Text, item.Description.Text);
            Assert.AreEqual(ach.ExtendedParameters, item.ExtendedParameters);
            Assert.AreEqual(ach.Name.Text, item.Name.Text);
        }

        /// <summary>
        /// Test, if the active type can be set.
        /// </summary>
        [TestMethod]
        public void TestAchievementsActive()
        {
            AchievementManager man = new AchievementManager();
            string s;

            CoinsEarned i1 = SetStdItem(man, "ac1", 31);
            i1.Active = true;
            Assert.IsTrue(i1.PrepareForSaving(out s));
            man.CreateAchievement(i1);

            CoinsEarned i2 = SetStdItem(man, "ac2", 32);
            i2.Active = false;
            Assert.IsTrue(i2.PrepareForSaving(out s));
            man.CreateAchievement(i2);

            Achievement ach1 = man.GetAllExistingAchievements().Where(x => x.Name.Text == i1.Name.Text).Single();
            Achievement ach2 = man.GetAllExistingAchievements().Where(x => x.Name.Text == i2.Name.Text).Single();
            Assert.IsTrue(ach1.Active);
            Assert.IsFalse(ach2.Active);
        }

        /// <summary>
        /// Tests, if the achievement can be obtained through events
        /// and tests if they can not be achieved twice
        /// </summary>
        [TestMethod]
        public void TestEventsWithAchievements()
        {
            AchievementManager man = new AchievementManager();
            string s;

            CoinsEarned i1 = SetStdItem(man, "ev1", 10);
            i1.Active = true;
            Assert.IsTrue(i1.PrepareForSaving(out s));
            man.CreateAchievement(i1);

            Assert.AreEqual(1, man.GetAchievements(TestUserA).Where(x => x.Name.Text == i1.Name.Text).Count());
            //he did not achieve it yet

            sGridServer.Code.CoinExchange.CoinExchange ce = new sGridServer.Code.CoinExchange.CoinExchange(TestUserA);
            Achievement achi = new Achievement();
            achi.Name = "";
            ce.Grant(achi, 10);//achievement only needed for alerting the user

            Assert.AreEqual(1, man.GetAchievements(TestUserA).Where(x => x.Name.Text == i1.Name.Text).Count());
            //-> he achieved it once

            ce.Grant(achi, 10);
            Assert.AreEqual(1, man.GetAchievements(TestUserA).Where(x => x.Name.Text == i1.Name.Text).Count());
            //-> he achieved it only once
        }

        /// <summary>
        /// Test, if you can obtain an unshown achievement and set it to shown
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestAchievementsUnshown()
        {
            AchievementManager man = new AchievementManager();
            string s;

            CoinsEarned i1 = SetStdItem(man, "ev1", 8);
            i1.Active = true;
            Assert.IsTrue(i1.PrepareForSaving(out s));
            man.CreateAchievement(i1);
            int id = man.GetAllExistingAchievements().Where(x => x.Name.Text == i1.Name.Text).Single().AchievementId;

            sGridServer.Code.CoinExchange.CoinExchange ce = new sGridServer.Code.CoinExchange.CoinExchange(TestUserB);
            Achievement achi = new Achievement();
            achi.Name = "";
            ce.Grant(achi, 4);
            ce.Grant(achi, 5);
            Assert.AreEqual(1, man.GetAchievements(TestUserB).Where(x => x.Name.Text == i1.Name.Text).Count());
            //the ach has been achieved


            //tests, if the achievement has not yet been shown
            //and tests, if an achievement can be set to shown
            bool dum3 = false;
            while (man.GetNextUnshownAchievement(TestUserB) != null)
            {
                Achievement ach= man.GetNextUnshownAchievement(TestUserB);
                Assert.AreEqual(ach.AchievementId, man.GetNextUnshownAchievement(TestUserB).AchievementId);
                if (ach.AchievementId == id)
                {
                    Assert.IsFalse(dum3);
                    dum3 = true;
                }

                man.SetAchievementShown(TestUserB, ach);
            }
            Assert.IsTrue(dum3);
        }

        /// <summary>
        /// Deletes the test accounts
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
