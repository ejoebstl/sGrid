using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Models
{
    /// <summary>
    /// This class represents a captcha. 
    /// </summary>
    public class Captcha
    {
        /// <summary>
        /// Gets the code associated with this captcha. 
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets the url to the GetCaptcha method of the corresponding captcha controller. 
        /// </summary>
        public string EnteredText { get; set; }

        /// <summary>
        /// Gets or sets the text entered by the user when the captcha was shown. 
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Emtpy constructor so we can use this class as model calss. 
        /// </summary>
        public Captcha()
        {
            Code = "";
            EnteredText = "";
            ImageUrl = "";
        }

        /// <summary>
        /// Creates a new instance of this class using the given parameters. 
        /// </summary>
        /// <param name="code">The code associated with this captcha. This code is used by the CaptchaGenerator and the 
        /// CaptchaController classes to get the text to show on the captcha image from the session. </param>
        /// <param name="imageUrl">The url to the GetCaptcha method of the corresponding captcha controller. 
        /// This url must include the code of the captcha as parameter. </param>
        public Captcha(string code, string imageUrl) : this()
        {
            this.Code = code;
            this.ImageUrl = imageUrl;
        }
    }
}