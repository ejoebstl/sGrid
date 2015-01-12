using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Models;
using sGridServer.Code.GridProviders;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using Resource = sGridServer.Resources.Project.Project;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The project user interface enables users to view and search for grid projects.
    /// Statics of the projects are also shown.
    /// Registered users can also change the project they are currently working on and start with another project. 
    /// </summary>
    public class ProjectController : Controller
    {
        /// <summary>
        /// Shows the GridProjectOverviewView.
        /// </summary>
        /// <returns>The GridProjectOverviewView.</returns>
        public ActionResult GridProjectOverview()
        {
            return View();
        }

        /// <summary>
        /// Shows the detailed view of the given grid provider.
        /// </summary>
        /// <param name="name">The identifier of the project to get the details for.</param>
        /// <returns>The GridProviderDetailView for the given grid provider.</returns>
        public ActionResult GridProviderDetail(String name)
        {
            //if the name isn't null or an empty string it finds a grid provider and if that grid provider exists it returns it,
            //otherwise it shows a message that the grid provider with the given name doesn't exist
            if (name != null && name != "")
            {
                GridProviderDescription provider = GridProviderManager.RegisteredProviders.Where(p => p.Name == name).FirstOrDefault();

                if (provider != null)
                {
                    return View(GridProviderManager.ProviderForName(provider.Id));
                }
            }

            ViewBag.Message = String.Format(Resource.NoGridProvider, name);

            return View();
        }

        /// <summary>
        /// Shows the ListProjectsView according to the given parameters.
        /// </summary>
        /// <param name="searchName">If this parameter is set, only projects which names match the given string are returned.</param>
        /// <returns>The partial ListProjectStatisticsView according to the given parameters.</returns>
        public ActionResult ListProjects(String searchName)
        {
            //gets all grid projects
            IEnumerable<GridProjectDescription> result = GridProviderManager.RegisteredProviders.SelectMany(p => p.AvailableProjects);

            //chooses only those projects that contain the given string in the name
            if (searchName != null && searchName != "")
            {
                result = result.Where(p => p.Name.Text.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
            }

            return PartialView(result);
        }

        /// <summary>
        /// Shows the ListProjectStatisticsView according to the given parameters.
        /// </summary>
        /// <param name="from">The start date of the time span.</param>
        /// <param name="to">The end date of the time span.</param>
        /// <param name="project">The statistic has to be shown for a grid project with this name.</param>
        /// <returns>The partial ListProjectStatisticsView according to the given parameters.</returns>
        public ActionResult ListProjectStatistics(DateTime from, DateTime to, String project)
        {
            //gets the project snapshots according to the given parameters
            GridPerformanceData data = GridProviderManager.CreateSumary(GridProviderManager.GlobalResults.Where(
                p => p.ProjectShortName == project &&
                p.Valid &&
                p.ServerReceivedTimestamp >= from &&
                p.ServerReceivedTimestamp <= to.AddDays(1)));

            if (data.ResultCount != 0)
            {
                ViewBag.ProjectName = project;
                return PartialView(data);
            }
            else
            {
                ViewBag.Message = Resource.NoStatistics;
                return PartialView();
            }

        }

        /// <summary>
        /// Returns the detail view for the given project.
        /// </summary>
        /// <param name="name">The identifier of the project to get the details for.</param>
        /// <returns>The ProjectDetailView for the given project.</returns>
        public ActionResult ProjectDetail(String name)
        {
            //if the name isn't null or an empty string it returns the corresponding grid project,
            //otherwise it shows a message that the grid project with the given name doesn't exist
            if (name != null && name != "")
            {
                return View(GridProviderManager.ProjectForName(name));
            }
            else
            {
                ViewBag.Message = String.Format(Resource.NoProject, name);
                return View();
            }
        }

        /// <summary>
        /// Shows the ProjectStatisticsView.
        /// </summary>
        /// <returns>The ProjectStatisticsView.</returns>
        public ActionResult ProjectStatistics(String shortName)
        {
            Object projectShortName = shortName;
            return View(projectShortName);
        }

        /// <summary>
        /// Switches the current project of the user to a given project.
        /// </summary>
        /// <param name="shortName">The identifier of the project to be set as the current user’s current project.</param>
        /// <returns>The ActionResult indicating either error or success. </returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult SwitchProject(string shortName)
        {
            GridProviderManager manager = new GridProviderManager(SecurityProvider.CurrentUser as User);
            GridProjectDescription project = GridProviderManager.ProjectForName(shortName);

            if (shortName != null && shortName != "")
            {
                GridProjectDescription currentProject = new GridProviderManager(manager.User).CurrentProject;

                //if user already has a project, it detaches him from this project
                if (currentProject != null)
                {
                    manager.DetachFromProject(currentProject);
                }

                //attaches user to the given project 
                manager.AttachToProject(project);
                
            }

            return new EmptyResult();
            
        }
    }
}
