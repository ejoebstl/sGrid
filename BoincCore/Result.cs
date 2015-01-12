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
    /// This class wraps a result of the Boinc application. 
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates whether we got a server acknowledgement. 
        /// </summary>
        public bool Acknowledged { get; internal set; }
        /// <summary>
        /// Indicates whether this task is currently active. 
        /// </summary>
        public bool IsActive { get; internal set; }
        /// <summary>
        /// Indicates whether this task is ready to report.
        /// </summary>
        public bool ReadyToReport { get; internal set; }

        /// <summary>
        /// Gets the time this result task has spent on the CPU. 
        /// </summary>
        public TimeSpan CurrentCpuTime { get; internal set; }
        /// <summary>
        /// Gets the time since work on this result started. 
        /// </summary>
        public TimeSpan ElapsedTime { get; internal set; }
        /// <summary>
        /// Gets the estimated remaining CPU time. 
        /// </summary>
        public TimeSpan EstimatedCpuTimeRemaining { get; internal set; }
        /// <summary>
        /// Gets the final CPU time, after work completed.
        /// </summary>
        public TimeSpan FinalCpuTime { get; internal set; }
        /// <summary>
        /// Gets the final elapsed time, after work completed.
        /// </summary>
        public TimeSpan FinalElapsedTime { get; internal set; }

        /// <summary>
        /// Indicates when this result was received.  
        /// </summary>
        public DateTime ReceivedTime { get; internal set; }
        /// <summary>
        /// Indicates when this result will expire. 
        /// </summary>
        public DateTime ReportDeadline { get; internal set; }

        /// <summary>
        /// Gets the exit status of the worker process.
        /// </summary>
        public int ExitStatus { get; internal set; }
        /// <summary>
        /// Gets the process id of the worker process.
        /// </summary>
        public int ProcessId { get; internal set; }
        /// <summary>
        /// Gets the received signal of the worker process.
        /// </summary>
        public int Signal { get; internal set; }
        /// <summary>
        /// Gets the state of the worker process.
        /// </summary>
        public int State { get; internal set; }
        /// <summary>
        /// Gets the state of the task.
        /// </summary>
        public TaskState TaskState { get; internal set; }
        /// <summary>
        /// Gets the version number of the work package. 
        /// </summary>
        public int VersionNumber { get; internal set; }

        /// <summary>
        /// Gets the name of the work package. 
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets the project url of the work package. 
        /// </summary>
        public string ProjectUrl { get; internal set; }
        /// <summary>
        /// Gets the name of the work package. 
        /// </summary>
        public string Resources { get; internal set; }
        /// <summary>
        /// Gets the work unit name of the work package. 
        /// </summary>
        public string WorkUnitName { get; internal set; }

        /// <summary>
        /// Gets the percentage which was done of this work package. 
        /// </summary>
        public double FractionDone { get; internal set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        internal Result()
        {

        }
    }

    /// <summary>
    /// Enumeration of different task states. 
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// The worker process does not exist yet.
        /// </summary>
        Unitialized = 0,
        /// <summary>
        /// The process is running. 
        /// </summary>
        Executing = 1,
        /// <summary>
        /// The process received a "abort" message. 
        /// </summary>
        AbortPending = 5,
        /// <summary>
        /// The process received a "quit" message.
        /// </summary>
        QuitPending = 8,
        /// <summary>
        /// The process received a "suspend" message. 
        /// </summary>
        Suspended = 9,
        /// <summary>
        /// The process is waiting for file copy operations to finish.
        /// </summary>
        CopyPending = 10
    }
}
