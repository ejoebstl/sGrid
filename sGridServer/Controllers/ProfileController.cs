using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Models;
using sGridServer.Code.GridProviders;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller is responsible for showing the profile views.
    /// It provides functions to view the users’ profiles and to search for and list users.
    /// </summary>
    public class ProfileController : Controller
    {
        /// <summary>
        /// Checks whether the user profile of the user with the given id can be shown to the current user.
        /// </summary>
        /// <param name="currentId">The id of the current user.</param>
        /// <param name="checkId">The id of the user whose profile needs to be viewed.</param>
        /// <returns>True or false depending on whether the information can be shown.</returns>
        private bool CanShowInformation(int currentId, int checkId)
        {
            MemberManager manager = new MemberManager();
            Account user = manager.GetAccountById(checkId);

            return !(((user.Privacy == PrivacyLevel.Private && (currentId != checkId))
                    || (user.Privacy == PrivacyLevel.Registered && (currentId == -1))));            
        }
        /// <summary>
        /// Returns the detailed view of the given profile.
        /// </summary>
        /// <param name="id">The identifier of the profile to get the details for.</param>
        /// <returns>The ProfileDetailView for the given profile or the error view if the profile can not be shown.</returns>  
        public ActionResult ProfileDetail(int id)
        {
            Account current = SecurityProvider.CurrentUser; 
            int currentId = ((current != null) ? current.Id : -1);
            
            MemberManager manager = new MemberManager();
            Account user = manager.GetAccountById(id);
            //CHeck if this profile can be shown to the current user.
            bool show = CanShowInformation(currentId, id) && user.Active;
            int friendsNumber = -1;
            bool showAddButton = false;
            bool showRemoveButton = false;
            if (user is User)
            {
                friendsNumber = manager.GetFriends(user as User).Count();
                if (current is User)
                {
                    showAddButton = true;
                    showRemoveButton = true;

                    User sender = current as User;
                    User receiver = user as User;
                    //Set bools corresponding to add and remove friend buttons to true, if these buttons have to be shown in the view.
                    if (current.Id == user.Id || (manager.GetFriendRequest(sender, receiver) != null && !manager.AreFriends(sender, receiver)))
                    {                        
                        showAddButton = false;
                        showRemoveButton = false;
                    } else if (manager.GetFriendRequest(sender, receiver) == null && !manager.AreFriends(sender, receiver))
                    {
                        showRemoveButton = false;
                    }
                    else if (manager.AreFriends(sender, receiver))
                    {
                        showAddButton = false;
                    }
                }
            }
            //Generate model for the view.
            ProfileDetailModel model = new ProfileDetailModel(){
                                                                Account = user,
                                                                ShowAddFriendButton = showAddButton,
                                                                ShowRemoveFriendButton = showRemoveButton,
                                                                FriendsNumber = friendsNumber
                                                                };            
            return (show ? View(model) : (ActionResult) RedirectToAction("ProfileError"));
        }
        /// <summary>
        /// Shows the error view.
        /// </summary>
        /// <returns>Returns the error view.</returns>
        public ActionResult ProfileError()
        {
            return View();
        }
        /// <summary>
        /// Shows the ProfileOverviewView.
        /// </summary>
        /// <returns>The ProfileOverviewView.</returns>
        public ActionResult ProfileOverview()
        {
            MemberManager manager = new MemberManager();
            int showTo = (SecurityProvider.Context != null) ? SecurityProvider.Context.ID : -1;
            //Get the number of pages that have to be shown.
            int count = manager.Accounts.Where(u => u.Active && CanShowInformation(showTo, u.Id)).Count();
            count = count / 6 + ((count % 6 != 0) ? 1 : 0); 
            return View(count);
        }
        /// <summary>
        /// Returns the ListProfileView according to the given parameters.
        /// </summary>
        /// <param name="searchName">If this parameter is set, only profiles which nicknames or email addresses match the given string are returned.</param>
        /// <param name="searchOption">If this parameter is set, only profiles of a specific type are returned.</param>
        /// <param name="sortOption">A String which specifies the attribute the profiles should be sorted by.</param>
        /// <param name="page">This parameter gives the number of page that has to be shown.</param>
        /// <returns></returns>
        public ActionResult ListProfile(int? page, String searchName, String searchOption, String sortOption)
        {
            MemberManager manager = new MemberManager();
            if (page == null)
            {
                page = 1;
            }
            int showTo = (SecurityProvider.Context != null) ? SecurityProvider.Context.ID : -1;
            IEnumerable<Account> accounts = manager.Accounts.Where(u => u.Active && CanShowInformation(showTo, u.Id));            
            //if searchName is set, search for the accounts, that match the given string.
            if (searchName != null && searchName != "")
            {
                accounts = accounts.Where(u => (u.Nickname.ToLowerInvariant().Contains(searchName.ToLowerInvariant())  || u.EMail.ToLowerInvariant().Contains(searchName.ToLowerInvariant())));
            }
            //if searchOption is set, search for the accounts, that match the given account type.
            if (searchOption != "All")
            {
                if (searchOption == "Users")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.User));
                }
                else if (searchOption == "Administrators")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.Admin));
                }
                else if (searchOption == "Sponsors")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.Sponsor));
                }
                else if (searchOption == "Coin Partners")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.CoinPartner));
                }
            }
            //if sortOption is set, search for the accounts, that match the given account type.
            if (sortOption == "Chronologically")
            {
                //Newest first
                accounts = accounts.OrderByDescending(u => u.RegistrationDate);
            }
            else
            {
                accounts = accounts.OrderBy(u => u.Nickname);
            }
            //get the accounts to be shown on a page with a provided page number.
            accounts = accounts.Skip(6 * (page.Value - 1)).Take(6);
            //generate a userScore list containing users and the number of projects they have done.
            List<UserScore> userScores = new List<UserScore>();
            GridProviderManager providerManager;
            int calculatedPackages;
            foreach (Account a in accounts)
            {
                //The number of packages done by team members, coin partners or sponsors is 0 by default.
                calculatedPackages = 0;
                
                if (a.UserPermission == SiteRoles.User)
                {   providerManager = new GridProviderManager((User)manager.GetAccountById(a.Id));
                    calculatedPackages = providerManager.CurrentSummary.ResultCount;
                }
                userScores.Add(new UserScore(calculatedPackages, a));
            }
            return PartialView("ListProfile", userScores);
        }
    }
}
