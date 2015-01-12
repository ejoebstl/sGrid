using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides functions to store and retrieve messages and to mark them as resolved. 
    /// </summary>
    public class MessageManager
    { 
        private SGridDbContext context;

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        public MessageManager()
        {
            context = new SGridDbContext();
        }

        /// <summary>
        /// Stores the given message. 
        /// </summary>
        /// <param name="message">The message to store. </param>
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);

            context.SaveChanges();
        }

        /// <summary>
        /// Retrieves all messages. 
        /// </summary>
        /// <returns>An enumeration of all messages.</returns>
        public IEnumerable<Message> GetMessages()
        {
            return context.Messages.AsNoTracking();
        }

        /// <summary>
        /// Marks the given message as resolved. 
        /// </summary>
        /// <param name="message">The message to mark as resolved. </param>
        public void MarkMessageAsResolved(Message message)
        {
            Message toMark = (from m in context.Messages
                              where m.Id == message.Id
                              select m).First();

            toMark.Resolved = true;

            context.SaveChanges();
        }
    }
}