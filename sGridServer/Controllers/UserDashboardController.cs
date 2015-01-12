using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Achievements;
using sGridServer.Code.Utilities;
using sGridServer.Code.Rewards;
using sGridServer.Code.GridProviders;
using System.IO;
using sGridServer.Properties;
using System.Drawing;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for showing the user dashboard.
    /// It also has a functionality to edit user information or upload and change user’s profile picture.
    /// </summary>
    public class UserDashboardController : Controller
    {
        /// <summary>
        /// Shows the list of the achievements gained by the current (or currently viewed by an admin) user.
        /// </summary>
        /// <returns>The partial AchievementsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Achievements(int? id)
        {
            MemberManager memberManager = new MemberManager();
            User current;
            //Set current to current user, if no id was provided.
            if (id != null && id > 0)
            {
                current = memberManager.GetAccountById(id.Value) as User;
            }
            else
            {
                current = SecurityProvider.CurrentUser as User;
            }
            AchievementManager manager = new AchievementManager();
            IEnumerable<Achievement> items = new List<Achievement>();
            if (current != null)
            {
                items = manager.GetAchievements(current);
            }
            return PartialView("Achievements", items);
        }
        /// <summary>
        /// Shows the EditUserInformationView to change the user’s current profile information.
        /// </summary>
        /// <returns>The partial EditUserInformationView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult EditUserInformation()
        {
            return PartialView();
        }
        /// <summary>
        /// Shows the EditProfilePictureView to change the user’s current profile picture.
        /// </summary>
        /// <param name="id">The id of the user whose profile picture has to be changed.</param>
        /// <returns>The partial EditProfilePictureView to change the user’s current profile picture.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditProfilePicture(int id)
        {
            Account current = new MemberManager().GetAccountById(id);
            User user = null;
            if (current != null)
            {
                user = current as User;
            }
            else
            {
                user = SecurityProvider.CurrentUser as User;
            }
            return PartialView(user);
        }
        /// <summary>
        /// Returns the partial PlacementView, showing the current user’s placement
        /// in the high score rankings (between all users and friends of a user).
        /// </summary>
        /// <returns>A partial PlacementView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult Placement(int? id)
        {
            //Set the user to current user (if not admin) or a user, currently viewed by an admin.
            Account current = SecurityProvider.CurrentUser;
            User user = null;
            if (id != null && current is SGridTeamMember)
            {
                current = new MemberManager().GetAccountById(id.Value);
            }
            else
            {
                user = current as User;
            }
            
            int allPlacement = HighscoreHelper.GetPlacementInHighscore(user);
            int friendsPlacement = HighscoreHelper.GetPlacementInHighscore(user, true);
            
            return PartialView(new Tuple<int, int>(allPlacement, friendsPlacement));
        }
        /// <summary>
        /// Returns the partial SGridNewsView showing the latest sGrid news.
        /// </summary>
        /// <returns>A partial SGridNewsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin | SiteRoles.CoinPartner | SiteRoles.Sponsor)]
        public ActionResult SGridNews()
        {
            NewsManager manager = new NewsManager();
            News item = manager.GetLatestNews();
            return PartialView(item);
        }
        /// <summary>
        /// Returns the partial RewardsView showing the latest sGrid rewards.
        /// </summary>
        /// <returns>A partial RewardsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin | SiteRoles.CoinPartner | SiteRoles.Sponsor)]
        public ActionResult Rewards()
        {
            RewardManager manager = new RewardManager();
            NewsManager newsManager = new NewsManager();
            bool newsAvailable = newsManager.GetLatestNews() != null;
            IEnumerable<Reward> items = manager.GetAllExistingRewards();
            if (items != null)
            {
                items = items.OrderByDescending(x => x.Begin);
                //Only approved rewards should be shown.
                items = items.Where(u => u.Approved);
                //Show only rewards specified for the current period.
                items = items.Where(u => (u.Begin <= DateTime.Now && DateTime.Now <= u.End));
                if (items.Count() >= 2)
                {
                    //2 Rewards will be shown if there are no sGrid news currently. 1 Reward will be shown otherwise (the other will be replace by sGrid news).
                    if (!newsAvailable)
                    {
                        items = items.Take(2);
                    }
                    else if (items.Count() >= 1)
                    {
                        items = items.Take(1);
                    }
                } 
            }
            return PartialView(items);
        }
        /// <summary>
        /// Returns the partial ShortStatisticsView showing short statistic of the current user.
        /// </summary>
        /// <returns>A partial ShortStatisticsView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult ShortStatistics(int? id)
        {
            //Check, what information has to be shown. A user can view his or her own statistics, an admin can view statistics
            //for the user with a given id.
            Account user = SecurityProvider.CurrentUser;
            if (id != null && user is SGridTeamMember)
            {
                user = (new MemberManager()).GetAccountById(id.Value);
            }
            GridPerformanceData data = null;
            if (user is User)
            {
                GridProviderManager provider = new GridProviderManager(user as User);
                data = provider.CurrentSummary;
            }
            return PartialView(data);
        }
        
        /// <summary>
        /// Uploads a new user profile photo, if the upload is successful, the old profile picture is replaced with the new one.
        /// </summary>
        /// <param name="id">The id of the current user.</param>
        /// <param name="file">The profile picture to set.</param>
        /// <returns>User dashboard view.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        [HttpPost]
        public ActionResult UploadProfilePicture(HttpPostedFileBase file, int? id)
        {
            MemberManager manager = new MemberManager();
            Account currentAccount = SecurityProvider.CurrentUser;
            //Set current to either current or currently viewed (by admin) user.
            User currentUser;
            if (currentAccount is SGridTeamMember && id != null)
            {
                currentUser = manager.GetAccountById(id.Value) as User;
            }
            else
            {
                currentUser = currentAccount as User;
            }
             
            BlobStorage storage = new BlobStorage(Settings.Default.ProfilePictureStorageContainer);
            if (currentUser != null && file != null && file.ContentLength > 0)
            {
                storage.RemoveBlob(currentUser.Picture);
                Stream picture = ImageUtil.ResizeImage(file.InputStream, Settings.Default.ProfilePictureWidth, Settings.Default.ProfilePictureHeight, Color.White);
                currentUser.Picture = storage.StoreBlob(picture);

            }
            manager.SaveAccount(currentUser);
            return View("UserDashboard", currentUser);
        }
        /// <summary>
        /// Displays the UserDashboardView.
        /// </summary>
        /// <param name="id">The id of the current or currently viewed (by admin) user.</param>
        /// <returns>A view showing the UserDashboard.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User | SiteRoles.Admin)]
        public ActionResult UserDashboard(int? id)
        {
            User user;
            Account account = SecurityProvider.CurrentUser;
            if (account is SGridTeamMember && id != null && id > 0)
            {
                MemberManager manager = new MemberManager();
                user = manager.GetAccountById(id.Value) as User;
            }
            else
            {
                user = account as User;
            }
            return View(user);
        }
    }
}
