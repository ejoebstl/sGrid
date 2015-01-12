using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Models
{
    /// <summary>
    /// The UserScore class represents a user statistic which includes the corresponding user and the count of results the user has delivered.
    /// </summary>
    public class UserScore
    {
        /// <summary>
        /// The amount of results the user has delivered.
        /// </summary>
        public int CalculatedPackages { get; set; }
        /// <summary>
        /// The user associated with this UserScore object.
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// Creates a new instance of this class with given parameters.
        /// </summary>
        /// <param name="calculatedPackages">Given number of calculated packages.</param>
        /// <param name="account">Given account to assoziate with an object.</param>
        public UserScore(int calculatedPackages, Account account)
        {
            this.Account = account;
            this.CalculatedPackages = calculatedPackages;
        }
    }
}