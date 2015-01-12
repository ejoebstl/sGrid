using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.GridProviders;
using sGridServer.Code.Achievements;
using sGridServer.Code.DataAccessLayer;
using System.Data.SqlClient;
using sGridServer.Code.Rewards;

namespace sGridServer.Code.CoinExchange
{
    /// <summary>
    /// The CoinExchange class provides methods to buy rewards or grant coins for 
    /// delivered results or reached achievements. 
    /// Also, this class is responsible for inserting transactions into the database 
    /// and for keeping the CurrentBalance, TotalGrant und TotalSpent properties 
    /// of the corresponding Account object up to date. 
    /// <remarks>
    /// The purchasing process follows a simple pattern: The CoinExchange class is used to create an instance of ITransaction, which holds the
    /// state of the purchase. The ITransaction instance is then used to check the state and to conduct or cancel the purchase. 
    /// </remarks>
    /// </summary>
    public partial class CoinExchange
    {
        /// <summary>
        /// This event is raised whenever a transaction was done. 
        /// </summary>
        public static event EventHandler<TransactionEventArgs> TransactionDone;

        /// <summary>
        /// This member holds the user associated with this CoinExchange object. 
        /// </summary>
        private User currentUser;

        /// <summary>
        /// The database context. 
        /// </summary>
        private SGridDbContext dbContext;

        /// <summary>
        /// The options to use for coin transactions. 
        /// </summary>
        private System.Transactions.TransactionOptions transactionOptions;

        /// <summary>
        /// Gets the global account used for granting coins.
        /// </summary>
        private CoinAccount GrantAccount
        {
            get { return null; }
        }

        /// <summary>
        /// Creates a new instance of this class and stores 
        /// the given user into the corresponding private member. 
        /// </summary>
        /// <param name="currentUser">The user who should be associated with this CoinExchange object. </param>
        public CoinExchange(User currentUser)
        {
            this.currentUser = currentUser;
            this.dbContext = new SGridDbContext();
            this.transactionOptions = new System.Transactions.TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 0, 0, 10)
            };
        }

        /// <summary>
        /// Begins the purchase process and returns a CoinTransaction object. 
        /// </summary>
        /// <param name="reward">The reward to buy.</param>
        /// <param name="amount">The amount of rewards to buy.</param>
        /// <returns>A CoinTransaction object describing the purchase state.</returns>
        public ICoinTransaction BeginBuy(Reward reward, int amount = 1)
        {
            return new CoinTransaction(currentUser, reward, amount, this);
        }

        /// <summary>
        /// Conducts the given buy.
        /// </summary>
        /// <param name="transaction">The transaction to conduct.</param>
        /// <returns>The purchase object.</returns>
        private Purchase ConductBuy(CoinTransaction transaction)
        { 
            System.Transactions.TransactionScope scope = null;
            Purchase purchase = null;

            //Prepare transaction parameters
            string description = "Reward " + transaction.Reward.Name;
            int coinCount = transaction.Amount * transaction.Reward.Cost;
            
            try
            {
                //Create a new transaction scope, if applicable
                if (System.Transactions.Transaction.Current == null)
                {
                    scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, transactionOptions);
                }

                //Gets and locks the coin account to modify
                CoinAccount userAccount = GetAndLockCoinAccount(currentUser);
                CoinAccount partnerAccount = GetAndLockCoinAccount(currentUser);

                //Checks if the purchase state is valid. 
                if (currentUser.UserPermission != SiteRoles.User)
                {
                    throw new InvalidOperationException("Only users are allowed to buy rewards");
                }

                if (currentUser.Id != transaction.Buyer.Id)
                {
                    throw new InvalidOperationException("The given transaction is not associated with the same user as this coin exchange.");
                }

                if (userAccount.CurrentBalance < coinCount)
                {
                    throw new InvalidOperationException("User has not enough coins to buy the given reward.");
                }

                //Conduct the reward purchase - if this operatoin fails, we should cancel the payment process. 
                RewardManager rewardManager = new RewardManager();

                purchase = rewardManager.BuyReward(currentUser, transaction.Reward, transaction.Amount);

                //The reward was bought, conduct the coin transaction. 
                Transaction transactionRecord = new Transaction()
                {
                    Description = description,
                    Destination = transaction.Reward.CoinPartner.CoinAccount,
                    Source = userAccount,
                    Timestamp = DateTime.Now,
                    Value = coinCount
                };

                userAccount.CurrentBalance -= coinCount;
                userAccount.TotalSpent += coinCount;

                partnerAccount.TotalGrant += coinCount;

                dbContext.Transactions.Add(transactionRecord);

                //Save all changes
                ReleaseLocksAndSave();

                //Complete the transaction scope, and so, commit all changes. 
                if (scope != null)
                {
                    scope.Complete();
                }
            }
            finally
            {
                //In all cases, dispose our scope.
                if (scope != null)
                {
                    scope.Dispose();
                }
            }

            //Raise the transaction done event. 
            NotifyTransactionDone(currentUser, transaction.Reward.CoinPartner, coinCount, description);

            //Return the newly purchased reward. 
            return purchase;
        }

        /// <summary>
        /// Cancels the given coin transaction.
        /// </summary>
        /// <param name="transaction">The transaction to cancel.</param>
        private void CancelBuy(CoinTransaction transaction)
        {
            //Do nothing, since transaction scopes are only allocated when ConductBuy is called. 
            //This method is here for forward compatibility. 
        }

        /// <summary>
        /// Grants a given amount of coins onto the coin account of the 
        /// associated user and adds a reference to the grid project into 
        /// the transaction which is stored into the database. 
        /// </summary>
        /// <param name="gridProject">The grid project to associate with the coin transaction.</param>
        /// <param name="coinCount">The amount of coins to grant. </param>
        public void Grant(GridProjectDescription gridProject, int coinCount)
        {
            Grant(coinCount, "Project " + gridProject.Name);
        }

        /// <summary>
        /// Grants the given coin count with the given description to the coin account of the current user.
        /// </summary>
        /// <param name="coinCount">The amount of coins to grant.</param>
        /// <param name="description">The description.</param>
        private void Grant(int coinCount, string description)
        {
            //Create a new transaction scope, if applicable. 
            using(System.Transactions.TransactionScope scope = 
                new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Required, 
                    transactionOptions))
            {
                //Gets and locks the coin account to modify
                CoinAccount accountToUpdate = GetAndLockCoinAccount(currentUser);

                //Perform changes
                accountToUpdate.CurrentBalance += coinCount;
                accountToUpdate.TotalGrant += coinCount;

                Transaction trans = new Transaction()
                {
                    Description = description,
                    Destination = accountToUpdate,
                    Source = GrantAccount,
                    Timestamp = DateTime.Now,
                    Value = coinCount
                };

                dbContext.Transactions.Add(trans);

                //Save changes
                ReleaseLocksAndSave();

                //Complete the scope and commit changes. 
                if (scope != null)
                {
                    scope.Complete();
                }
            }

            NotifyTransactionDone(null, currentUser, coinCount, description);

        }

        /// <summary>
        /// Loads a coin account from the database, using an update lock.
        /// </summary>
        /// <param name="account">The account to get the coin account for.</param>
        /// <returns>The loaded and locked coin account.</returns>
        private CoinAccount GetAndLockCoinAccount(Account account)
        {
            //Use hand-crafted SQL to select the target account with an update lock, since LINQ does not support selecting with locks. 
            return dbContext.CoinAccounts.SqlQuery("SELECT TOP 1 * FROM CoinAccounts WITH (UPDLOCK) WHERE id = @id",
                new SqlParameter("id", account.CoinAccountId)).Single();
        }

        /// <summary>
        /// Saves all changes. 
        /// </summary>
        private void ReleaseLocksAndSave()
        {
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Grants a given amount of coins onto the coin account of the 
        /// associated user and adds a reference to the given achievement 
        /// into the transaction which is stored into the database. 
        /// </summary>
        /// <param name="achievement">The achievement to associate with the coin transaction. </param>
        /// <param name="coinCount">The amount of coins to grant. </param>
        public void Grant(Achievement achievement, int coinCount)
        {
            Grant(coinCount, "Achievement " + achievement.Name);
        }

        /// <summary>
        /// Gets the coin account associated with the given account.
        /// </summary>
        /// <param name="account">The account to get the coin account for. </param>
        /// <returns>A CoinAccount object.</returns>
        [Obsolete("Simply use the Account.CoinAccount property.")]
        public static CoinAccount GetCoinAccount(Account account)
        {
            return account.CoinAccount;
        }

        /// <summary>
        /// Gets all transactions associated with the given account.
        /// </summary>
        /// <param name="account">The account to get the transactions for. </param>
        /// <returns>An enumeration of transactions.</returns>
        public static IEnumerable<Transaction> GetTransactions(Account account)
        {
            SGridDbContext dbContext = new SGridDbContext();

            return from t in dbContext.Transactions.AsNoTracking()
                   where t.Source.Id == account.CoinAccount.Id
                   || t.Destination.Id == account.CoinAccount.Id
                   select t;
        }

        /// <summary>
        /// Raises the TransactionDone event using the given parameters. 
        /// </summary>
        /// <param name="source">The source account of the transaction.</param>
        /// <param name="destination">The destination account of the transaction.</param>
        /// <param name="value">The value, in coins, of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        public static void NotifyTransactionDone(Account source, Account destination, int value, string description) 
        {
            if(TransactionDone != null)
            {
                TransactionDone(null, new TransactionEventArgs(source, destination, value, description));
            }
        }

    }
}