using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.CoinExchange
{
    /// <summary>
    /// This class represents arguments
    /// for the TransactionDone event. 
    /// </summary>
    public class TransactionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the source account of the transaction.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the destination account of the transaction.
        /// </summary>
        public Account Destination { get; private set; }

        /// <summary>
        /// Gets the value, in coins, of the transaction.
        /// </summary>
        public Account Source { get; private set; }

        /// <summary>
        /// Gets the description of the transaction.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Creates a new instance of this class, storing the given parameters into their corresponding properties. 
        /// </summary>
        /// <param name="source">The source account of the transaction.</param>
        /// <param name="destination">The destination account of the transaction.</param>
        /// <param name="value">The value, in coins, of the transaction.</param>
        /// <param name="description">The description of the transaction .</param>
        public TransactionEventArgs(Account source, Account destination, int value, string description)
        {
            this.Source = source;
            this.Destination = destination;
            this.Value = value;
            this.Description = description;
        }
    }
}