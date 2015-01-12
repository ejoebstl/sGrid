using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Security;
using System.Reflection;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServerTests.Security
{
    /// <summary>
    /// Tests the SessoinManagementModule and Authorization of sGrid
    /// </summary>
    [TestClass]
    public class SessionManagementModuleTests : SGridServerTest 
    {
        /// <summary>
        /// Initializes the test by creating a dummy http context. 
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();   
        }

        /// <summary>
        /// Tests for each combination of each site role if the authorization fails or is successful.
        /// </summary>
        [TestMethod]
        public void TestSecurityProviderSuccess()
        {
            CheckLogin(TestUserA);
            CheckLogin(TestSponsorA);
            CheckLogin(TestAdminA);
            CheckLogin(TestCoinPartnerA);
        }

        /// <summary>
        /// Tests for all permissions, if permissions different then the given account permissions fail. 
        /// </summary>
        /// <param name="account">The account to check.</param>
        private void CheckLogin(Account account)
        {
            //Activate the account
            MemberManager.ActivateAccount(account);

            //Log in the user
            UserContext context = new UserContext(account);

            SecurityProvider.LogIn(context);

            //Check Authorize Core function
            for (int x = 1; x <= 8; x *= 2)
            {
                SGridAuthorizeAttribute auth = new SGridAuthorizeAttribute() { RequiredPermissions = (SiteRoles)x };

                Assert.AreEqual(RunAuthorizeCore(auth), (int)context.UserPermissions == x, "Authorization error! " + (context.UserPermissions) + " - " + ((SiteRoles)x));
            }

            //Log out the account
            SecurityProvider.LogOut();
        }

        /// <summary>
        /// Tests for a failure, if no user is logged in. 
        /// </summary>
        [TestMethod]
        public void TestSecurityProviderFailure()
        {
            SGridAuthorizeAttribute auth = new SGridAuthorizeAttribute() { RequiredPermissions = SiteRoles.Admin };
            Assert.IsFalse(RunAuthorizeCore(auth), "Authorization was successfull. It should have failed.");
        }

        /// <summary>
        /// Tests for various combinations of site role flags, especially no flags.
        /// </summary>
        [TestMethod]
        public void TestSecurityProviderCombinedFlags()
        {
            MemberManager.ActivateAccount(TestCoinPartnerA);

            UserContext context = new UserContext(TestCoinPartnerA);

            SecurityProvider.LogIn(context);

            SGridAuthorizeAttribute auth = new SGridAuthorizeAttribute() { RequiredPermissions = SiteRoles.CoinPartner | SiteRoles.Sponsor };
            Assert.IsTrue(RunAuthorizeCore(auth), "Authorization failed. It should have been successful.");

            auth = new SGridAuthorizeAttribute() { RequiredPermissions = SiteRoles.User | SiteRoles.Admin };
            Assert.IsFalse(RunAuthorizeCore(auth), "Authorization was successfull. It should have failed.");

            auth = new SGridAuthorizeAttribute();
            Assert.IsFalse(RunAuthorizeCore(auth), "Authorization was successfull. It should have failed.");

            SecurityProvider.LogOut();
        }

        /// <summary>
        /// Runs the protected AuthorizeCore method and returns the result.
        /// </summary>
        /// <param name="auth">The attribute to run the AuthorizeCore method on.</param>
        /// <returns>The value returned by AuthorizeCore.</returns>
        private bool RunAuthorizeCore(SGridAuthorizeAttribute auth)
        {
            MethodInfo info = typeof(SGridAuthorizeAttribute).GetMethod("AuthorizeCore", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)info.Invoke(auth, new object[] { new HttpContextWrapper(System.Web.HttpContext.Current) });
        }

        /// <summary>
        /// Cleans up the test by removing the session. 
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
