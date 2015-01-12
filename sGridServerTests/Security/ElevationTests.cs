using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;

namespace sGridServerTests.Security
{
    /// <summary>
    /// Tests the elevation functionality of the MemberManager class
    /// </summary>
    [TestClass]
    public class ElevationTests : SGridServerTest 
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
        /// Tests whether elevating accounts works.
        /// </summary>
        [TestMethod]
        public void TestElevation()
        {
            MemberManager manager = new MemberManager();

            Account testA = manager.Users.Where(u => u.Id == TestUserA.Id).First();
            Account testB = manager.Users.Where(u => u.Id == TestUserB.Id).First();
            Account testC = manager.Users.Where(u => u.Id == TestUserC.Id).First();

            CoinPartner coinPartnerA = manager.ElevateToCoinPartner((User)testA);
            Sponsor sponsorB = manager.ElevateToSponsor((User)testB);
            SGridTeamMember adminC = manager.ElevateToTeamMember((User)testC);

            testA = manager.Users.Where(u => u.Id == TestUserA.Id).FirstOrDefault();
            testB = manager.Users.Where(u => u.Id == TestUserB.Id).FirstOrDefault();
            testC = manager.Users.Where(u => u.Id == TestUserC.Id).FirstOrDefault();

            Assert.IsNull(testA);
            Assert.IsNull(testB);
            Assert.IsNull(testC);

            testA = manager.CoinPartners.Where(u => u.Id == coinPartnerA.Id).First();
            testB = manager.Sponsors.Where(u => u.Id == sponsorB.Id).First();
            testC = manager.SGridMembers.Where(u => u.Id == adminC.Id).First();

            Assert.IsInstanceOfType(testA, typeof(CoinPartner));
            Assert.IsInstanceOfType(testB, typeof(Sponsor));
            Assert.IsInstanceOfType(testC, typeof(SGridTeamMember));

            Assert.IsTrue(testA.UserPermission == SiteRoles.CoinPartner);
            Assert.IsTrue(testB.UserPermission == SiteRoles.Sponsor);
            Assert.IsTrue(testC.UserPermission == SiteRoles.Admin);
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
