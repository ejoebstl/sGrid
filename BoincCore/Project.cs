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
    /// This class wraps a Boinc project. 
    /// <remarks>
    /// Instances of this class do not update automatically. Use the BoincClient class to get an updated copy
    /// for each project. 
    /// </remarks>
    /// </summary>
    public class Project
    {
        private bool moreWorkAllowed;
        private bool suspended;
        private BoincClient client;
        
        /// <summary>
        /// Boinc operation code for project suspend. 
        /// </summary>
        private const string OperationSuspend = "project_suspend";

        /// <summary>
        /// Boinc operation code for project resume. 
        /// </summary>
        private const string OperationResume = "project_resume";

        /// <summary>
        /// Boinc operation code for project reset. 
        /// </summary>
        private const string OperationReset = "project_reset";

        /// <summary>
        /// Boinc operation code for project detach. 
        /// </summary>
        private const string OperationDetach = "project_detach";

        /// <summary>
        /// Boinc operation code for project update. 
        /// </summary>
        private const string OperationUpdate = "project_update";

        /// <summary>
        /// Boinc operation code for project allowing more work. 
        /// </summary>
        private const string OperationAllowMoreWork = "project_allowmorework";

        /// <summary>
        /// Boinc operation code for project allowing no more work. 
        /// </summary>
        private const string OperationNoMoreWork = "project_nomorework";

        /// <summary>
        /// Gets or sets a bool specifying whether this project should allow more work.
        /// </summary>
        public bool MoreWorkAllowed
        {
            get { return moreWorkAllowed; }
            set
            {
                if (value && !moreWorkAllowed)
                    AllowMoreWork();
                else if (!value && moreWorkAllowed)
                    DisallowMoreWork();

                moreWorkAllowed = value;
            }
        }

        /// <summary>
        /// Gets or sets a bool specifying whether the project was suspended.
        /// </summary>
        public bool Suspended
        {
            get { return suspended; }
            set
            {
                if (value && !suspended)
                    Resume();
                else if (!value && suspended)
                    Suspend();

                suspended = value;
            }
        }

        /// <summary>
        /// Gets this project’s disk usage in megabyte.
        /// </summary>
        public int DiskUsage { get; internal set; }

        /// <summary>
        /// Gets a bool indicating whether this project has ended.
        /// </summary>
        public bool HasEnded { get; internal set; }

        /// <summary>
        /// Gets the project’s master url.
        /// </summary>
        public string MasterUrl { get; internal set; }

        /// <summary>
        /// Gets the project’s name.
        /// </summary>
        public string ProjectName { get; internal set; }

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="allowMoreWork">A bool indicating if more work is allowed</param>
        /// <param name="diskUsage">The disk usage of this project, in megabyte</param>
        /// <param name="suspended">A bool indicating if this project was suspended</param>
        /// <param name="hasEnded">A bool indicating if this project hsa ended</param>
        /// <param name="masterUrl">The master url of this project</param>
        /// <param name="projectName">The name of this project</param>
        /// <param name="client">The Boinc client associated with this project</param>
        internal Project(bool allowMoreWork, int diskUsage, bool suspended, bool hasEnded, string masterUrl, string projectName, BoincClient client)
        {
            this.HasEnded = hasEnded;
            this.MasterUrl = masterUrl;
            this.ProjectName = projectName;

            this.DiskUsage = diskUsage;

            this.suspended = suspended;
            this.moreWorkAllowed = allowMoreWork;

            this.client = client;
        }

        /// <summary>
        /// Detaches the Boinc client from this project.
        /// </summary>
        public void Detach()
        {
            client.WriteProjectOp(MasterUrl, OperationDetach);
        }


        /// <summary>
        /// Allows more work for this project. 
        /// </summary>
        public void AllowMoreWork()
        {
            client.WriteProjectOp(MasterUrl, OperationAllowMoreWork);
        } 
        
        /// <summary>
        /// Disallows more work for this project. 
        /// </summary>
        public void DisallowMoreWork()
        {
            client.WriteProjectOp(MasterUrl, OperationNoMoreWork);
        }


        /// <summary>
        /// Resumes project calculation. 
        /// </summary>
        public void Resume()
        {
            client.WriteProjectOp(MasterUrl, OperationResume);
        }

        /// <summary>
        /// Suspends project calculation.
        /// </summary>
        public void Suspend()
        {
            client.WriteProjectOp(MasterUrl, OperationSuspend);
        }

        /// <summary>
        /// Gets all results for this project from Boinc.
        /// </summary>
        /// <returns>All results for this project.</returns>
        public IEnumerable<Result> GetResults(bool onlyActive = true)
        {
            List<Result> results = new List<Result>();

            foreach (Result result in client.GetResults())
            {
                if (result.ProjectUrl.Equals(this.MasterUrl))
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
