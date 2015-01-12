using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Security.Policy;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// Provides functions to resize images.
    /// </summary>
    public class ImageUtil
    {
        /// <summary>
        /// Resizes an image from a stream to fit a given constraint.
        /// </summary>
        /// <param name="stream">The stream to get the image from.</param>
        /// <param name="width">The width constraint.</param>
        /// <param name="height">The height constraint.</param>
        /// <param name="backColor">The color to fill any unused image parts with.</param>
        /// <param name="minWidth">The minimum width of the image. If the image is smaller than this parameter, an exception is thrown.</param>
        /// <param name="minHeight">The minimum height of the image. If the image is smaller than this parameter, an exception is thrown.</param>
        /// <param name="maxWidth">The maximum height of the image. If the image is larger than this parameter, an exception is thrown.</param>
        /// <param name="maxHeight">The maximum height of the image. If the image is larger than this parameter, an exception is thrown.</param>
        /// <returns>The resized image, saved into the stream, in JPEG format.</returns>
        public static Stream ResizeImage(Stream stream, int width, int height, Color backColor, int minWidth = 0, int minHeight = 0, int maxWidth = Int32.MaxValue, int maxHeight = Int32.MaxValue)
        {
            //Load the image.
            Image img = Image.FromStream(stream);

            //Check minimum and maximum width and height. 
            if (img.Width < minWidth || img.Height < minHeight)
            {
                throw new ArgumentException("The given image was too small.");
            }

            if (img.Width > maxWidth || img.Height > maxHeight)
            {
                throw new ArgumentException("The given image was too large.");
            }

            //Resize the image. 
            img = ResizeImage(img, width, height, backColor);

            //Save the image to the output stream. 
            MemoryStream outStream = new MemoryStream();

            //Make the quality a little better than default. 
            ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters parameters = new EncoderParameters(1);
            EncoderParameter quality = new EncoderParameter(Encoder.Quality, 90L);
            parameters.Param[0] = quality;

            img.Save(outStream, jpegEncoder, parameters);
            outStream.Position = 0; //Don't forget to "rewind" the stream, so it can be read again. 

            return outStream;
        }

        /// <summary>
        /// Resizes a given image to fit the given constraints.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width constraint.</param>
        /// <param name="height">The height constraint.</param>
        /// <param name="backColor">The color to fill any unused image parts with.</param>
        /// <returns>The resized image.</returns>
        public static Image ResizeImage(Image image, int width, int height, Color backColor)
        {
            //First, calculate the now size of the image, so the image can be resized without disortion. 
            Size size = new Size(width, height);

            if ((image.Width * size.Height) > (image.Height * size.Width))
            {
                if (image.Width > size.Width)
                {
                    width = size.Width;
                }
                else
                {
                    width = image.Width;
                }
                height = (int)(((float)image.Height) / ((float)image.Width / (float)width));
            }
            else
            {
                if (image.Height < size.Height)
                {
                    height = image.Height;
                }
                else
                {
                    height = size.Height;
                }
                width = (int)(((float)image.Width) / ((float)image.Height / (float)height));
            }

            //Create a target image for drawing onto and gather the graphics context. 
            Bitmap bitmap = new Bitmap(size.Width, size.Height);
            Graphics gfx = Graphics.FromImage(bitmap);

            //Use high quality settings for drawing. 
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            //Fill the target image with background color. 
            gfx.FillRectangle(new Pen(backColor).Brush, 0, 0, size.Width, size.Height);
            
            //Draw the source image onto the target, using the new size. 
            gfx.DrawImage(image, (size.Width - width) / 2, (size.Height - height) / 2, width, height);
            
            //Dispose the graphics context. 
            gfx.Dispose();

            //Return resized image. 
            return bitmap;
        }

        /// <summary>
        /// Downloads the image located by the given url into the blob storage and returns the storage url. 
        /// </summary>
        /// <returns>The url to the stored picture in the blob storage.</returns>
        public static string GetPicturyByUrl(string url, int width, int height, string container)
        {
            WebClient httpClient = new WebClient();

            BlobStorage storage = new BlobStorage(container);

            string name = storage.StoreBlob(ImageUtil.ResizeImage(httpClient.OpenRead(url),
                width,
                height,
                System.Drawing.Color.White));

            httpClient.Dispose();

            return name;
        }

        /// <summary>
        /// Gets the image encoder by format. 
        /// </summary>
        /// <param name="format">The format to get the encoder for.</param>
        /// <returns>The corresponding image encoder.</returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}