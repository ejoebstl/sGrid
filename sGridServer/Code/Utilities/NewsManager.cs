using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides methods to store, edit and retrieve news. 
    /// </summary>
    public class NewsManager
    {
        private SGridDbContext context;

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public NewsManager()
        {
            context = new SGridDbContext();
        }

        /// <summary>
        /// Deletes the given news object.
        /// </summary>
        /// <param name="news">The news item to delete.</param>
        public void DeleteNews(News news)
        {
            News toDelete = (from n in context.News
                           where n.Id == news.Id
                           select n).First();

            context.News.Remove(toDelete);

            context.SaveChanges();
        }

        /// <summary>
        /// Gets the most recent news item. 
        /// </summary>
        /// <returns>The most recent news item.</returns>
        public News GetLatestNews()
        {
            return context.News.AsNoTracking().OrderByDescending(x => x.Timestamp).FirstOrDefault();
        }

        /// <summary>
        /// Gets all news items.
        /// </summary>
        /// <returns>An enumeration of all news items.</returns>
        public IEnumerable<News> GetNews()
        {
            return context.News.AsNoTracking();
        }

        /// <summary>
        /// Saves the given news object. If the news item already exists, 
        /// only the changes are saved. Else, a new news item is created. 
        /// </summary>
        /// <param name="news">The news item to save.</param>
        public void SaveNews(News news)
        {
            if (news.Id == -1) //We know - by convention - that News objects with id -1 are not stored yet. 
            {
                context.News.Add(news);

                news.Subject.Reattach(context);
                news.Text.Reattach(context);

                context.SaveChanges();
            }
            else
            {
                News toSave = (from n in context.News
                                  where n.Id == news.Id
                                  select n).First();

                context.Entry(toSave).CurrentValues.SetValues(news);

                news.Subject.Reattach(context);
                news.Text.Reattach(context);

                context.SaveChanges();
            }
        }
    }
}