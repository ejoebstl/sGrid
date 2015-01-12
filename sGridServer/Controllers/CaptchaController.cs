using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using sGridServer.Code.Utilities;
using System.IO;
using System.Drawing.Imaging;
using sGridServer.Models;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for generating the captcha image. 
    /// </summary>
    public class CaptchaController : Controller
    {
        /// <summary>
        /// Gets the height of the captcha image.
        /// </summary>
        public static int CaptchaWidth
        {
            get { return Properties.Settings.Default.CaptchaWidth; }
        }

        /// <summary>
        /// Gets the width of the captcha image.
        /// </summary>
        public static int CaptchaHeight
        {
            get { return Properties.Settings.Default.CaptchaHeight; }
        }

        /// <summary>
        /// Generates a captcha image by looking up the captcha text from the session using the captcha code. 
        /// The image is returned as file result.
        /// </summary>
        /// <param name="captchaCode">The code of the captcha to generate. </param>
        /// <returns>The generated captcha as file result.</returns>
        public FileResult GetCaptcha(string captchaCode)
        {
            //Create a new bitmap and initialize drawing tools
            Bitmap bmp = new Bitmap(CaptchaWidth, CaptchaHeight);

            CaptchaGenerator generator = new CaptchaGenerator(this.ControllerContext, captchaCode);

            string captchaText = generator.CurrentCaptchaText;
            float charWidth = (CaptchaWidth / (captchaText.Length + 1));
            Random rand = new Random();
            Font captchaFont = new Font(FontFamily.GenericMonospace, 15.0f);

            Graphics gfx = Graphics.FromImage(bmp);
            
            //Fill the background
            gfx.FillRectangle(Brushes.White, 0, 0, CaptchaWidth, CaptchaHeight);

            //Draw all letters of the captcha.
            for(int i = 0; i < captchaText.Length; i++) {
                //Create a random color
                Color color = Color.FromArgb(rand.Next(0, 200), rand.Next(0, 200), rand.Next(0, 200));
                //Draw a letter in random height
                gfx.DrawString(captchaText[i].ToString(), captchaFont, new Pen(color).Brush, i * charWidth, rand.Next(0, CaptchaHeight - captchaFont.Height));
            }

            //Draw random circles over the drawn text.
            for (int i = 0; i < 20; i++)
            {
                Color color = Color.FromArgb(rand.Next(150, 255), rand.Next(150, 255), rand.Next(150, 255));
                int radius = rand.Next(2, 20);
                gfx.DrawEllipse(new Pen(color), rand.Next(0, CaptchaWidth) - radius, rand.Next(0, CaptchaHeight) - radius, radius * 2, radius * 2);
            }

            //Save and send the image. 
            MemoryStream stream = new MemoryStream();

            bmp.Save(stream, ImageFormat.Png);

            return File(stream.ToArray(), "image/png");
        }

        /// <summary>
        /// A test method which generates a captcha. 
        /// TODO: Delete when finished. 
        /// </summary>
        /// <returns>A view.</returns>
        public ViewResult CaptchaTest()
        {
            CaptchaGenerator generator = new CaptchaGenerator(this.ControllerContext, "CaptchaTest");
            ViewBag.Message = "Enter captcha.";
            return View(generator.CreateCaptcha());
        }

        /// <summary>
        /// A test method which validates a captcha.
        /// TODO: Delete when finished. 
        /// </summary>
        /// <returns>A view.</returns>
        public ViewResult CaptchaValidate(Captcha captcha)
        {
            CaptchaGenerator generator = new CaptchaGenerator(this.ControllerContext, "CaptchaTest");
            if (generator.ValidateCaptcha(captcha))
            {
                ViewBag.Message = "Captcha correct.";
            }
            else
            {
                ViewBag.Message = "Captcha not correct.";
            }
            return View("CaptchaTest", generator.CreateCaptcha());
        }
    }
}
