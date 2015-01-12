using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// The GridPerformanceData class represents a performance snapshot for a 
    /// grid provider or for a single user of a grid provider.
    /// </summary>
    public class GridPerformanceData
    {
        /// <summary>
        /// Gets the count of devices. 
        /// </summary>
        public int DeviceCount { get; private set; }

        /// <summary>
        /// Gets the grid provider which is associated with this object. 
        /// </summary>
        public GridProviderDescription GridProvider { get; private set; }

        /// <summary>
        /// Gets the count of submitted results. 
        /// </summary>
        public int ResultCount { get; private set; }

        /// <summary>
        /// Gets the time the last result was submitted.
        /// </summary>
        public DateTime TimeOfLastResult { get; private set; }

        /// <summary>
        /// Gets the total runtime in seconds.
        /// </summary>
        public int TotalRuntime { get; private set; }

        /// <summary>
        /// Gets the user associated with this performance object.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the 
        /// parameters into their corresponding properties.
        /// </summary>
        /// <param name="deviceCount">The count of devices. </param>
        /// <param name="gridProvider">The grid provider which is associated with this object. </param>
        /// <param name="resultCount">The count of submitted results. </param>
        /// <param name="timeOfLastResult">The time the last result was submitted.</param>
        /// <param name="totalRuntime">The total runtime in seconds.</param>
        /// <param name="user">The user associated with this performance object.</param>
        public GridPerformanceData(int deviceCount, GridProviderDescription gridProvider, 
            int resultCount, DateTime timeOfLastResult, int totalRuntime, User user) {
            
            this.DeviceCount = deviceCount;
            this.GridProvider = gridProvider;
            this.ResultCount = resultCount;
            this.TimeOfLastResult = timeOfLastResult;
            this.TotalRuntime = totalRuntime;
            this.User = user;
        }

    }
}