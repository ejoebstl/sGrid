using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace sGridServer.Models
{
    /// <summary>
    /// A model used for Facebook registration.
    /// </summary>
    public class FacebookRegistrationData
    {
        /// <summary>
        /// The facebook id of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The nickname of the user.
        /// </summary>
        [Required()]
        [MinLength(1)]
        public string Nickname { get; set; }

        /// <summary>
        /// The culture of the user.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The e-mail address of the user.
        /// </summary>
        [Required()]
        [DataType(DataType.EmailAddress)]
        public string EMailAddress { get; set; }

        /// <summary>
        /// A bool indicating whether the sGrid profile picture should be gathered from Facebook. 
        /// </summary>
        public bool GetPictureFormFacebook { get; set; }

        /// <summary>
        /// The url pointing the profile picture of the user. 
        /// </summary>
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public FacebookRegistrationData()
        {
            Nickname = "";
            EMailAddress = "";
            GetPictureFormFacebook = true;
        }
    }
}