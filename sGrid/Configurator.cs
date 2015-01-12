using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using Boinc;

namespace sGrid
{
    /// <summary>
    /// This class is responsible for applying application settings. 
    /// </summary>
    public class Configurator
    {
        private const string AutostartKey = "sGridClient";

        /// <summary>
        /// A reference to the BoincCore instance.
        /// </summary>
        private BoincClient boinc;

        /// <summary>
        /// Creates a new instance of this class and stores a reference to the given BoincCore instance. 
        /// </summary>
        /// <param name="boinc">The BoincCore instance to store. </param>
        public Configurator(BoincClient boinc)
        {
            if (boinc == null)
            {
                throw new ArgumentException("Parameter BoincClient boinc must not be null.");
            }
            this.boinc = boinc;
        }

        /// <summary>
        /// Applies the given configuration concerning the autostart
        /// to the application.
        /// </summary>
        /// <param name="configuration">The configuration to apply.</param>
        public void ApplyAutostart(Configuration configuration)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (configuration.Autostart)
            {
                //If autostart key does not exist, add it.
                if (key.GetValue(AutostartKey) == null)
                {
                    key.SetValue(AutostartKey, Assembly.GetExecutingAssembly().Location);
                }
            }
            else
            {
                //If autostart key does exist, remove it it.
                if (key.GetValue(AutostartKey) != null)
                {
                    key.DeleteValue(AutostartKey);
                }
            }
        }

        /// <summary>
        /// Applies the given configuration to the application.
        /// </summary>
        /// <param name="configuration">The configuration to apply. </param>
        public void ApplyConfiguration(Configuration configuration)
        {
            ApplyAutostart(configuration);

            ApplyBoincConfiguration(configuration);
        }

        /// <summary>
        /// Applies the given configuration to the BoincCore instance.
        /// </summary>
        /// <param name="configuration">The configuration to apply. </param>
        private void ApplyBoincConfiguration(Configuration configuration)
        {
            Preferences prefs = new Preferences(configuration.RunOnBattery, 
                configuration.UseGpu, 
                (float)configuration.CpuUsageLimit, 
                (float)configuration.DiskSpaceLimit / 1000.0f, 
                configuration.ProcessorCoresInUse);

            boinc.SetPreferences(prefs);
        }
    }
}
