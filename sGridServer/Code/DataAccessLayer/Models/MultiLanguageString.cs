using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.ComponentModel.DataAnnotations.Schema;
using sGridServer.Code.Utilities;
using System.Linq.Expressions;

namespace sGridServer.Code.DataAccessLayer.Models
{
    /// <summary>
    /// Represents a string which can be translated in different languages.
    /// </summary>
    /// <remarks>
    /// If a MultiLanguageString is edited with MVC, it should be initialized with an empty string if it is unset before 
    /// showing the corresponding editor - doing else will break database relations and render the multi-language functionality
    /// of the object unfunctional. 
    /// </remarks>
    public class MultiLanguageString
    {
        /// <summary>
        /// Gets or sets the identifier of this language string. 
        /// </summary>
        [Key]
        public int Id {get; set;}

        /// <summary>
        /// Gets the translations of this string. 
        /// </summary>
        public virtual List<Translation> Translations { get; set; }

        /// <summary>
        /// Gets the value of this string matching the culture of the current thread.
        /// </summary>
        [NotMapped]
        public string Text
        {
            get
            {
                //Fetch the translation from the DB
                string value = (from text in Translations
                       where text.Culture == Thread.CurrentThread.CurrentCulture.Name
                       select text.Text).FirstOrDefault();

                
                //If nothing is found, fallback to english.
                if (value == null)
                {
                    value = (from text in Translations where text.Culture == "en-US" select text.Text).FirstOrDefault();
                }

                //If still nothing is found, panic and fallback to a default string. 
                if (value == null)
                {
                    value = "(Translation missing)";
                }

                return value;
            }
        }

        /// <summary>
        /// Returns the value of the Text property of the given MultiLanguageString object, as string.
        /// </summary>
        /// <param name="mls">The MultiLanguageString object to cast.</param>
        /// <returns>The text of the given MultiLanguageString object.</returns>
        public static implicit operator string(MultiLanguageString mls)
        {
            return mls.Text;
        }

        /// <summary>
        /// Creates a new MultiLanguageString object from a given string, setting all translations for all known
        /// values to the given string. 
        /// </summary>
        /// <param name="text">The string to create the new MultiLanguageString from.</param>
        /// <returns>A new MultiLanguageString object.</returns>
        public static implicit operator MultiLanguageString(string text)
        {
            MultiLanguageString newMultiString = new MultiLanguageString();

            foreach (LanguageItem language in LanguageManager.Languages)
            {
                newMultiString.Add(language.Code, text);
            }

            return newMultiString;
        }
        
        /// <summary>
        /// Checks whether a given object equals this instance. 
        /// If obj is of type string, the given string is compared to the Text property.
        /// Else, the object references are compared.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>A bool indicating whether the given object is equal to this.</returns>
        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return Text.Equals((string)obj);
            }
            else if (obj is MultiLanguageString)
            {
                return base.Equals(obj);
            }

            return false;
        }

        /// <summary>
        /// Calls base.GetHashCode().
        /// </summary>
        /// <returns>Returns base.GetHashCode().</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Checks whether a given string equals the Text property of a given MultiLanguageString object.
        /// </summary>
        /// <param name="a">The string to compare.</param>
        /// <param name="b">The MultiLanguageString object to compare.</param>
        /// <returns>A bool indicating whether the two given objects are equal.</returns>
        public static bool operator ==(string a, MultiLanguageString b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return b.Equals(a);
        }

        /// <summary>
        /// Checks whether a given string equals the Text property of a given MultiLanguageString object.
        /// </summary>
        /// <param name="a">The MultiLanguageString object to compare.</param>
        /// <param name="b">The string to compare.</param>
        /// <returns>A bool indicating whether the two given objects are equal.</returns>
        public static bool operator ==(MultiLanguageString a, string b)
        {
            return b == a;
        }

        /// <summary>
        /// Checks whether a given string does not equal the Text property of a given MultiLanguageString object.
        /// </summary>
        /// <param name="a">The string to compare.</param>
        /// <param name="b">The MultiLanguageString object to compare.</param>
        /// <returns>A bool indicating whether the two given objects are different.</returns>
        public static bool operator !=(string a, MultiLanguageString b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Checks whether a given string does not equal the Text property of a given MultiLanguageString object.
        /// </summary>
        /// <param name="a">The MultiLanguageString object to compare.</param>
        /// <param name="b">The string to compare.</param>
        /// <returns>A bool indicating whether the two given objects are different.</returns>
        public static bool operator !=(MultiLanguageString a, string b)
        {
            return b != a;
        }
        
        /// <summary>
        /// Returns the Text property of this MultiLanguageString object.
        /// </summary>
        /// <returns>The text, as string.</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public MultiLanguageString()
        {
            this.Id = -1;
            this.Translations = new List<Translation>();
        }

        /// <summary>
        /// Adds a translation to this instance's translation list. 
        /// </summary>
        /// <param name="languageCode">The language code associated with this translation.</param>
        /// <param name="text">The translated text.</param>
        public void Add(string languageCode, string text)
        {
            Translations.Add(new Translation() { Culture = languageCode, Text = text });
        }
    }

    /// <summary>
    /// This class provides static helpers for the MultiLanguageString. 
    /// </summary>
    public static class MultiLanguageStringExtensionMethods
    {
        /// <summary>
        /// Inserts empty translations for all languages, which are not yet translated.
        /// </summary>
        /// <param name="multiString">The MultiLanguageString object to insert the translations to.</param>
        /// <param name="text">The string to use as text for the new translations.</param>
        public static void InsertMissingTranslations(this MultiLanguageString multiString, string text = "")
        {
            foreach (LanguageItem language in LanguageManager.Languages)
            {
                if (!multiString.Translations.Where(x => x.Culture == language.Code).Any())
                {
                    multiString.Translations.Add(new Translation() { Culture = language.Code, Text = text });
                }
            }
        }

        /// <summary>
        /// Reattaches a detached MultiLanguageString object to the database context. 
        /// </summary>
        /// <param name="multiString">The MultiLanguageString object to reattach.</param>
        /// <param name="context">The database context to attach the MultiLanguageString object to.</param>
        public static void Reattach(this MultiLanguageString multiString, SGridDbContext context)
        {
            //Attach the MultiLanguageString object itself.
            MultiLanguageString stringToSave = context.Strings.Where(x => x.Id == multiString.Id).FirstOrDefault();

            if (stringToSave != (string)null && stringToSave.Id != -1)
            {
                context.Entry(stringToSave).CurrentValues.SetValues(multiString);
            }
            else
            {
                context.Strings.Add(multiString);
            }

            //Attach all Translations of the MultiLanguageString object. 
            foreach (Translation trans in multiString.Translations)
            {
                Translation translationToSave = context.Translations.Where(x => x.Id == trans.Id).FirstOrDefault();

                if (translationToSave != null && trans.Id != -1)
                {
                    context.Entry(translationToSave).CurrentValues.SetValues(trans);
                }
                else
                {
                    context.Translations.Add(trans);
                }
            }
        }

        /// <summary>
        /// Removes a MultiLanguageString object and all its translations from the database context.
        /// </summary>
        /// <param name="multiString">The MultiLanguageString to remove.</param>
        /// <param name="context">The database context to remove the MultiLanguageString object from.</param>
        public static void Remove(this MultiLanguageString multiString, SGridDbContext context)
        {
            //Delete the MultiLanguageString object.
            MultiLanguageString stringToDelete = context.Strings.Where(x => x.Id == multiString.Id).FirstOrDefault();

            if (stringToDelete != (string)null)
            {
                context.Strings.Remove(stringToDelete);
            }

            //Delete all translations. 
            foreach (Translation trans in multiString.Translations)
            {
                Translation translationToDelete = context.Translations.Where(x => x.Id == trans.Id).FirstOrDefault();

                if (translationToDelete != null)
                {
                    context.Translations.Remove(translationToDelete);
                }
            }
        }
    }
}