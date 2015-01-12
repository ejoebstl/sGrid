using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.CoinExchange
{
    public partial class CoinExchange 
    {
        /// <summary>
        /// This class represents a transaction. Either EndBuy or 
        /// CancelBuy has to be called on the transaction before 
        /// the object is disposed by the garbage collector since the 
        /// object holds a lock in the database. This class is an inner
        /// class of CoinExchange. 
        /// </summary>
        private class CoinTransaction : sGridServer.Code.CoinExchange.ICoinTransaction
        {
            /// <summary>
            /// The CoinExchange associated with this transaction. 
            /// </summary>
            private CoinExchange coinExchange;

            /// <summary>
            /// Gets the amount of rewards to buy.
            /// </summary>
            public int Amount { get; private set; }

            /// <summary>
            /// Gets the buyer of the reward.
            /// </summary>
            public User Buyer { get; private set; }

            /// <summary>
            /// Gets a bool indicating whether the reward can be bought now.
            /// </summary>
            public bool CanBuy
            {
                get
                {
                    return HasEnoughCoins && !HasExpired && RewardAvailable;
                }
            }

            /// <summary>
            /// Gets a bool indicating whether a sufficient amount of rewards is still available. 
            /// </summary>
            public bool RewardAvailable
            {
                get
                {
                    return Reward.Amount >= Amount;
                }
            }

            /// <summary>
            /// Gets a bool indicating whether the 
            /// user has enough balance on his coin account.
            /// </summary>
            public bool HasEnoughCoins
            {
                get
                {
                    return Buyer.CoinAccount.CurrentBalance >= Reward.Cost * Amount;
                }
            }

            /// <summary>
            /// Gets a bool indicating whether this 
            /// transaction is valid or has expired.
            /// </summary>
            public bool HasExpired { get; private set; }

            /// <summary>
            /// Gets the reward to buy.
            /// </summary>
            public Reward Reward { get; private set; }

            /// <summary>
            /// Creates a new instance of this class, storing all 
            /// given parameters and locking the reward in the database, 
            /// so no other user can buy it as long as this transaction is active. 
            /// </summary>
            /// <param name="buyer">The buyer of the reward.</param>
            /// <param name="reward">The reward to buy.</param>
            /// <param name="amount">The amount of rewards to buy.</param>
            /// <param name="coinExchange">The CoinExchange object which created this transaction.</param>
            public CoinTransaction(User buyer, Reward reward, int amount, CoinExchange coinExchange)
            {
                this.Buyer = buyer;
                this.Reward = reward;
                this.Amount = amount;
                this.coinExchange = coinExchange;

                this.HasExpired = false;
            }

            /// <summary>
            /// Cancels the purchase associated with this transaction 
            /// and frees the lock on the database this object holds. 
            /// The object is invalid afterwards and must not be accessed any more. 
            /// </summary>
            public void CancelBuy()
            {
                this.coinExchange.CancelBuy(this);
                this.HasExpired = true;
            }

            /// <summary>
            /// Conducts the purchase associated with this transaction and 
            /// frees the lock on the database this object holds. The object 
            /// is invalid afterwards and must not be accessed any more. 
            /// </summary>
            /// <returns>The purchase object, which makes it possible to obtain the reward.</returns>
            public Purchase EndBuy()
            {
                Purchase purchase = this.coinExchange.ConductBuy(this);
                this.HasExpired = true;
                return purchase;
            }

            /// <summary>
            /// Is called by the garbage collector when the object is destroyed. 
            /// If the buy is neither cancelled nor conducted and the transaction 
            /// still holds a lock in the database when this method is called, an 
            /// InvalidOperationException is raised. 
            /// </summary>
            ~CoinTransaction()
            {
                if (!HasExpired)
                {
                    throw new InvalidOperationException("Transaction was neither conducted nor cancelled.");
                }
            }
        }
    }
}