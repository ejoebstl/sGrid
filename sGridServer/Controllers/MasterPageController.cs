using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Models;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Achievements;
using sGridServer.Code.CoinExchange;
using sGridServer.Code.Utilities;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Resources.MasterPage;

namespace sGridServer.Controllers
{
    /// <summary>
    /// The master page controller is responsible for controlling the master page. 
    /// Note that almost all views on this page are partial. 
    /// This is because the master page should be as flexible as possible. 
    /// </summary>
    public class MasterPageController : Controller
    {
        /// <summary>
        /// Returns the AccountBalanceView.
        /// </summary>
        /// <returns>The AccountBalanceView.</returns>
        public PartialViewResult AccountBalance()
        {
            return PartialView();
        }

        /// <summary>
        /// Returns the FooterView using the corresponding MenuItem objects. 
        /// </summary>
        /// <returns>The FooterView.</returns>
        public PartialViewResult Footer()
        {
            if (SecurityProvider.Context != null)
            {
                //the user is logged in
                return PartialView(new MenuItem[] {
                        new MenuItem() { LinkText = MenuText.Help, LinkUrl = "~/UserHelp/UserHelp" },
                        new MenuItem() { LinkText = MenuText.FAQ, LinkUrl = "~/UserHelp/UserFAQ" },
                        new MenuItem() { LinkText = MenuText.Impressum, LinkUrl = "~/Help/Impressum" },
                        new MenuItem() { LinkText = MenuText.Contact, LinkUrl = "~/Contact/Contact" }
                });
            }
            else
            {
                return PartialView(new MenuItem[] {
                        new MenuItem() { LinkText = MenuText.Help, LinkUrl = "~/Help/Help" },
                        new MenuItem() { LinkText = MenuText.FAQ, LinkUrl = "~/Help/FAQ" },
                        new MenuItem() { LinkText = MenuText.Impressum, LinkUrl = "~/Help/Impressum" },
                        new MenuItem() { LinkText = MenuText.Contact, LinkUrl = "~/Help/Contact" }
                });
            }
        }

        /// <summary>
        /// Returns the state of the current user via json. 
        /// </summary>
        /// <returns>A UserState object serialized as json, if a user is currently logged in, an empty json object, else. </returns>
        public JsonResult GetUserState()
        {
            User account = SecurityProvider.CurrentUser as User;

            if (account == null)
            {
                return Json(new object());
            }

            //Fetch the user state.
            UserState userState = new UserState()
            {
                CoinBalance = account.CoinAccount.CurrentBalance,
                Id = account.Id,
                Username = account.Nickname
            };

            //Fetch the next unshown achievement. 
            Achievement nextUnshownAchievement = new AchievementManager().GetNextUnshownAchievement(account);

            if (nextUnshownAchievement != null)
            {
                //Copy the achievement into the user-state object using all relevant information. 
                userState.NextAchievement = new NextAchievementState()
                {
                    AchievementId = nextUnshownAchievement.AchievementId,
                    AchievementType = nextUnshownAchievement.AchievementType,
                    BonusCoins = nextUnshownAchievement.BonusCoins,
                    Description = nextUnshownAchievement.Description.Text,
                    Icon = nextUnshownAchievement.Icon,
                    Name = nextUnshownAchievement.Name.Text
                };
            }

            //Send the data to the user. 
            return Json(userState, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Shows the partial LanguageSelectionView.
        /// </summary>
        /// <returns>LanguageSelectionView.</returns>
        public PartialViewResult LanguageSelection()
        {
            return PartialView();
        }

        /// <summary>
        /// Shows the partial LoginPartView.
        /// </summary>
        /// <returns>LoginPartView.</returns>
        public PartialViewResult LoginPart()
        {
            return PartialView();
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>Returns a redirect result to the frontpage.</returns>
        public ActionResult Logout()
        {
            SecurityProvider.LogOut();

            return Redirect("~/");
        }

        /// <summary>
        /// Returns a partial global helper scripts section.
        /// </summary>
        /// <returns>A partial view.</returns>
        public PartialViewResult HelperScripts()
        {
            return PartialView();
        }

        /// <summary>
        /// Returns the MainMenuView using the corresponding MenuItem objects. 
        /// </summary>
        /// <returns>The MainMenuView.</returns>
        public PartialViewResult MainMenu()
        {
            if (SecurityProvider.Context != null && SecurityProvider.Context.UserPermissions == SiteRoles.Admin)
            {
                //Admins main menu
                return PartialView(new MenuItem[] {
                    new MenuItem() { LinkText = MenuText.ProfileOverview, LinkUrl = "~/MemberConfiguration/MemberOverview"},                    
                    new MenuItem() { LinkText = MenuText.GridProjects, LinkUrl = "~/Project/GridProjectOverview"},
                    new MenuItem() { LinkText = MenuText.About, LinkUrl = "~/About/About"},
                    new MenuItem() { LinkText = MenuText.CoinExchange, LinkUrl = "~/CoinExchange/RewardOverview"},
                    new MenuItem() { LinkText = MenuText.Ranking, LinkUrl = "~/Statistics/Ranking"}
                });
            }
            else
            {
                return PartialView(new MenuItem[] {
                    new MenuItem() { LinkText = MenuText.ProfileOverview, LinkUrl = "~/Profile/ProfileOverview"},
                    new MenuItem() { LinkText = MenuText.GridProjects, LinkUrl = "~/Project/GridProjectOverview"},
                    new MenuItem() { LinkText = MenuText.About, LinkUrl = "~/About/About"},
                    new MenuItem() { LinkText = MenuText.CoinExchange, LinkUrl = "~/CoinExchange/RewardOverview"},
                    new MenuItem() { LinkText = MenuText.Ranking, LinkUrl = "~/Statistics/Ranking"}
                });
            }
            
        }

        /// <summary>
        /// Returns the partial ScriptSectionView.
        /// </summary>
        /// <returns>The ScriptSectionView.</returns>
        public PartialViewResult ScriptSection()
        {
            return PartialView();
        }

        /// <summary>
        /// Returns the partial BannerPart view..
        /// </summary>
        /// <returns>The BannerPart view.</returns>
        public ActionResult BannerPart()
        {
            //Select a random Sponsor.
            MemberManager memberManager = new MemberManager();
            Random rand = new Random();
            Sponsor sponsorToShow = null;
            IEnumerable<Sponsor> sponsors = memberManager.Sponsors.Where(x => x.Approved);

            int count = sponsors.Count();

            if (count > 0)
            {
                sponsorToShow = sponsors.Skip(rand.Next(0, count)).First();
            }

            //If there is no sponsor, nothing can be shown. 
            if (sponsorToShow != null && sponsorToShow.Banner != null && sponsorToShow.Banner != "")
            {
                return PartialView(sponsorToShow);
            }
            else
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Selects the language given for the current user, or if not applicable, for the current session. 
        /// </summary>
        /// <param name="language">The language to set as given by the model returned by the 
        /// LanguageSelection action method. </param>
        /// <returns>An empty result.</returns>
        public EmptyResult SelectLangauge(string language)
        {
            LanguageItem newLanguage = LanguageManager.LanguageByCode(language);
            if (newLanguage != null)
            {
                LanguageManager.CurrentLanguage = newLanguage;
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Notifies the controller, that the achievement with the given id has been 
        /// shown to the user and should not been shown again.
        /// </summary>
        /// <param name="achievementId">The id of the achievement which has been shown as 
        /// given by the GetUserState action method. </param>
        /// <returns>An empty result.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.User)]
        public ActionResult SetAchievementShown(int achievementId)
        {
            AchievementManager achMan = new AchievementManager();
            User account = (User)SecurityProvider.CurrentUser;
            achMan.SetAchievementShown(account, achMan.GetAchievements(account).Where(a => a.AchievementId == achievementId).FirstOrDefault());

            return new EmptyResult();
        }

        /// <summary>
        /// Returns the SubMenuView using the corresponding MenuItem objects. 
        /// </summary>
        /// <returns>The SubMenuView.</returns>
        public PartialViewResult SubMenu()
        {
            MenuItem[] menu = null;
            if (SecurityProvider.Context != null)
            {
                switch (SecurityProvider.Context.UserPermissions)
                {
                    case SiteRoles.Admin:

                        //Sub menu for Admins

                        menu = new MenuItem[] {
                            new MenuItem() { LinkText = MenuText.AdminDashboard, LinkUrl = "~/AdminDashboard/AdminDashboard"}, 
                            new MenuItem() { LinkText = MenuText.AchievementConfiguration, LinkUrl = "~/AchievementConfiguration/AchievementOverview"}, 
                            new MenuItem() { LinkText = MenuText.NewsConfiguration, LinkUrl = "~/AppConfiguration/ConfigureNews"},
                            new MenuItem() { LinkText = MenuText.ErrorLog, LinkUrl = "~/AppConfiguration/ShowErrors"}, 
                            new MenuItem() { LinkText = MenuText.BannerRequests, LinkUrl = "~/PartnerSupport/BannerOverview"},
                            new MenuItem() { LinkText = MenuText.Rewards, LinkUrl = "~/PartnerSupport/RewardsOverview"}, 
                            new MenuItem() { LinkText = MenuText.MessageRequests, LinkUrl = "~/Support/MessageOverview"}
                        };

                        break;
                    case SiteRoles.CoinPartner:

                        //Sub menu for Coin Partners

                        menu = new MenuItem[] {  
                            new MenuItem() { LinkText = MenuText.PartnerDashboard, LinkUrl = "~/Partnership/PartnershipDashboard"}, 
                            new MenuItem() { LinkText = MenuText.Rewards, LinkUrl = "~/RewardConfiguration/RewardsOverview"}, 
                            new MenuItem() { LinkText = MenuText.PartnerHelp, LinkUrl = "~/Partnership/PartnershipHelp"},
                            new MenuItem() { LinkText = MenuText.PartnerFAQ, LinkUrl = "~/Partnership/PartnershipFAQ"}
                        };

                        break;
                    case SiteRoles.Sponsor:

                        //Sub menu for Sponsors

                        menu = new MenuItem[] {  
                            new MenuItem() { LinkText = MenuText.SponsorDashboard, LinkUrl = "~/Partnership/SponsorDashboard"},
                            new MenuItem() { LinkText = MenuText.SponsorHelp, LinkUrl = "~/Partnership/SponsorHelp"}
                        };

                        break;
                    case SiteRoles.User:

                        //Sub menu for Users

                        menu = new MenuItem[] {  
                            new MenuItem() { LinkText = MenuText.UserDashboard, LinkUrl = "~/UserDashboard/UserDashboard"},
                            new MenuItem() { LinkText = MenuText.Friends, LinkUrl = "~/Friends/FriendsOverview"},
                            new MenuItem() { LinkText = MenuText.UserAchievement, LinkUrl = "~/UserAchievement/Achievements"},
                            new MenuItem() { LinkText = MenuText.ProfileSettings, LinkUrl = "~/ProfileSettings/Settings"},
                            new MenuItem() { LinkText = MenuText.Statistics, LinkUrl = "~/Statistics/UserStatistics"},
                            new MenuItem() { LinkText = MenuText.Download, LinkUrl = "~/Public/ClientDownload"}
                       };

                        break;
                }
            }

            if (menu == null)
            {

                //Default (empty) sub menu

                menu = new MenuItem[] {
                };
            }

            return PartialView(menu);
        }
    }
}
