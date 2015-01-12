using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Achievements;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for managing the achievement views.
    /// All achievements and already gained achievements can be viewed.
    /// </summary>
    public class UserAchievementController : Controller
    {
        /// <summary>
        /// Returns the AchievementsView.
        /// </summary>
        /// <returns>The AchievementsView.</returns>
        public ActionResult Achievements()
        {
            return View();
        }
        /// <summary>
        /// Returns the partial AchievementsListView showing all the achievements that can be gained.
        /// </summary>
        /// <returns>The AchievementsListView.</returns>
        public ActionResult GetAllAchievements()
        {
            AchievementManager manager = new AchievementManager();
            IEnumerable<Achievement> items = manager.GetAllExistingAchievements().Where(u => u.Active);
            return PartialView("AchievementsList", items);
        }
        /// <summary>
        /// Returns the partial AchievementsListView showing the achievements gained by the current (or currently viewed) user.
        /// </summary>
        /// <returns>The AchievementsListView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult GetMyAchievements(int? id)
        {            
            Account current = SecurityProvider.CurrentUser;
            //check, what information has to be shown. A user can view his or her own achievements, an admin can view achievements for a user
            //with a given id
            if (id != null && current is SGridTeamMember)
            {
                current = (new MemberManager()).GetAccountById(id.Value);
            }
            AchievementManager manager = new AchievementManager();
            IEnumerable<Achievement> items = new List<Achievement>();
            if (current != null && current is User)
            {
                items = manager.GetAchievements(current as User);
            }
            return PartialView("AchievementsList", items);
        }
        /// <summary>
        /// Returns the detail view for the given achievement.
        /// </summary>
        /// <param name="id">The integer identifier of the achievement to get the details for.</param>
        /// <returns>The AchievementDetailView for the given achievement.</returns>
        public ActionResult AchievementDetail(int id)
        {
            AchievementManager manager = new AchievementManager();
            Achievement item = manager.GetAchievementById(id);
            bool gotAchievement = false;
            Account account = SecurityProvider.CurrentUser;
            if (account != null && account is User)
            {
                User current = account as User;
                //Check, if user already has this achievement.
                gotAchievement = manager.GetAchievements(current).Count(u => u.AchievementId == item.AchievementId) > 0;
            }           
            return PartialView(new Tuple<Achievement, bool>(item, gotAchievement));
        }
    }
}
