using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sGridServer.Code.GridProviders.BoincProviders
{
    /// <summary>
    /// Demo provider for internal testing. 
    /// </summary>
    public class SGridDemoProvider : BoincGridProvider
    {
        private const string ProviderId = "sGrid_DemoServer";
        private new const string ProjectUrl = "http://sgrid.ipd.kit.edu";

        /// <summary>
        /// Returns the description for SGridDemoProvider.
        /// </summary>
        public static new GridProviderDescription Description
        {
            get
            {
                return new GridProviderDescription(ProviderId, "sGrid Demo", "~/Content/images/gridProviders/wcg.jpg",
                    "",
                    "",
                    ProjectUrl, ProjectUrl,
                    new GridProjectDescription[]
                    {
                        new GridProjectDescription("sgrid_demo", 
                            "", 
                            "sGrid Demo Project",  "~/Content/images/gridProviders/globe.jpg", 
                            "", 
                            ProjectUrl, ProjectUrl, 20, 700)
                    },
                    typeof(SGridDemoProvider));
            }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public SGridDemoProvider() : base(Description)
        {
            
        }

    }
}