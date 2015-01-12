using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class generates and validates captchas. 
    /// </summary>
    public class CaptchaGenerator
    {
        /// <summary>
        /// Captcha session key.
        /// </summary>
        private const string CaptchaSessionKeyPrefix = "CaptchaText";

        /// <summary>
        /// The code associated with the captcha generator.
        /// </summary>
        private string code;

        /// <summary>
        /// The System.Web.Mvc.ControllerContext object 
        /// of the controller using this captcha generator. 
        /// </summary>
        private ControllerContext context;

        /// <summary>
        /// Gets the captcha text associated with the current context and the given code. 
        /// </summary>
        public string CurrentCaptchaText
        {
            //The captcha text itself is stored in the session. 
            get { return (string)context.HttpContext.Session[CaptchaSessionKeyPrefix + code]; }
            private set { context.HttpContext.Session[CaptchaSessionKeyPrefix + code] = value; }
        }

        /// <summary>
        /// Generates a new instance of this class, using the given parameters. 
        /// </summary>
        /// <param name="context">The System.Web.Mvc.ControllerContext object of the controller using the captcha.</param>
        /// <param name="code">A code uniquely identifying the controller using the captcha.</param>
        public CaptchaGenerator(ControllerContext context, string code)
        {
            this.code = code;
            this.context = context;
            if (this.CurrentCaptchaText == null)
            {
                this.CurrentCaptchaText = "";
            }
        }

        /// <summary>
        /// Creates a new Captcha object by generating a text to show and stores 
        /// this text in the session using the code given to the constructor as identifier. 
        /// </summary>
        /// <returns>The created Captcha object.</returns>
        public Models.Captcha CreateCaptcha()
        {
            //Generates a new Captcha and temporarily stores the captcha text. 
            Random r = new Random();
            UrlHelper urlHelper = new UrlHelper(context.RequestContext);

            CurrentCaptchaText = r.Next(0, 999999).ToString("D6");

            //Returns the new Captcha object. 
            return new Models.Captcha(code, urlHelper.Action("GetCaptcha", "Captcha", new {captchaCode = (code), rnd = r.Next()} ));
        }

        /// <summary>
        /// Validates whether the given captcha answer was valid. This is done by comparing the EnteredText property of
        /// the Captcha object with the text stored in the session by the CreateCaptcha method before. 
        /// </summary>
        /// <param name="captcha">The captcha to validate.</param>
        /// <returns>True, if the captcha was valid, false if not.</returns>
        public bool ValidateCaptcha(Models.Captcha captcha)
        {
            return captcha.EnteredText != null && captcha.EnteredText.Equals(CurrentCaptchaText);
        }


    }
}