using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Achievements;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the achievement configuration views.
    /// It also provides a function to initiate the creation of an achievement.
    /// </summary>
    public class AchievementConfigurationController : Controller
    {
        /// <summary>
        /// Shows the detail view for the given achievement to the admin.
        /// </summary>
        /// <param name="id">The identifier of the achievement to get the details for.</param>
        /// <returns>The AchievementDetailView for the given achievement.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult AchievementDetail(int id)
        {
            AchievementManager manager = new AchievementManager();
            Achievement achievement = manager.GetAchievementById(id);
            return View(achievement);
        }
        /// <summary>
        /// Shows the AchievementOverviewView.
        /// </summary>
        /// <returns>The AchievementOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult AchievementOverview()
        {
            return View();
        }
        /// <summary>
        /// Returns the partial ListView containing a list of achievements according to the given parameter.
        /// </summary>
        /// <param name="searchName">If this parameter is set, only achievements which names that match the given string are returned.</param>
        /// <returns>The partial ListView according to the given parameter.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult List(String searchName)
        {
            AchievementManager manager = new AchievementManager();
            IEnumerable<Achievement> achievements = manager.GetAllExistingAchievements();
            User current = SecurityProvider.CurrentUser as User;
            string currentLanguage = LanguageManager.CurrentLanguage.Code;
            if (searchName != null && searchName != "")
            {
                //Get the achievements where the translation of the name contains the given parameter (name). 
                achievements = achievements.Where(u => u.Name.Translations.Where(x => x.Culture == currentLanguage).FirstOrDefault().Text.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
            }
                        
            return PartialView("List", achievements);
        }
        /// <summary>
        /// Shows the CreateAchievementView containing a list of the achievement types in order to create a new achievement of a specific type.
        /// </summary>
        /// <returns>The CreateAchievementView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult CreateAchievement()
        {
            AchievementManager manager = new AchievementManager();
            //Create a SelectListItem list to use in a view as a drop down list.
            List<SelectListItem> achievementTypes = new List<SelectListItem>();
            IEnumerable<String> types = manager.GetAllAchievementTypes();
            foreach (String type in types)
            {
                achievementTypes.Add(new SelectListItem() { Text = type, Value = type });
            }

            return View("CreateAchievement", achievementTypes);
        }
        /// <summary>
        /// Gives the control to the Achievement Manager in order to edit an old Achievement.
        /// Afterwards the control is returned.
        /// </summary>
        /// <param name="id">This parameter represents the id of the achievement to be edited.</param>
        /// <returns>The ActionResult, indicating either error or success.</returns>
        public ActionResult EditAchievement(int id)
        {
            AchievementManager manager = new AchievementManager();
            return Redirect(manager.GetEditUrl(this.Url.Action("AchievementDetail", new { id = id }), manager.GetAchievementById(id), this.ControllerContext));
        }
        /// <summary>
        /// Gives the control to the Achievement Manager in order to create a new Achievement of a specific type.
        /// Afterwards the control is returned.
        /// </summary>
        /// <param name="achievementType">This parameter represents the type of the achievement to be created.</param>
        /// <returns>The ActionResult, indicating either error or success.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult CreateAchievement(String achievementType)
        {   
            //ahievementType has to be choosen in order to create a new achievement.
            if ((achievementType == null) || (achievementType == ""))
            {
                return RedirectToAction("CreateAchievement");
            }
            AchievementManager manager = new AchievementManager();
            //Redirects to the Achievement Manager, shows AchievementOverview afterwards.
            return Redirect(manager.GetCreateUrl(this.Url.Action("AchievementOverview"), achievementType, this.ControllerContext));
        }
    }
}
