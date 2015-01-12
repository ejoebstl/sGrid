using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.GridProviders;
using System.Reflection;

namespace sGrid
{
    public class ClientApi : IClientApi
    {
        /// <summary>
        /// Gets information about the grid project the client
        /// should attach to and also returns authentication data
        /// for the communication with the grid provider.
        /// Note: if the user is not attached to any project, null
        /// will be returned.
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <returns>Information and an authentication token for
        /// the project the client should attach to.</returns>
        public ProjectData GetCurrentProject(Credential authentication)
        {
            User user = GetUser(authentication);
            GridProjectDescription project = new GridProviderManager(user).CurrentProject;
            if (project == null)
            {
                return null;
            }
            String token = GridProvider.GetAuthTokenForUser(user, project).AuthenticationInfo;
            return new ProjectData(new SerializableGridProjectDescription(project), token);
        }

        /// <summary>
        /// Gets information and authentication data about the grid
        /// project the client has to authenticate at.
        /// Note: if no project with the given name exists, null
        /// will be returned.
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <param name="projectName">The name identifying the
        /// project the token is wanted for</param>
        /// <returns>Information and authentication tokens for the
        /// project the client has to authenticate to.</returns>
        public ProjectData GetTokenForProject(Credential authentication, String projectName)
        {
            GridProjectDescription project = GridProviderManager.ProjectForName(projectName);
            if (project == null)
            {
                return null;
            }
            String token = GridProvider.GetAuthTokenForUser(GetUser(authentication), project).AuthenticationInfo;
            return new ProjectData(new SerializableGridProjectDescription(project), token);
        }

        /// <summary>
        /// Gets the User object associated with the authentication
        /// credentials. If the authentication is wrong an exception
        /// is thrown.
        /// </summary>
        /// <param name="authentication">The data used to identify
        /// the client at the server.</param>
        /// <returns>The User object associated with the Credential.</returns>
        public User GetUser(Credential authentication)
        {
            User user = (User)new MemberManager().ValidateAuthToken(
                authentication.AuthenticationToken, authentication.UserId);
            if (user == null)
            {
                throw new ArgumentException("invalid Credential");
            }
            return user;
        }

        /// <summary>
        /// Notifies the server that the client has finished
        /// calculating a package and sends a StatusMessage.
        /// </summary>
        /// <param name="message">A message giving information
        /// about the client state and the calculated package.</param>
        public void PackageFinished(StatusMessage message)
        {
            GridProjectDescription project = GridProviderManager.ProjectForName(message.ProjectShortName);
            if (project != null)
            {
                GridProvider provider = project.Provider.CreateProvider();

                CalculatedResult result = new CalculatedResult();
                result.EndTime = DateTime.Now;
                result.ProjectShortName = message.ProjectShortName;
                result.StartTime = message.StartTime;
                result.UserId = GetUser(message.Authentication).Id;
                result.Valid = false;
                provider.RegisterResultsSent(result);
            }
        }

        /// <summary>
        /// Tests if the client is in an consistant state.
        /// That is to say: if the credential is correct
        /// and the clientversion is the newest one.
        /// </summary>
        /// <param name="authentication">The credential to be tested.</param>
        /// <returns>True, if the credential is associated with a user.</returns>
        public TestResult TestClient(Credential authentication, int clientversion)
        {
            //try
           //{
                User user = (User)new MemberManager().ValidateAuthToken(
                    authentication.AuthenticationToken, authentication.UserId);
                if (user == null) return TestResult.WrongCredential;
                if (clientversion < 1) return TestResult.WrongCredential;
                return TestResult.Correct;
            //}
            //catch (ReflectionTypeLoadException ex)
            //{
            //    StringBuilder sb = new StringBuilder();

            //    foreach (Exception e in ex.LoaderExceptions)
            //    {
            //        sb.AppendLine();
            //        sb.AppendLine(e.GetType().ToString() + ":");
            //        sb.AppendLine(e.Message);
            //        sb.AppendLine(e.StackTrace);
            //    }

            //    System.IO.File.WriteAllText("C:\\DWASFiles\\Sites\\sgridservice\\VirtualDirectory0\\site\\wwwroot\\asdflog.txt", sb.ToString());
            //}

            //return TestResult.WrongCredential;
        }
    }
}
