using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.CoinExchange;
using sGridServer.Code.GridProviders;
using System.Threading.Tasks;
using System.Threading;

namespace sGridServerTests.CoinExchange
{
    /// <summary>
    /// Tests the CoinExchange class.
    /// </summary>
    [TestClass]
    public class CoinExchangeTests : SGridServerTest 
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

        /// <summary>
        /// Tests the granting functions.
        /// </summary>
        [TestMethod]
        public void TestCoinExchangeGrant()
        {
            User user = base.TestUserA;

            int currentBalance = user.CoinAccount.CurrentBalance;

            sGridServer.Code.CoinExchange.CoinExchange coinExchange = new sGridServer.Code.CoinExchange.CoinExchange(user);

            Achievement testAchievement = new Achievement() { Name = "Testachievement", Description = "Testachievement" };
            GridProjectDescription gridProject = new GridProjectDescription("", "", "Testproject", "", "", "", "", 0, 0);

            coinExchange.Grant(testAchievement, 1);
            coinExchange.Grant(gridProject, 1);

            user = MemberManager.GetAccountById(user.Id) as User;

            Assert.IsTrue(user.CoinAccount.TotalGrant == currentBalance + 2);
            Assert.IsTrue(user.CoinAccount.CurrentBalance == currentBalance + 2);

            Assert.IsTrue(sGridServer.Code.CoinExchange.CoinExchange.GetTransactions(user).Where(x => x.Description.Contains(testAchievement.Name)).Any());
            Assert.IsTrue(sGridServer.Code.CoinExchange.CoinExchange.GetTransactions(user).Where(x => x.Description.Contains(gridProject.Name)).Any());
        }

        /// <summary>
        /// Tests the granting transactions for proper concurrency handling. 
        /// <remarks>
        /// This test will fail due to transactional issues - without transactions it would fill up the DB with meaningless data. 
        /// This method is marked as ignored since it will fail on SQL Server express edition (default for development). 
        /// </remarks>
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestCoinExchangeConcurrency()
        {
            User user = base.TestUserB;

            int currentBalance = user.CoinAccount.CurrentBalance;

            ThreadPool.SetMinThreads(30, 30);

            Parallel.For(0, 20, (int i) =>
            {
                sGridServer.Code.CoinExchange.CoinExchange coinExchange = new sGridServer.Code.CoinExchange.CoinExchange(user);
                coinExchange.Grant(new Achievement() { Name = "Testachievement" }, 1);
            });

            Assert.IsTrue(user.CoinAccount.TotalGrant == currentBalance + 100);
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
