using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace sGrid
{
    /// <summary>
    /// This class contains the description of a project and the
    /// data which is necessary for the authentication of the user
    /// at the grid provider.
    /// </summary>
    [DataContract]
    public class ProjectData
    {
        /// <summary>
        /// Gets the description of the project with all the
        /// necessary data.
        /// </summary>
        [DataMember]
        public SerializableGridProjectDescription GridProject { get; private set; }

        /// <summary>
        /// Gets the authentication token the client will use to
        /// identify itself at the grid provider.
        /// </summary>
        [DataMember]
        public String GridToken { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the
        /// given parameters into the corresponding properties.
        /// </summary>
        /// <param name="gridProject">The description of the
        /// project with all the necessary data.</param>
        /// <param name="gridToken">The authentication token the
        /// client will use to identify itself at the grid provider.</param>
        public ProjectData(SerializableGridProjectDescription gridProject, String gridToken)
        {
            this.GridProject = gridProject;
            this.GridToken = gridToken;
        }
    }
}