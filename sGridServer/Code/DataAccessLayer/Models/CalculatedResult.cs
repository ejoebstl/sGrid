using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents a calculation result. 
    /// </summary>
    public class CalculatedResult
    {
        /// <summary>
        /// Gets or sets the id of the element in the calculated result database set.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the time when the result was received by the server. 
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime ServerReceivedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the time when the work unit was sent from the server to the client. 
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime ServerSentTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the time when the result was sent from the client to the server. 
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime ClientSentTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the time when the work unit was was received by the client. 
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime ClientReceivedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the time when the result was validated by the project server. 
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime ValidatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the short name of the project to which the result belongs.
        /// </summary>
        public String ProjectShortName { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the result of the calculation has been validated and is valid.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the account id of the user who performed the calculation.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the calculation.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets the ID of the device which returned the result. 
        /// </summary>
        public string DeviceIdentifyer { get; set; }

        /// <summary>
        /// Gets or sets the state of this result. 
        /// </summary>
        public ResultState State { get; set; }

        /// <summary>
        /// Gets or sets the name of the work unit associated with this result. 
        /// </summary>
        public string WorkUnitName { get; set; }

        /// <summary>
        /// A default constructor of the calculated result.
        /// </summary>
        public CalculatedResult()
        {
            this.ServerReceivedTimestamp = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            this.ServerSentTimestamp = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            this.ClientReceivedTimestamp = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            this.ClientSentTimestamp = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            this.ValidatedTimestamp = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            this.ProjectShortName = "";
        }
    }

    /// <summary>
    /// An enumaration defining result states.
    /// </summary>
    public enum ResultState
    {
        /// <summary>
        /// The result has been sent to the client.
        /// </summary>
        SentToClient = 1,
        /// <summary>
        /// The result was received by the client.
        /// </summary>
        ReceivedByClient = 2,
        /// <summary>
        /// The result has been sent to the server.
        /// </summary>
        SentToServer = 3,
        /// <summary>
        /// The result was received by the server.
        /// </summary>
        ReceivedByServer = 4,
        /// <summary>
        /// The result was validated.
        /// </summary>
        Validated = 5
    }
}