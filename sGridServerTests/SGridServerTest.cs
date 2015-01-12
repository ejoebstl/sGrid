using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.Utilities;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using System.Threading;

namespace sGridServerTests
{
    public class SGridServerTest
    {
        /// <summary>
        /// Database transaction scope for reverting changes. 
        /// </summary>
        private TransactionScope scope;

        /// <summary>
        /// A User testaccount for testing. 
        /// </summary>
        protected User TestUserA { get; set; }

        /// <summary>
        /// A User testaccount for testing. 
        /// </summary>
        protected User TestUserB { get; set; }

        /// <summary>
        /// A User testaccount for testing. 
        /// </summary>
        protected User TestUserC { get; set; }

        /// <summary>
        /// A User testaccount for testing. 
        /// </summary>
        protected User TestUserD { get; set; }

        /// <summary>
        /// A CoinPartner testaccount for testing. 
        /// </summary>
        protected CoinPartner TestCoinPartnerA { get; set; }

        /// <summary>
        /// A Sponsor testaccount for testing. 
        /// </summary>
        protected Sponsor TestSponsorA { get; set; }

        /// <summary>
        /// A SGridTeamMember testaccount for testing. 
        /// </summary>
        protected SGridTeamMember TestAdminA { get; set; }

        /// <summary>
        /// MemberManager for member managing. 
        /// </summary>
        protected MemberManager MemberManager { get; set; }

        /// <summary>
        /// Mutex object for synchronization. 
        /// </summary>
        private static object mutex;

        /// <summary>
        /// Static constructor for mutex initialization. 
        /// </summary>
        static SGridServerTest()
        {
            mutex = new object();
        }

        /// <summary>
        /// Initializes the Database and starts a transaction. 
        /// </summary>
        protected void InitializeDatabase()
        {
            // Lock the whole test run, 
            // since SQL server express edition does not handle concurrent access well.
            // Be careful! This will cause all unit tests to fail if one unit test fails. 
            Monitor.Enter(mutex);

            //Create the database, if it not exists. 
            using (SGridDbContext context = new SGridDbContext())
            {
                context.Database.CreateIfNotExists();
            }

            //Create a fake HTTP context so we can use the session functionality. 
            HttpContext.Current = TestUtilities.FakeHttpContext();

            //Configure languages. 
            if (LanguageManager.Languages.Count() == 0)
            {
                LanguageManager.RegisterLanguage(new LanguageItem("English", "en-US", ""));
                LanguageManager.RegisterLanguage(new LanguageItem("Deutsch", "de-DE", ""));
            }

            //Start a new transaction
            scope = new TransactionScope(TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 10, 0, 0)
            });

            //Create users for testing
            MemberManager = new MemberManager();

            TestUserA = MemberManager.CreateUser(new User()
            {
                Nickname = "LumLum_569107_test",
                EMail = "LumLum_569107_test@sgrid.ipd.kit.edu",
            });

            TestUserB = MemberManager.CreateUser(new User()
            {
                Nickname = "Ainara_569107_test",
                EMail = "Ainara_569107_test@sgrid.ipd.kit.edu",
            });

            TestUserC = MemberManager.CreateUser(new User()
            {
                Nickname = "Jerome_569107_test",
                EMail = "Jerome_569107_test@sgrid.ipd.kit.edu",
            });

            TestUserD = MemberManager.CreateUser(new User()
            {
                Nickname = "Emi_569107_test",
                EMail = "emi_569107_test@sgrid.ipd.kit.edu"
            });

            TestAdminA = MemberManager.ElevateToTeamMember(MemberManager.CreateUser(new User()
            {
                Nickname = "Admin_569107_test",
                EMail = "Admin_569107_test@sgrid.ipd.kit.edu"
            }));

            TestCoinPartnerA = MemberManager.ElevateToCoinPartner(MemberManager.CreateUser(new User()
            {
                Nickname = "CoinPartner_569107_test",
                EMail = "CoinPartner_569107_test@sgrid.ipd.kit.edu"
            }));

            TestSponsorA = MemberManager.ElevateToSponsor(MemberManager.CreateUser(new User()
            {
                Nickname = "Sponsor_569107_test",
                EMail = "Sponsor_569107_test@sgrid.ipd.kit.edu"
            }));
        }

        /// <summary>
        /// Disposes the transaction. 
        /// </summary>
        protected void CleanupDatabase()
        {
            //Destroy our transaction, so the test changes are not persistent. 
            scope.Dispose();
            System.Transactions.Transaction.Current = null;
            HttpContext.Current = null;

            Monitor.Exit(mutex);
        }
    }
}
