// This source file is part of BoincGuiRpc.Net
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://boincguirpc.codeplex.com
//
// Licensed under the MIT License
//
// (c) 2012-2013

using System;
using System.Collections.Generic;
using System.Text;

namespace Boinc
{
    /// <summary>
    /// This class represents Boinc preferences. 
    /// </summary>
    public class Preferences
    {
        /// <summary>
        /// Gets a bool indicating whether Boinc should run if the computer
        /// runs on batteries. 
        /// </summary>
        public bool RunOnBatteries { get; private set; }

        /// <summary>
        /// Gets a bool indicating whether Boinc should use the gpu for calculations
        /// even if the user is active.
        /// </summary>
        public bool UseGpuIfUserIsActive { get; private set; }

        /// <summary>
        /// Gets a float number indicating the cpu usage limit in percent. 
        /// </summary>
        public float CpuUsageLimit { get; private set; }

        /// <summary>
        /// Gets a float indicating the disk usage limit in gigabyte. 
        /// </summary>
        public float DiskUsageLimit { get; private set; }

        /// <summary>
        /// Gets an integer indicating the maxmimum count of processor cores to use.
        /// </summary>
        public int CpuCountLimit { get; private set; }

        /// <summary>
        /// Gets a float indicating the maxmimum percentage of processor cores to use.
        /// <remarks>If this value is set, it has precedence over CpuCountLimit.</remarks>
        /// </summary>
        public float CpuCountPercentage { get; private set; }

        /// <summary>
        /// Creates a new instance of this class, storing the given parameters. 
        /// </summary>
        /// <param name="runOnBatteries">A bool indicating whether Boinc should run if the computer
        /// runs on batteries.</param>
        /// <param name="useGpuIfUserActive">A bool indicating whether Boinc should use the gpu for calculations.</param>
        /// <param name="cpuUsageLimit">A float number indicating the cpu usage limit in percent.</param>
        /// <param name="diskUsageLimit">A float number indicating the maximum disk usage in gigabyte.</param>
        /// <param name="cpuCountLimit">An integer indicating the maxmimum count of processor cores to use.</param>
        /// <param name="cpuCountPercentage">The percentage of cores to use, or -1 to auto-calculate this percentage specific to the local host from cpuCountLimit. <remarks>If this value is set, it has precedence over cpuCountLimit.</remarks></param>
        public Preferences(bool runOnBatteries, bool useGpuIfUserActive, float cpuUsageLimit, float diskUsageLimit, int cpuCountLimit, float cpuCountPercentage = -1)
        {
            if (cpuUsageLimit < 0 || cpuUsageLimit > 100)
            {
                throw new ArgumentException("Parameter cpuUsageLimit is a percentage and therefore must be between 0 and 100");
            }
            if (diskUsageLimit < 0)
            {
                throw new ArgumentException("Parameter diskUsageLimit must not be negative, since negative disk space limits cannot be enforced.");
            }
            if (cpuCountLimit < 0)
            {
                throw new ArgumentException("Parameter cpuCountLimit must not be negative, since a negative cpu count cannot be enforced.");
            }

            if (cpuCountPercentage == -1)
            {
                cpuCountPercentage = cpuCountLimit * 100.0F / Environment.ProcessorCount;
            }

            if (cpuCountPercentage < 0)
            {
                throw new ArgumentException("Parameter cpuCountPercentage must not be negative, since a negative cpu count cannot be enforced.");
            }

            this.RunOnBatteries = runOnBatteries;
            this.UseGpuIfUserIsActive = useGpuIfUserActive;
            this.CpuUsageLimit = cpuUsageLimit;
            this.DiskUsageLimit = diskUsageLimit;
            this.CpuCountLimit = cpuCountLimit;
            this.CpuCountPercentage = cpuCountPercentage;
        }
    }
}
