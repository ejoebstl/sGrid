using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// The IdProviderManager class provides a way for 
    /// registering all available id provider classes at 
    /// startup and also provides an enumeration of them to other modules.
    /// </summary>
    public static class IdProviderManager
    {
        /// <summary>
        /// Private static concurrent collection for holding IdProviderDescription objects.
        /// </summary>
        private static ConcurrentBag<IdProviderDescription> descriptionList;

        /// <summary>
        /// Static constructor which initializes static fields. 
        /// </summary>
        static IdProviderManager() {
            descriptionList = new ConcurrentBag<IdProviderDescription>();
        }

        /// <summary>
        /// Returns an enumeration of all registered providers.
        /// </summary>
        /// <returns>An enumeration of all registered providers.</returns>
        public static IEnumerable<IdProviderDescription> GetRegisteredProviders()
        {
            return descriptionList.ToArray();
        }

        /// <summary>
        /// Registers the given IdProviderDescription with the IdProviderManager. 
        /// </summary>
        /// <param name="idProvider">The description of the IdProvider to register.</param>
        public static void RegisterIdProvider(IdProviderDescription idProvider)
        {
            descriptionList.Add(idProvider);
        }
    }
}