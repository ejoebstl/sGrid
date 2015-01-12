using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents authentication information for grid providers.
    /// </summary>
    public class GridProviderAuthenticationData
    {
        /// <summary>
        /// Gets or sets the id of an element in the data for grid provider database set.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the information necessary for the client 
        /// to authenticate with the grid provider server.
        /// </summary>
        public string AuthenticationInfo { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the grid provider associated with this authentication data.
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// Gets or sets username used by the corresponding grid provider.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets a token which is used to access user information over the API of the corresponding grid provider.
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Gets or sets a random password, if set by sGrid. 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the account id of the user to which this authentication data belongs.
        /// </summary>
        [ForeignKey("User")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user to which this authentication data belongs.
        /// </summary>
        public virtual User User { get; set; }


        /// <summary>
        /// A default constructor of the data for grid provider.
        /// </summary>
        public GridProviderAuthenticationData()
        {
            this.AuthenticationInfo = "";
            this.ProviderId = "";
            this.UserName = "";
            this.UserToken = "";
            this.Id = -1;
            this.Password = "";
        }
    }
}