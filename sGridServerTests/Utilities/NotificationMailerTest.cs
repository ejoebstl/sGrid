using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.GridProviders;
using sGridServer.Code.Utilities;
using sGridServer.Code.GridProviders.WorldCommunityGrid;

namespace sGridServerTests.Utilities
{
    /// <summary>
    /// Tests the notification mailer class. 
    /// Notice: This test will fail without an appropriate e-mail server installation. 
    /// </summary>
    [TestClass]
    public class NotificationMailerTest : SGridServerTest
    {
         /// <summary>
        /// Sets up some accounts for testing.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(NotificationMailer).TypeHandle);
        }

        /// <summary>
        /// Tets whether an e-mail gets correctly sent. 
        /// <remarks>This test is ignored since it needs a working mailserver to function properly.</remarks>
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestProjectChangedNotification()
        {
            User u = new MemberManager().CreateUser(new User() {
                Nickname = "Emi",
                EMail = "emi@eex-dev.net",
                NotifyOnProjectChanged = true
            });

            GridProviderManager.RegisterProvider(new GridProviderDescription("wcg", "World Community Grid", "","", "", "", "", new GridProjectDescription[] {
                new GridProjectDescription("asdf", "Fight Cancer at Home", "Fight Cancer at Home", "", "" , "" , "", 0, 0)
            }, typeof(MockWorldCommunityGridProvider)));

            GridProviderManager providerManager = new GridProviderManager(u);

            providerManager.AttachToProject(GridProviderManager.ProjectForName("asdf"));
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
