using System;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.CoinExchange
{    
    /// <summary>
    /// This interface represents a transaction. Either EndBuy or 
    /// CancelBuy has to be called on the transaction before 
    /// the object is disposed by the garbage collector since the 
    /// object holds a lock in the database. 
    /// </summary>
    public interface ICoinTransaction
    {

        /// <summary>
        /// Gets the amount of rewards to buy.
        /// </summary>
        int Amount { get; }

        /// <summary>
        /// Gets the buyer of the reward.
        /// </summary>
        sGridServer.Code.DataAccessLayer.Models.User Buyer { get; }

        /// <summary>
        /// Gets a bool indicating whether the reward can be bought now.
        /// </summary>
        bool CanBuy { get; }

        /// <summary>
        /// Gets a bool indicating whether a sufficient amount of rewards is still available. 
        /// </summary>
        bool RewardAvailable { get; }

        /// <summary>
        /// Cancels the purchase associated with this transaction 
        /// and frees the lock on the database this object holds. 
        /// The object is invalid afterwards and must not be accessed any more. 
        /// </summary>
        void CancelBuy();

        /// <summary>
        /// Conducts the purchase associated with this transaction and 
        /// frees the lock on the database this object holds. The object 
        /// is invalid afterwards and must not be accessed any more. 
        /// </summary>
        /// <returns>The purchase object, which makes it possible to obtain the reward.</returns>
        Purchase EndBuy();

        /// <summary>
        /// Gets a bool indicating whether the 
        /// user has enough balance on his coin account.
        /// </summary>
        bool HasEnoughCoins { get; }

        /// <summary>
        /// Gets a bool indicating whether this 
        /// transaction is valid or has expired.
        /// </summary>
        bool HasExpired { get; }

        /// <summary>
        /// Gets the reward to buy.
        /// </summary>
        sGridServer.Code.DataAccessLayer.Models.Reward Reward { get; }
    }
}
