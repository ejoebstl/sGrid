using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.Security;

namespace sGridServerTests.Security
{
    /// <summary>
    /// Tests  basic IdProvider management functions
    /// </summary>
    [TestClass]
    public class IdProviderManagementTests
    {
        /// <summary>
        /// Tests wheter a registered id provider is returned by the manager
        /// </summary>
        [TestMethod]
        public void TestBasicIdProviderManagementFunctions()
        {
            IdProviderDescription provider = new IdProviderDescription("test", "icon", "testprovider");

            IdProviderManager.RegisterIdProvider(provider);

            Assert.IsTrue(IdProviderManager.GetRegisteredProviders().Contains(provider));
        }
    }
}
