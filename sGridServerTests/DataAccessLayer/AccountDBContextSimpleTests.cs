using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.DataAccessLayer.Models;
using System.Transactions;

namespace sGridServerTests
{
    /// <summary>
    /// This tests should test whether the database is in a functional state.
    /// </summary>
    [TestClass]
    public class AccountDBContextSimpleTests : SGridServerTest
    {
        
        /// <summary>
        /// Starts a Transaction for Testing
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Tries to remove and create a user and a friendship. 
        /// </summary>
        [TestMethod]
        public void TestBasicUserFunctions()
        {
            SGridDbContext context = new SGridDbContext();

            User createTest = new User();

            createTest.Nickname = "Lumlum";

            createTest.CoinAccount = new CoinAccount();

            context.Accounts.Add(createTest);
            context.SaveChanges();

            Friendship friendship = new Friendship()
            {
                User = createTest,
                Friend = createTest,
                WasInvited = false
            };

            context.Friendships.Add(friendship);
            context.SaveChanges();

            context.Friendships.Remove(friendship);
            context.SaveChanges();

            Account loadTest = context.Accounts.Where(x => x.Id == createTest.Id).First();

            User castTest = (User)loadTest;

            context.Accounts.Remove(castTest);
            context.SaveChanges();

        }

        /// <summary>
        /// Tries to insert and to remove a news item. 
        /// </summary>
        [TestMethod]
        public void TestBasicNewsFunctions()
        {
            SGridDbContext context = new SGridDbContext();

            News n = new News();

            n.Text = "TestBla";

            context.News.Add(n);

            context.SaveChanges();

            News a = context.News.Where(x => x.Id == n.Id).First();

            context.News.Remove(a);
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
