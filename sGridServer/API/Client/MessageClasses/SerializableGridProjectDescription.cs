using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using sGridServer.Code.GridProviders;

namespace sGrid
{
    /// <summary>
    /// A serializeable copy of the GridProjectDescription class. 
    /// </summary>
    [DataContract]
    public class SerializableGridProjectDescription
    {
        /// <summary>
        /// Gets the human readable name of this grid project description.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a short info or slogan for this grid project description.
        /// </summary>
        [DataMember]
        public string ShortInfo { get; private set; }

        /// <summary>
        /// Gets the unique short name of this grid project description.
        /// </summary>
        [DataMember]
        public string ShortName { get; private set; }

        /// <summary>
        /// Gets the url pointing to the website of this grid project description.
        /// </summary>
        [DataMember]
        public string WebsiteUrl { get; private set; }

        /// <summary>
        /// Gets the url pointing to the workspace of this grid project description.
        /// </summary>
        [DataMember]
        public string WorkspaceUrl { get; private set; }

        /// <summary>
        /// Creats a new instance of this class.
        /// </summary>
        /// <param name="project">The GridProjectDescription to copy the properties from.</param>
        public SerializableGridProjectDescription(GridProjectDescription project)
        {
            this.Name = project.Name;
            this.ShortInfo = project.ShortInfo;
            this.ShortName = project.ShortName;
            this.WebsiteUrl = project.WebsiteUrl;
            this.WorkspaceUrl = project.WorkspaceUrl;
        }
    }
}