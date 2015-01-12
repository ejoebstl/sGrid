using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace sGrid
{
    /// <summary>
    /// A message that is sent from the client to the server after
    /// a package was finished. 
    /// </summary>
    [DataContract]
    public class StatusMessage
    {
        /// <summary>
        /// Gets the authentication data of the user.
        /// </summary>
        [DataMember]
        public Credential Authentication { get; private set; }

        /// <summary>
        /// Gets the String identifying the project the client was
        /// calculating.
        /// </summary>
        [DataMember]
        public String ProjectShortName { get; private set; }

        /// <summary>
        /// Gets the time, when the client has started calculating
        /// the now finished package.
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the
        /// given parameters into the corresponding properties.
        /// </summary>
        /// <param name="authentication">The authentication data of
        /// the user.</param>
        /// <param name="projectShortName">The String identifying
        /// the project the client was calculating.</param>
        /// <param name="maxUptime">The longest time span the
        /// client has been calculating without any interruption.</param>
        public StatusMessage(Credential authentication,
            String projectShortName)
        {
            this.Authentication = authentication;
            this.ProjectShortName = projectShortName;
        }
    }
}