using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;

namespace sGridServerTests.Utilities
{
    /// <summary>
    /// Test the message manager
    /// </summary>
    [TestClass]
    public class MessageManagerTest : SGridServerTest
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
        /// Tests whether message sending works. 
        /// </summary>
        [TestMethod]
        public void TestMessages()
        {
            Message message = new Message()
            {
                EMail = "emi@eex-dev.net",
                MessageText = "Lorem Ipsum Döhner sit armin.",
                Resolved = false,
                Subject = "my message"
            };

            MessageManager manager = new MessageManager();

            manager.AddMessage(message);

            int id = message.Id;

            Message message2 = manager.GetMessages().Where(x => x.Id == id).First();

            Assert.AreEqual(message.EMail, message2.EMail);
            Assert.AreEqual(message.MessageText, message2.MessageText);
            Assert.IsFalse(message2.Resolved);
            Assert.AreEqual(message.Subject, message2.Subject);

            manager.MarkMessageAsResolved(message2);

            message2 = manager.GetMessages().Where(x => x.Id == id).First();

            Assert.IsTrue(message2.Resolved);
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
