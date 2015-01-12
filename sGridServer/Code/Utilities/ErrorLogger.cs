using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This class provides functionality to persistently log and retrieve errors. 
    /// </summary>
    public static class ErrorLogger
    {
        /// <summary>
        /// Returns all errors which were stored before. 
        /// </summary>
        /// <returns>An enumeration of all errors. </returns>
        public static IEnumerable<Error> GetErrors()
        {
            return new SGridDbContext().Errors.AsNoTracking();
        }

        /// <summary>
        /// Stores the given error.
        /// </summary>
        /// <param name="error">The error text.</param>
        /// <param name="stackTrace">The stack trace associated with the error.</param>
        public static void Log(string error, string stackTrace)
        {
            SGridDbContext context = new SGridDbContext();

            context.Errors.Add(new Error()
            {
                Stacktrace = stackTrace,
                Description = error,
                Timestamp = DateTime.Now
            });

            context.SaveChanges();
        }
    }
}