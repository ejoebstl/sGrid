using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.Utilities;

namespace sGridServerTests.DataAccessLayer
{
    [TestClass]
    public class MultiLanguageStringTests : SGridServerTest
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
        /// Checks whether translations are retreived correctly.
        /// </summary>
        [TestMethod]
        public void TestLanguageStringTranslation()
        {
            //Insert a language string

            MultiLanguageString multiString = new MultiLanguageString();
            multiString.Add("de-DE", "Deutscher Test");
            multiString.Add("en-US", "Englischer Test");

            multiString.InsertMissingTranslations();

            SGridDbContext context = new SGridDbContext();

            multiString.Reattach(context);

            context.SaveChanges();

            MultiLanguageString loadedMultiString = context.Strings.AsNoTracking().Where(x => x.Id == multiString.Id).FirstOrDefault();

            LanguageManager.SetThreadCulture("de-DE");

            Assert.AreEqual(loadedMultiString.Text, "Deutscher Test");
            Assert.AreEqual(loadedMultiString.ToString(), "Deutscher Test"); 

            LanguageManager.SetThreadCulture("en-US");

            Assert.AreEqual(loadedMultiString.Text, "Englischer Test");
            Assert.AreEqual(loadedMultiString.ToString(), "Englischer Test");

            loadedMultiString.Remove(context);

            context.SaveChanges();
        }

        /// <summary>
        /// Tests insertion and deletion of a language string and it's corresponding translations. 
        /// </summary>
        [TestMethod]
        public void TestLanguageStringInsertionAndDeletion()
        {
            //Insert a language string

            MultiLanguageString multiString = new MultiLanguageString();
            multiString.Add("de-DE", "Deutscher Test");
            multiString.Add("en-US", "Englischer Test");

            multiString.InsertMissingTranslations();

            SGridDbContext context = new SGridDbContext();

            multiString.Reattach(context);

            context.SaveChanges();

            //Check whether the language string has been correctly inserted

            MultiLanguageString loadedMultiString = context.Strings.AsNoTracking().Where(x => x.Id == multiString.Id).FirstOrDefault();

            foreach (Translation translation in multiString.Translations)
            {
                Assert.IsTrue(loadedMultiString.Translations.Where(x => x.Text == translation.Text && x.Culture == translation.Culture).Any());
            }

            foreach (Translation translation in loadedMultiString.Translations)
            {
                Assert.IsTrue(multiString.Translations.Where(x => x.Text == translation.Text && x.Culture == translation.Culture).Any());
            }

            //Delete the language string

            loadedMultiString.Remove(context);

            context.SaveChanges();

            //Check whether the language string has been correctly deleted

            Assert.IsFalse(context.Strings.Where(x => x.Id == multiString.Id).Any());

            foreach (Translation translation in multiString.Translations)
            {
                Assert.IsFalse(context.Translations.Where(x => x.Id == translation.Id).Any());

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
