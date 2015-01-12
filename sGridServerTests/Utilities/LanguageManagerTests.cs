using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Utilities;

namespace sGridServerTests.Utilities
{
    /// <summary>
    /// Tests the language manager class.
    /// </summary>
    [TestClass]
    public class LanguageManagerTests : SGridServerTest 
    {
        /// <summary>
        /// Initializes this test.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Checks whether language settings are correcly set and retreived when no user is logged on.
        /// </summary>
        [TestMethod]
        public void TestLanguageSettings()
        {
            SecurityProvider.LogOut();

            LanguageManager.CurrentLanguage = LanguageManager.LanguageByCode("de-DE");

            Assert.IsTrue(LanguageManager.CurrentLanguage.Code == "de-DE");

            LanguageManager.CurrentLanguage = LanguageManager.LanguageByCode("en-US");

            Assert.IsTrue(LanguageManager.CurrentLanguage.Code == "en-US");
        }

        /// <summary>
        /// Checks whether language settings are correcly and persistently set and retreived when a user is logged on.
        /// </summary>
        [TestMethod]
        public void TestLanguageSettingsForUser()
        {
            SecurityProvider.LogOut();

            User user = TestUserC;

            SecurityProvider.LogIn(new UserContext(user));

            LanguageManager.CurrentLanguage = LanguageManager.LanguageByCode("de-DE");

            Assert.IsTrue(LanguageManager.CurrentLanguage.Code == "de-DE");

            SecurityProvider.LogIn(new UserContext(user));

            Assert.IsTrue(LanguageManager.CurrentLanguage.Code == "de-DE");
        }


        /// <summary>
        /// Cleans up this test.
        /// </summary>
        [TestCleanup()]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
