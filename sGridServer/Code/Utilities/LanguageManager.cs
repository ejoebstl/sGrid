using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using System.Threading;
using System.Globalization;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides a way to get or set the current language. 
    /// </summary>
    public static class LanguageManager
    {
        /// <summary>
        /// The session key of the current language.
        /// </summary>
        private const string CurrentLanguageKey = "CurrentLanguage";

        /// <summary>
        /// A concurrent bag containing all languages.
        /// </summary>
        private static ConcurrentBag<LanguageItem> languages;

        /// <summary>
        /// Initializes static fields.
        /// </summary>
        static LanguageManager()
        {
            languages = new ConcurrentBag<LanguageItem>();            
            
            //Register languages
            LanguageManager.RegisterLanguage(new LanguageItem("English", "en-US", "~/Content/images/flags/us.png"));
            LanguageManager.RegisterLanguage(new LanguageItem("Deutsch", "de-DE", "~/Content/images/flags/de.png"));

        }

        /// <summary>
        /// Registers the given language.
        /// </summary>
        /// <param name="language">The language to register.</param>
        public static void RegisterLanguage(LanguageItem language)
        {
            languages.Add(language);
        }

        /// <summary>
        /// Gets all Languages. 
        /// </summary>
        public static IEnumerable<LanguageItem> Languages
        {
            get
            {
                return languages;
            }
        }

        /// <summary>
        /// Gets a language by it's language code.
        /// </summary>
        /// <param name="code">The code to get the language for.</param>
        /// <returns>The language item associated with the given code, or null, if no such item is registered.</returns>
        public static LanguageItem LanguageByCode(string code)
        {
            return languages.Where(l => l.Code.ToLower().Contains(code.ToLower())).FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the current language. 
        /// </summary>
        /// <remarks>
        /// The language is stored in the session, and if applicable, also for the currently logged in user.
        /// </remarks>
        public static LanguageItem CurrentLanguage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        //First try: Retreive language from session.
                        if (HttpContext.Current.Session[CurrentLanguageKey] != null)
                        {
                            return (LanguageItem)HttpContext.Current.Session[CurrentLanguageKey];
                        }
                        else if (SecurityProvider.Context != null)
                        {
                            //Second try: If no language is registered in the session, retreive it from the database.
                            Account current = SecurityProvider.CurrentUser;
                            LanguageItem culture = LanguageByCode(current.Culture);
                            if (culture != null)
                            {
                                HttpContext.Current.Session[CurrentLanguageKey] = culture;
                                return (LanguageItem)HttpContext.Current.Session[CurrentLanguageKey];
                            }
                        }
                    }

                    //Third try: Retreive language from browser preferences. 
                    if (HttpContext.Current.Request.UserLanguages != null)
                    {
                        foreach (string languageCode in HttpContext.Current.Request.UserLanguages)
                        {
                            //Trim out "q=x" http header weighting values. 
                            string code = languageCode;

                            int index = code.IndexOf(';');
                            if (index != -1)
                            {
                                code = code.Substring(0, index);
                            }

                            //Try to get the language. 
                            LanguageItem item = LanguageByCode(code);

                            if (item != null)
                            {
                                return item;
                            }
                        }
                    }
                }

                //If neither database nor session, nor the request were able to retreive the language, use default.
                return languages.First();

            }
            set
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    throw new InvalidOperationException("Cannot set the current language without session or HTTP context");
                }

                //Store the language in the session
                HttpContext.Current.Session[CurrentLanguageKey] = value;

                if (SecurityProvider.Context != null)
                {
                    //If there is a user logged in, store the language setting also in the database. 
                    Account current = SecurityProvider.CurrentUser;

                    current.Culture = value.Code;
                    MemberManager manager = new MemberManager();

                    manager.SaveAccount(current);
                }

                SetThreadCulture();
            }
        }

        /// <summary>
        /// Applies the current language to the current thread.
        /// </summary>
        public static void SetThreadCulture()
        {
            string languageCode = CurrentLanguage.Code;
            SetThreadCulture(languageCode);
        }

        /// <summary>
        /// Applies the culture of the given account to the current thread. 
        /// </summary>
        /// <param name="a">The account to get the culture to apply from.</param>
        public static void SetThreadCulture(Account a)
        {
            string languageCode = a.Culture;
            SetThreadCulture(languageCode);
        }

        /// <summary>
        /// Applies the given culture to the current thread.
        /// </summary>
        /// <param name="languageCode">The language code of the culture to apply.</param>
        public static void SetThreadCulture(string languageCode)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
        }
    }

    /// <summary>
    /// This class represents a language item.
    /// </summary>
    [Serializable]
    public class LanguageItem
    {
        /// <summary>
        /// Gets the native name of the language.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Gets the code identifying the language.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets the url of the icon associated with this language.
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="name">The native name of the language.</param>
        /// <param name="code">Gets the code identifying the language.</param>
        /// <param name="iconUrl">Gets the url of the icon associated with this language.</param>
        public LanguageItem(string name, string code, string iconUrl)
        {
            this.Name = name;
            this.Code = code;
            this.IconUrl = iconUrl;
        }
    }
}