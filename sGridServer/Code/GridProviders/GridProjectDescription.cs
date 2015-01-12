using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// This class describes a grid project. 
    /// </summary>
    public class GridProjectDescription
    {
        /// <summary>
        /// Gets the average time, in minutes, which 
        /// is needed by clients to calculate one result for this project. 
        /// </summary>
        public int AverageCalculationTime { get; private set; }

        /// <summary>
        /// Gets the number of coins a user gets when submitting a result.
        /// </summary>
        public int CoinsPerResult { get; private set; }

        /// <summary>
        /// Gets the description of this grid project description.
        /// </summary>
        public MultiLanguageString Description { get; private set; }

        /// <summary>
        /// Gets the url of the icon associated with this grid project description.
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets the human readable name of this grid project description.
        /// </summary>
        public MultiLanguageString Name { get; private set; }

        /// <summary>
        /// Gets a short info or slogan for this grid project description.
        /// </summary>
        public MultiLanguageString ShortInfo { get; private set; }

        /// <summary>
        /// Gets the unique short name of this grid project description.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Gets the url pointing to the website of this grid project description.
        /// </summary>
        public string WebsiteUrl { get; private set; }

        /// <summary>
        /// Gets the url pointing to the workspace of this grid project description.
        /// </summary>
        public string WorkspaceUrl { get; private set; }

        /// <summary>
        /// Gets the provider associated with this project description.
        /// </summary>
        public GridProviderDescription Provider {get; internal set;}

        /// <summary>
        /// Creates a new instance of this class and stores all given 
        /// parameters into the corresponding properties. 
        /// </summary>
        /// <param name="shortName">The unique short name of this grid project description.</param>
        /// <param name="description">A description of this grid project description.</param>
        /// <param name="name">The human readable name of this grid project description.</param>
        /// <param name="iconUrl">The url of the icon associated with this grid project description.</param>
        /// <param name="shortInfo">A short info or slogan for this grid project description.</param>
        /// <param name="websiteUrl">The url pointing to the website of this grid project description.</param>
        /// <param name="workspaceUrl">The url pointing to the workspace of this grid project description.</param>
        /// <param name="coinsPerResult">The number of coins a user gets for submitting a result.</param>
        /// <param name="averageCalculationTime">The average time, in minutes, which is needed by clients to calculate one result for this project. </param>
        public GridProjectDescription(string shortName, MultiLanguageString description, MultiLanguageString name, string iconUrl,
            MultiLanguageString shortInfo, string websiteUrl, string workspaceUrl, int coinsPerResult, int averageCalculationTime)
        {
            this.AverageCalculationTime = averageCalculationTime;
            this.CoinsPerResult = coinsPerResult;
            this.Description = description;
            this.IconUrl = iconUrl;
            this.Name = name;
            this.ShortInfo = shortInfo;
            this.ShortName = shortName;
            this.WebsiteUrl = websiteUrl;
            this.WorkspaceUrl = workspaceUrl;
            this.Provider = null;
        }
    }
}