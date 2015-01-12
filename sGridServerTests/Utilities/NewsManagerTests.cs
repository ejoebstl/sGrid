using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Utilities;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServerTests.Utilities
{
    /// <summary>
    /// Tests the news manager class.
    /// </summary>
    [TestClass]
    public class NewsManagerTests : SGridServerTest 
    {  
        /// <summary>
        /// Prepares the database.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Tests whether news management works. 
        /// </summary>
        [TestMethod]
        public void TestNews()
        {
            News n = new News()
            {
                Image = "",
                Subject = "TestNews",
                Text = "TestText"
            };

            NewsManager manager = new NewsManager();

            manager.SaveNews(n);

            int id = n.Id;

            News latest = manager.GetLatestNews();

            Assert.AreEqual(id, latest.Id);
            Assert.AreEqual((string)n.Subject, (string)latest.Subject);
            Assert.AreEqual((string)n.Text, (string)latest.Text);

            manager.DeleteNews(latest);

            latest = manager.GetLatestNews();

            if (latest != null && latest.Id == id)
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// Cleans up the database.
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
