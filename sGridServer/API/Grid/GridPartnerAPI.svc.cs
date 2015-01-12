using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.GridProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace sGridServer.API.Grid
{
    public class GridPartnerAPI : IGridPartnerAPI
    {
        //          /_  __/ __ \/ __ \/ __ \
        //           / / / / / / / / / / / /
        //          / / / /_/ / /_/ / /_/ / 
        //         /_/  \____/_____/\____/
        //
        //      TODO: Add authentication to service (using certs). 
        
        public void FeededWorkUnit(string projectId, string workunitId, string userId)
        {
            RegisterResultEvent(projectId, workunitId, Int32.Parse(userId), ResultState.SentToClient);
        }

        public void ReceivedResult(string projectId, string workunitId, string userId)
        {
            RegisterResultEvent(projectId, workunitId, Int32.Parse(userId), ResultState.ReceivedByServer);
        }

        public void ValidatedResult(string projectId, string workunitId, string userId, string success = "false")
        {
            RegisterResultEvent(projectId, workunitId, Int32.Parse(userId), ResultState.Validated, bool.Parse(success));
        }

        private static void RegisterResultEvent(string projectId, string workunitId, int userId, ResultState state, bool success = false)
        {
            GridProjectDescription project = GridProviderManager.ProjectForName(projectId);
            if (project == null)
            {
                throw new ArgumentException("Invalid project identifyer");
            }
            GridProvider provider = project.Provider.CreateProvider();

            CalculatedResult result = new CalculatedResult()
            {
                ProjectShortName = projectId,
                UserId = userId,
                WorkUnitName = workunitId
            };

            provider.RegisterResultEvent(result, state, success);
        }
    }
}
