using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer;
using System.Transactions;

namespace sGridServerTests.Security
{
    /// <summary>
    /// Tests the MemberManager class
    /// </summary>
    [TestClass]
    public class MemberManagerTests : SGridServerTest
    {
        /// <summary>
        /// Sets up some accounts for testing
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Tests whether password hashing works correctly. 
        /// </summary>
        [TestMethod]
        public void TestPasswordHashing()
        {
            const string password = "this 1s A SeCurE p@$$w0rd";
            const string falsePassword1 = "this is A SeCurE p@$$w0rd";
            const string falsePassword2 = "this is not my p@$$w0rd";

            MemberManager manager = new MemberManager();

            User testUser = TestUserA;

            testUser.IdType = sGridServer.Controllers.EMailIdProviderController.ProviderIdentifier;

            manager.SaveAccount(testUser);

            TestUserA = testUser;
            
            manager.SetPassword(testUser, password);

            Assert.IsTrue(manager.ValidatePassword(testUser, password));
            Assert.IsFalse(manager.ValidatePassword(testUser, falsePassword1));
            Assert.IsFalse(manager.ValidatePassword(testUser, falsePassword2));
        }

        /// <summary>
        /// Tests whether entities from the member manager are correctly detached. 
        /// </summary>
        [TestMethod]
        public void TestDetachedEntities()
        {
            const string someUrl = "some url";
            const string somePassword = "some password";

            MemberManager manager = new MemberManager();
            User testA = manager.Users.Where(u => u.Id == TestUserA.Id).First();
            User testB = manager.Users.Where(u => u.Id == TestUserB.Id).First();
            User testC = manager.Users.Where(u => u.Id == TestUserC.Id).First();
            User testD = manager.Users.Where(u => u.Id == TestUserD.Id).First();

            testA.Picture = someUrl;
            testB.Picture = someUrl;
            testC.Picture = someUrl;
            testD.Picture = someUrl;

            testA.IdType = sGridServer.Controllers.EMailIdProviderController.ProviderIdentifier;

            manager.SaveAccount(testA);
            manager.DeactivateAccount(testB);
            manager.ActivateAccount(testC);

            testA = manager.Users.Where(u => u.Id == TestUserA.Id).First();
            testB = manager.Users.Where(u => u.Id == TestUserB.Id).First();
            testC = manager.Users.Where(u => u.Id == TestUserC.Id).First();
            testD = manager.Users.Where(u => u.Id == TestUserD.Id).First();

            Assert.AreEqual(testA.Picture, someUrl);
            Assert.AreNotEqual(testB.Picture, someUrl);
            Assert.AreNotEqual(testC.Picture, someUrl);
            Assert.AreNotEqual(testD.Picture, someUrl);

            Assert.IsFalse(testB.Active);
            Assert.IsTrue(testC.Active);

            testA.Picture = "";
            manager.SetPassword(testA, somePassword);

            testA = manager.Users.Where(u => u.Id == TestUserA.Id).First();

            Assert.AreEqual(testA.Picture, someUrl);
            Assert.IsTrue(manager.ValidatePassword(testA, somePassword));
        }

        /// <summary>
        /// Tests whether all events of MemberManager are raised correctly.
        /// </summary>
        [TestMethod]
        public void TestEvents()
        {
            MemberManager manager = new MemberManager();

            bool eventRised = false;

            MemberManager.UserCreated += delegate(object sender, UserCreadedEventArgs args)
            {
                eventRised = true;
            };
            MemberManager.FriendshipAdded += delegate(object sender, FriendshipAddedEventArgs args)
            {
                eventRised = true;
            };

            User a = manager.CreateUser(new User() { Nickname = "A_569107_test", EMail = "A_569107_test@sgrid.ipd.kit.edu" });
            Assert.IsTrue(eventRised, "The user created event was not rised.");

            eventRised = false;

            User b = manager.CreateUser(new User() { Nickname = "B_569107_test", EMail = "B_569107_test@sgrid.ipd.kit.edu" });
            Assert.IsTrue(eventRised, "The user created event was not rised.");

            eventRised = false;

            manager.RegisterFriendship(a, b);
            Assert.IsTrue(eventRised, "The friendship added event was not rised.");

            manager.RemoveFriendship(a, b); 
        }

        /// <summary>
        /// Tests whether friendship adding and removing works
        /// </summary>
        [TestMethod]
        public void TestFriendships()
        {
            MemberManager manager = new MemberManager();
            User testA = manager.Users.Where(u => u.Id == TestUserA.Id).First();
            User testB = manager.Users.Where(u => u.Id == TestUserB.Id).First();

            manager.AddFriendRequest(testA, testB);

            Assert.IsFalse(manager.AreFriends(testA, testB));
            Assert.IsFalse(manager.AreFriends(testB, testA));
            Assert.IsTrue(manager.GetFriendRequests(testB).Count() == 1);

            manager.RegisterFriendship(testA, testB);

            Assert.IsTrue(manager.GetFriendRequests(testB).Count() == 0);
            Assert.IsTrue(manager.AreFriends(testA, testB));
            Assert.IsTrue(manager.AreFriends(testB, testA));

            manager.RemoveFriendship(testB, testA);

            Assert.IsFalse(manager.AreFriends(testA, testB));
            Assert.IsFalse(manager.AreFriends(testB, testA));
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
