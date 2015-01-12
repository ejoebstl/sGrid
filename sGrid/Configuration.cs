using System;
using System.Collections.Generic;
using System.Text;

namespace sGrid
{
    /// <summary>
    /// This class holds the client configuration. 
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets a bool indicating whether the client should start automatically.
        /// </summary>
        public bool Autostart { get; set; }

        /// <summary>
        /// Gets or sets a bool indicating whether the client should run even if the computer runs on battery. 
        /// </summary>
        public bool RunOnBattery { get; set; }
        
        /// <summary>
        /// Gets or sets a bool indicating whether the client should run even if the computer runs on energy saver mode. 
        /// </summary>
        public bool RunOnEnergySaver { get; set; }
        
        /// <summary>
        /// Gets or sets a bool indicating whether the GPU can be used for calculations. 
        /// </summary>
        public bool UseGpu { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage limit in percent. 
        /// </summary>
        public double CpuUsageLimit { get; set; }
        
        /// <summary>
        /// Gets or sets the disk space limit in megabyte. 
        /// </summary>
        public int DiskSpaceLimit { get; set; }
        
        /// <summary>
        /// Gets or sets the count of processor cores to use. 
        /// </summary>
        public int ProcessorCoresInUse { get; set; }
    }
}
