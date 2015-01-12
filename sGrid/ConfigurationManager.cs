using System;
using System.Collections.Generic;
using System.Text;
using sGrid.Properties;
using Boinc;

namespace sGrid
{
    /// <summary>
    /// This class is responsible for storing settings and user information persistently. 
    /// </summary>
    class ConfigurationManager
    {
        /// <summary>
        /// Gets the current configuration. 
        /// </summary>
        public Configuration CurrentConfiguration
        {
            get
            {
                return Settings.Default.Configuration;
            }
        }

        /// <summary>
        /// Gets the current user information. 
        /// </summary>
        public UserInformation CurrentUser
        {
            get
            {
                return Settings.Default.CurrentUser;
            }
        }

        /// <summary>
        /// Gets the short name of the project the client is
        /// currently calculating on.
        /// </summary>
        public string CurrentProjectShortName
        {
            get
            {
                return Settings.Default.CurrentProjectShortName;
            }
        }

        /// <summary>
        /// Gets the name of the project the client is
        /// currently calculating on.
        /// </summary>
        public string CurrentProjectName
        {
            get
            {
                return Settings.Default.CurrentProjectName;
            }
        }

        /// <summary>
        /// Stores the given user information persistently. 
        /// </summary>
        /// <param name="user">The user to remember. If this object is null, nothing is remembered and all stored user information is deleted. </param>
        public void RememberUser(UserInformation user)
        {
            Settings.Default.CurrentUser = user;
            Settings.Default.Save();

        }

        /// <summary>
        /// Stores the given configuration persistently.
        /// </summary>
        /// <param name="conf">The configuration to store. </param>
        public void SaveConfiguration(Configuration conf)
        {
            Settings.Default.Configuration = conf;
            Settings.Default.Save();
        }

        /// <summary>
        /// Saves the shortName of the Project the client is
        /// actually calculating on.
        /// </summary>
        /// <param name="shortName">The short name identifying
        /// the project.</param>
        /// <param name="name">The name of the project.</param>
        public void SaveProject(string shortName, string name)
        {
            Settings.Default.CurrentProjectShortName = shortName;
            Settings.Default.CurrentProjectName = name;
            Settings.Default.Save();
        }
    }
}
