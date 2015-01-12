using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using System.Collections.Concurrent;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.GridProviders.WorldCommunityGrid;

namespace sGridServer.Code.GridProviders
{
    /// <summary>
    /// This class provides a way to register GridProviderDescriptions and their 
    /// corresponding GridProjectDescriptions on application startup. 
    /// The registered descriptions can be enumerated by other classes later.
    /// </summary>
    public class GridProviderManager
    {
        /// <summary>
        /// The data context of this class.
        /// </summary>
        private SGridDbContext dataContext;

        /// <summary>
        /// A collection for holding all registered grid providers.
        /// </summary>
        private static ConcurrentBag<GridProviderDescription> registeredProviders;

        /// <summary>
        /// Initializes static fields of this class.
        /// </summary>
        static GridProviderManager()
        {
            registeredProviders = new ConcurrentBag<GridProviderDescription>();

            //Register grid providers
            GridProviderManager.RegisterProvider(sGridServer.Code.GridProviders.BoincProviders.SGridDemoProvider.Description);

        }

        /// <summary>
        /// Gets all previously registered GridProviders.
        /// </summary>
        public static IEnumerable<GridProviderDescription> RegisteredProviders
        {
            get
            {
                return registeredProviders;
            }
        }

        /// <summary>
        /// Gets the project currently assigned to the user 
        /// associated with this GridProviderManager object. 
        /// </summary>
        public GridProjectDescription CurrentProject
        {
            get
            {
                AttachedProject current = (from a in dataContext.AttachedProjects
                                           where a.Current && a.UserId == this.User.Id
                                           select a).FirstOrDefault();

                if (current == null)
                {
                    return null;
                }
                else
                {
                    return ProjectForName(current.ShortName);
                }
            }
        }


        /// <summary>
        /// Gets a performance summary of the user currently associated 
        /// with this GridProviderManager object. The GridProvider, DeviceCount and 
        /// Projects attributes of the returned GridPerformanceData object are not set. 
        /// </summary>
        public GridPerformanceData CurrentSummary
        {
            get
            {
                return CurrentSummaryRange(DateTime.MinValue, DateTime.MaxValue.AddDays(-1));
            }
        }
        /// <summary>
        /// Gets a performance summary of the user currently associated 
        /// with this GridProviderManager object. The GridProvider, DeviceCount and 
        /// Projects attributes of the returned GridPerformanceData object are not set. 
        /// </summary>
        /// <param name="from">The start date of the time span to summarize.</param>
        /// <param name="to">The end date of the time span to summarize.</param>
        public GridPerformanceData CurrentSummaryRange(DateTime from, DateTime to)
        {
            IEnumerable<CalculatedResult> results = Results.Where(p => p.Valid && p.ServerReceivedTimestamp >= from && p.ServerReceivedTimestamp <= to);
            return CreateSumary(results);
        }

        /// <summary>
        /// Gets the user associated with this GridProviderManager object.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets information about all results returned to the project servers by all users.
        /// </summary>
        public static IEnumerable<CalculatedResult> GlobalResults
        {
            get
            {
                return new SGridDbContext().CalculatedResults.AsNoTracking();
            }
        }

        /// <summary>
        /// Gets information about all results returned to the project servers by the current user.
        /// </summary>
        public IEnumerable<CalculatedResult> Results
        {
            get
            {
                return dataContext.CalculatedResults.AsNoTracking().Where(p => p.User.Id == User.Id);
            }
        }

        /// <summary>
        /// Gets a performance summary of all users. The GridProvider, User, DeviceCount and Projects attributes of the 
        /// returned GridPerformanceData object are not set. 
        /// </summary>
        /// <param name="from">The start date of the time span to summarize.</param>
        /// <param name="to">The end date of the time span to summarize.</param>
        public static GridPerformanceData GlobalSummaryRange(DateTime from, DateTime to)
        {
            IEnumerable<CalculatedResult> results = GlobalResults.Where(p => p.Valid && p.ServerReceivedTimestamp >= from && p.ServerReceivedTimestamp <= to);

            return CreateSumary(results);
        }

        /// <summary>
        /// Summarizes the given list of calculated results.
        /// </summary>
        /// <param name="results">The list to summarize</param>
        /// <returns>The summarized list as a GridPerformanceData object.</returns>
        public static GridPerformanceData CreateSumary(IEnumerable<CalculatedResult> results)
        {
            int resultCount = results.Count();
            int totalRuntime = (int)results.Sum(p => (p.ServerReceivedTimestamp - p.ServerSentTimestamp).TotalSeconds);
            DateTime timeOfLastResult = DateTime.MinValue;
            if (results.Any())
            {
                timeOfLastResult = results.Max(p => p.ValidatedTimestamp);
            }
            return new GridPerformanceData(-1, null, resultCount, timeOfLastResult, totalRuntime, null);
        }

        /// <summary>
        /// Gets a performance summary of all users. The GridProvider, User, DeviceCount and Projects attributes of the 
        /// returned GridPerformanceData object are not set. 
        /// </summary>
        public static GridPerformanceData GlobalSummary
        {
            get
            {
                return GlobalSummaryRange(DateTime.MinValue, DateTime.MaxValue);
            }
        }

        /// <summary>
        /// This event is raised whenever a user attached to or detached from a project. 
        /// </summary>
        public static event EventHandler<ProjectChangedEventArgs> ProjectChanged;

        /// <summary>
        /// Creates a new instance of this class, associated with the given user. 
        /// </summary>
        /// <param name="user">The user to associate the new instance of GridProviderManager with. </param>
        public GridProviderManager(User user)
        {
            this.dataContext = new SGridDbContext();
            this.User = user;
        }

        /// <summary>
        /// Attaches the user associated with this GridProviderManager object with the given project. 
        /// <remarks>
        /// If no authentication information is known for this user for this project, the user is registered with the grid provider. 
        /// </remarks>
        /// </summary>
        /// <param name="proj">The GridProjectDescription referring the project to attach to. </param>
        public void AttachToProject(GridProjectDescription proj) 
        {
            //Check if the user already has a current project. 
            if (CurrentProject != null)
            {
                throw new InvalidOperationException("The user is already associated with a project. Detach the current project first.");
            }

            //If the user was already attached to the given project, re-use the record. 
            AttachedProject project = (from a in dataContext.AttachedProjects
                                       where a.UserId == this.User.Id 
                                             && a.ShortName == proj.ShortName
                                       select a).FirstOrDefault();

            if (project != null)
            {
                project.Current = true;
            }
            else
            {
                dataContext.AttachedProjects.Add(new AttachedProject()
                {
                    Current = true,
                    Date = DateTime.Now,
                    ShortName = proj.ShortName,
                    UserId = this.User.Id
                });
            }

            //Create the provider and register the user with the provider, if necassary. 
            GridProvider provider = proj.Provider.CreateProvider();

            if (GridProvider.GetAuthTokenForUser(User, proj) == null)
            {
                provider.RegisterUser(User);
            }

            //Save all changes.
            dataContext.SaveChanges();

            //Invoke the ProjectChanged event.
            InvokeProjectChanged(User, proj, true);
        }

        /// <summary>
        /// Disassociates the user associated with this GridProviderManager object from the given project. 
        /// </summary>
        /// <param name="proj">The GridProjectDescription referring the project to detach from.</param>
        public void DetachFromProject(GridProjectDescription proj)
        {
            //Get the record to edit. 
            AttachedProject current = (from a in dataContext.AttachedProjects
                                       where a.ShortName == proj.ShortName && a.UserId == this.User.Id
                                       select a).FirstOrDefault();

            //Mark the project as detached.
            if (current != null)
                current.Current = false;

            //Save all changes. 
            dataContext.SaveChanges();

            //Invoke the ProjectChanged event. 
            InvokeProjectChanged(User, proj, false);
        }

        /// <summary>
        /// Gets a GridProjectDescription object by its short name. 
        /// </summary>
        /// <param name="projectName">The short name of the project to get. </param>
        /// <returns>The grid project with the given short name or null, if nothing was found. </returns>
        public static GridProjectDescription ProjectForName(string projectName)
        {
            return registeredProviders.SelectMany(p => p.AvailableProjects)
                .Where(p => p.ShortName == projectName).FirstOrDefault();
        }

        /// <summary>
        /// Gets a GridProviderDescription object by its identifier string. 
        /// </summary>
        /// <param name="providerId">The identifier of the provider to get. </param>
        /// <returns>The grid provider with the given identifier or null, if nothing was found. </returns>
        public static GridProviderDescription ProviderForName(string providerId)
        {
            return RegisteredProviders.Where(p => p.Id == providerId).FirstOrDefault();
        }

        /// <summary>
        /// Gets a GridProviderDescription object by its identifier string. 
        /// </summary>
        /// <param name="gridProvider">The GridProviderDescription object to register.</param>
        public static void RegisterProvider(GridProviderDescription gridProvider)
        {
            registeredProviders.Add(gridProvider);
        }

        /// <summary>
        /// Invokes the ProjectChanged event using the given parameters.
        /// </summary>
        /// <param name="user">The user who attached to or detached from a project.</param>
        /// <param name="project">The project the user detached from or attached to. </param>
        /// <param name="isAttach">A bool indicating whether the user attached to or detached from the project. </param>
        protected static void InvokeProjectChanged(User user, GridProjectDescription project, bool isAttach)
        {
            if (ProjectChanged != null)
            {
                ProjectChanged(null, new ProjectChangedEventArgs(user, project, isAttach));
            }
        }
    }
}