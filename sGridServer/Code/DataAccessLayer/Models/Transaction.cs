using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a coin transaction.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the description of the transaction.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets a timestamp indicating when the transaction was done.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the amount of coins transferred.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Value { get; set; }


        /// <summary>
        /// Gets or sets the coin account id of the receiver.
        /// </summary>
        [ForeignKey("Destination")]
        public int DestinationId { get; set; }

        /// <summary>
        /// Gets or sets the coin account id of the sender.
        /// </summary>
        [ForeignKey("Source")]
        public int? SourceId { get; set; }


        /// <summary>
        /// Gets or sets the coin account of the sender.
        /// </summary>
        public virtual CoinAccount Source { get; set; }

        /// <summary>
        /// Gets or sets the coin account of the receiver.
        /// </summary>
        public virtual CoinAccount Destination { get; set; }


        /// <summary>
        /// A default constructor of the transaction.
        /// </summary>
        public Transaction()
        {
            this.Description = "";
            this.Timestamp = DateTime.Now;
        }

    }
}