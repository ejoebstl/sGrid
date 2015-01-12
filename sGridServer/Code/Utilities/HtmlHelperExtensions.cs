using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Resource = sGridServer.Resources.Utilities.HtmlHelperExtensions;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides useful extension methods for rendering MVC views. 
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Url template for the facebook share button. 
        /// </summary>
        private const string ShareButtonUrl = "https://www.facebook.com/dialog/feed?app_id={0}&link={1}&picture={2}&name={3}&caption={4}&description={5}&redirect_uri={6}";

        /// <summary>
        /// Generates a share url for facebook using the given parameters. 
        /// </summary>
        /// <param name="html">The HtmlHelper to use for this operation.</param>
        /// <param name="url">The url associated with the shared post. </param>
        /// <param name="imageUrl">The url of an image shown on the shared post. </param>
        /// <param name="caption">The caption of the shared post. </param>
        /// <param name="name">The name of the shared post. </param>
        /// <param name="text">The text of the shared post. </param>
        /// <returns>The rendered button as System.Web.Mvc.MvcHtmlString object.</returns>
        public static MvcHtmlString ShareButton(this HtmlHelper html, string url, string imageUrl, string caption, string name, string text) 
        {
            string shareUrl = String.Format(ShareButtonUrl,
                Properties.Settings.Default.FacebookAppId,
                HttpUtility.UrlEncode(url),
                HttpUtility.UrlEncode(imageUrl),
                HttpUtility.UrlEncode(name),
                HttpUtility.UrlEncode(caption),
                HttpUtility.UrlEncode(text),
                HttpUtility.UrlEncode(html.ViewContext.HttpContext.Request.Url.ToString()));

            string shareButton = String.Format("<a href=\"{0}\">{1}</a>", shareUrl, "<img src='"+ UrlHelper.GenerateContentUrl("~/Content/images/buttons/share.png", html.ViewContext.HttpContext) +"' style='border: none'/>");

            return new MvcHtmlString(shareButton);
        }

        /// <summary>
        /// Generates a drop down list for the given enum type. 
        /// </summary>       
        /// <param name="helper">The HtmlHelper to use for this operation.</param>
        /// <param name="name">The name of the HTML input element to generate.</param>
        /// <param name="type">The type of the enum to generate the drop down list for.</param>
        /// <param name="selected">The value which is currently selected.</param>
        /// <returns>The rendered drop down lost as System.Web.Mvc.MvcHtmlString object.</returns>
        public static MvcHtmlString DropDownList(this HtmlHelper helper, string name, Type type, object selected)
        {
            //Type checks
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type is not an enum.");
            }

            if (selected != null && selected.GetType() != type)
            {
                throw new ArgumentException("Selected object is not " + type.ToString());
            }

            //Generate a drop down list using the enum momebers. 
            List<SelectListItem> enums = new List<SelectListItem>();
            foreach (int value in Enum.GetValues(type))
            {
                SelectListItem item = new SelectListItem();
                item.Value = value.ToString();
                item.Text = Enum.GetName(type, value);

                if (selected != null)
                {
                    item.Selected = (int)selected == value;
                }

                enums.Add(item);
            }

            return System.Web.Mvc.Html.SelectExtensions.DropDownList(helper, name, enums, "--Select--");
        }

        /// <summary>
        /// Generates the display for a Timespan object. 
        /// </summary>
        /// <param name="helper">The HtmlHelper to use for this operation.</param>
        /// <param name="timespan">The Timespan object to render.</param>
        /// <returns>The Timespan as a String object.</returns>
        public static MvcHtmlString TimeSpan(this HtmlHelper helper, TimeSpan timespan)
        {
            StringBuilder builder = new StringBuilder();

            //if 0 minutes return "0"
            if ((int)timespan.TotalMinutes == 0)
            {
                builder.Append("0");
            }

            //Add amount of days
            if ((int)timespan.Days != 0)
            {
                builder.Append(timespan.Days + " " + Resource.DaysText + ", ");
            }

            //Add amount of hours
            if ((int)timespan.Hours != 0)
            {
                builder.Append(timespan.Hours + " " + Resource.HoursText + ", ");
            }

            //Add amount of minutes
            if ((int)timespan.Minutes != 0)
            {
                builder.Append(timespan.Minutes + " " + Resource.MinutesText + ", ");
            }

            if (builder.Length >= 2)
            {
                builder.Length -= 2;
            }
            return new MvcHtmlString(builder.ToString());
        }

        /// <summary>
        /// Converts an array into its json representation, using nested javascript arrays.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="helper">The HtmlHelper to use for this operation.</param>
        /// <param name="vals">The array to convert.</param>
        /// <returns>The encoded JSON string.</returns>
        public static MvcHtmlString JsonArray<T>(this HtmlHelper helper, IEnumerable<T> vals)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("[");

            //Append all values, comma seperated.
            foreach (T val in vals)
            {
                builder.Append(val.ToString());
                builder.Append(",");
            }

            builder.Length = builder.Length - 1;

            builder.Append("]");

            return new MvcHtmlString(builder.ToString());
        }

        /// <summary>
        /// Converts an array of tuples into their json representation, using nested javascript arrays.
        /// </summary>
        /// <param name="helper">The HtmlHelper to use for this operation.</param>
        /// <param name="vals">The array to convert.</param>
        /// <returns>The encoded JSON string.</returns>
        public static MvcHtmlString JsonArray(this HtmlHelper helper, IEnumerable<Tuple<DateTime, int>> vals)
        {
            StringBuilder builder = new StringBuilder();
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Append("[");

            foreach (Tuple<DateTime, int> val in vals)
            {
                builder.Append("[");
                builder.Append((Int64)((val.Item1 - epoch).TotalMilliseconds));
                builder.Append(",");
                builder.Append(val.Item2);
                builder.Append("]");
                builder.Append(",");
            }
            builder.Length = builder.Length - 1;

            builder.Append("]");

            return new MvcHtmlString(builder.ToString());
        }
    }
}
