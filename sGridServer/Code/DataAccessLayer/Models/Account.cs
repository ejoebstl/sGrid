using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// This class represents an account in the sGrid application
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Gets or sets the participant id of an account.
        /// </summary>
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets a token which is used to match the account with the data provided by id providers.
        /// If the id provider is the default id provider which uses e-mail address and password for authentication,
        /// this field is used to store the SHA1 password hash.
        /// </summary>
        public String AccountToken { get; set; }

        /// <summary>
        /// Gets or sets a bool indicating whether an account is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a token which is used to authenticate the client on the server.
        /// </summary>
        public String AuthenticationToken { get; set; }

        /// <summary>
        /// Gets or sets the coin account id of an account.
        /// </summary>
        public int CoinAccountId { get; set; }

        /// <summary>
        /// Gets or sets the language setting associated with an account. 
        /// </summary>
        public String Culture { get; set; }

        /// <summary>
        /// Gets or sets the email address of an account.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public String EMail { get; set; }

        /// <summary>
        /// Gets or sets the type of the id provider used.
        /// </summary>
        public String IdType { get; set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        [Required()]
        [MinLength(1)]
        public String Nickname { get; set; }

        /// <summary>
        /// Gets or sets a boolean which indicates whether the user should be notified by mail when reaching an achievement.
        /// </summary>
        public bool NotifyOnAchievementReached { get; set; }

        /// <summary>
        /// Gets or sets a boolean which indicates whether the user should be notified by mail over coin account changes. 
        /// </summary>
        public bool NotifyOnCoinBalanceChanged { get; set; }

        /// <summary>
        /// Gets or sets a boolean which indicates whether the user should be notified by mail over the change of his associated project by mail.
        /// </summary>
        public bool NotifyOnProjectChanged { get; set; }

        /// <summary>
        /// Gets or sets a code which can be used to reset the password.
        /// </summary>
        public String PasswordResetCode { get; set; }

        /// <summary>
        /// Gets or sets the picture url of the profile picture.
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public String Picture { get; set; }

        /// <summary>
        /// Gets or sets the privacy options of the account.
        /// </summary>
        public PrivacyLevel Privacy { get; set; }


        /// <summary>
        /// Internally used to overcome Entity Framework's problems with enums.
        /// Use Privacy instead.
        /// </summary>
        [Obsolete("Use Privacy instead.")]
        public int InternalPrivacy
        {
            get
            {
                return (int)Privacy;
            }
            set
            {
                Privacy = (PrivacyLevel)value;
            }
        }


        /// <summary>
        /// Gets or sets a registration date of the account.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Gets or sets a boolean which indicates whether the account should be shown in the high score tables.
        /// </summary>
        public bool ShowInHighScore { get; set; }

        /// <summary>
        /// Gets the role of the account. 
        /// This property is used to distinguish between different account types and to check permissions. 
        /// </summary>
        public SiteRoles UserPermission { get; protected set; }

        /// <summary>
        /// Gets or sets a coin account of an account.
        /// </summary>
        public virtual CoinAccount CoinAccount { get; set; }

        /// <summary>
        /// Gets or sets a list of messages which are sended from an account.
        /// </summary>
        public virtual List<Message> Messages { get; set; }

        /// <summary>
        /// A default constructor of the account.
        /// </summary>
        public Account()
        {
            this.AccountToken = "";
            this.AuthenticationToken = "";
            this.Culture = "";
            this.EMail = "";
            this.Id = -1;
            this.IdType = "";
            this.Nickname = "";
            this.PasswordResetCode = "";
            this.RegistrationDate = DateTime.Now;
            this.ShowInHighScore = true;
        }
    }
}