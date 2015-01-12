using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.GridProviders;
using System.Transactions;
using sGridServer.Code.GridProviders.WorldCommunityGrid;

namespace sGridServerTests.GridProvider
{
    /// <summary>
    /// This test tests grid provider management functions, like attach and detach.
    /// </summary>
    [TestClass]
    public class GridProviderManagerTests : SGridServerTest
    {
        /// <summary>
        /// A test user for testing.
        /// </summary>
        User testUser;

        /// <summary>
        /// Initializes the test environment.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Checks whether project attaching works.
        /// </summary>
        [TestMethod]
        public void TestProjectAttach()
        {
            User testUser = base.TestUserA;

            GridProviderManager gridManager = new GridProviderManager(testUser);

            GridProjectDescription malaria = new GridProjectDescription("ml@home2", "", "Malaria @ Home", "", "", "", "", 0, 100);
            GridProjectDescription cancer = new GridProjectDescription("cn@home2", "", "Cancer @ Home", "", "", "", "", 0, 100);

            GridProviderDescription wcg = new GridProviderDescription("wcg2", "WorldCommunityGrid", "", "", "", "", "", new GridProjectDescription[] { malaria, cancer }, typeof(MockWorldCommunityGridProvider));

            GridProviderManager.RegisterProvider(wcg);

            gridManager.AttachToProject(malaria);

            Assert.AreEqual(gridManager.CurrentProject, malaria);

            gridManager.DetachFromProject(malaria);

            Assert.IsNull(gridManager.CurrentProject);
        }

        /// <summary>
        /// Checks whether attaching to two projects is correctly handled.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAttachError()
        {
            User testUser = base.TestUserA;

            GridProviderManager gridManager = new GridProviderManager(testUser);

            GridProjectDescription malaria = new GridProjectDescription("ml@home3", "", "Malaria @ Home", "", "", "", "", 0, 100);
            GridProjectDescription cancer = new GridProjectDescription("cn@home3", "", "Cancer @ Home", "", "", "", "", 0, 100);

            GridProviderDescription wcg = new GridProviderDescription("wcg3", "WorldCommunityGrid", "", "", "", "", "", new GridProjectDescription[] { malaria, cancer }, typeof(MockWorldCommunityGridProvider));

            GridProviderManager.RegisterProvider(wcg);

            gridManager.AttachToProject(cancer);
            gridManager.AttachToProject(malaria);

        }

        /// <summary>
        /// Tests whether grid provider management (registering and searching for projects and providers) works correctly.
        /// </summary>
        [TestMethod]
        public void TestProviderManagement()
        {
            GridProjectDescription malaria = new GridProjectDescription("ml@home", "", "Malaria @ Home", "", "", "", "", 0, 100);
            GridProjectDescription cancer = new GridProjectDescription("cn@home", "", "Cancer @ Home", "", "", "", "", 0, 100);

            GridProviderDescription wcg = new GridProviderDescription("wcg", "WorldCommunityGrid", "", "", "", "", "", new GridProjectDescription[] { malaria, cancer }, typeof(MockWorldCommunityGridProvider));

            GridProviderManager.RegisterProvider(wcg);

            Assert.AreSame(GridProviderManager.ProjectForName(malaria.ShortName), malaria);
            Assert.AreSame(GridProviderManager.ProviderForName(wcg.Id), wcg);

            Assert.IsTrue(GridProviderManager.RegisteredProviders.Contains(wcg));
        }

        /// <summary>
        /// Cleans up the test environment. 
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            CleanupDatabase();
        }
    }
}
