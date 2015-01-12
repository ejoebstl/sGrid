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
    /// Tests the error logger class.
    /// </summary>
    [TestClass]
    public class ErrorLoggerTests : SGridServerTest
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
        /// Tests whether error logging works. 
        /// </summary>
        [TestMethod]
        public void TestLogging()
        {
            string error = "ShmemException";
            string stacktrace = "[...]";

            ErrorLogger.Log(error, stacktrace);

            Assert.IsTrue(ErrorLogger.GetErrors().Where(x => x.Description == error && x.Stacktrace == stacktrace).Any());
            
            Error last =ErrorLogger.GetErrors().Last();

            Assert.IsTrue(last.Description == error && last.Stacktrace == stacktrace);
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
