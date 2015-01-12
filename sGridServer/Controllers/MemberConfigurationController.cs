using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Security;
using sGridServer.Code.Utilities;
using Resource = sGridServer.Resources.MemberConfiguration.MemberConfiguration;
using sGridServer.Models;
using sGridServer.Properties;

namespace sGridServer.Controllers
{
    /// <summary>
    /// This controller provides functions to view and edit sGrid accounts of each type.
    /// It also provides functions to change a user to other account types.
    /// </summary>
    public class MemberConfigurationController : Controller
    {
        private MemberManager memberManager;
        /// <summary>
        /// Set the memberManager for thi controller.
        /// </summary>
        public MemberConfigurationController()
        {
            memberManager = new MemberManager();
        }

        /// <summary>
        /// Returns the DetailsView for the account with the given id.
        /// </summary>
        /// <param name="id">The identifier of the account to get the details for.</param>
        /// <returns>The DetailsView of the given account.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult Details(int id)
        {
            Account account = memberManager.GetAccountById(id);
            return View(account);
        }
        /// <summary>
        /// Starts the edit process for the account with the given id and, depending on the account type,
        /// calls one of EditPartner, EditSponsor, EditTeamMember or EditUser with the account loaded from the database.
        /// </summary>
        /// <param name="id">The identifier of the account to edit.</param>
        /// <returns>The edit view (corresponding to the user type value of the given account).</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult Edit(int? id)
        {
            Account account;
            SiteRoles type;
            if (id != null)
            {
                account = memberManager.GetAccountById(id.Value);                
            }
            else
            {
                account = new User();
                account.IdType = EMailIdProviderController.ProviderIdentifier;
            }
            type = account.UserPermission;
            switch (type)
            {
                case SiteRoles.Admin:
                    return View("EditTeamMember", new TeamMemberConfigurationModel() { Account = account as SGridTeamMember });
                case SiteRoles.CoinPartner:
                    return View("EditPartner", new PartnerConfigurationModel() { Account = account as CoinPartner });
                case SiteRoles.Sponsor:
                    return View("EditSponsor", new SponsorConfigurationModel(){ Account = account as Sponsor });
                default:
                    {
                        if (account.IdType == EMailIdProviderController.ProviderIdentifier)
                        {
                            return View("EditUser", new UserConfigurationModel() { Account = account as User, ShowPasswordField = true });
                        }
                        else
                        {
                            return View("EditUser", new UserConfigurationModel() { Account = account as User });
                        }
                    }
            }
        }
        /// <summary>
        /// Shows the partial view to edit an account.
        /// </summary>
        /// <param name="id">The identifier of the account to edit.</param>
        /// <returns>The partial EditAccountView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult EditAccount(Account account)
        {
            return PartialView(new AccountConfigurationModel(account));
        }
        /// <summary>
        /// Replace stored-account properties with properties of the account edited by user.  
        /// </summary>
        /// <param name="edited">The account currently stored in the database.</param>
        /// <param name="stored">The account edited by the user.</param>
        /// <returns>The EditAccountView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        private void EditAccount(Account edited, Account stored)
        {
            stored.Active = edited.Active;
            stored.Nickname = edited.Nickname;
            stored.Culture = edited.Culture;
            stored.EMail = edited.EMail;
            stored.IdType = edited.IdType;
            stored.NotifyOnAchievementReached = edited.NotifyOnAchievementReached;
            stored.NotifyOnCoinBalanceChanged = edited.NotifyOnCoinBalanceChanged;
            stored.NotifyOnProjectChanged = edited.NotifyOnProjectChanged;
            stored.Privacy = edited.Privacy;
            stored.ShowInHighScore = edited.ShowInHighScore;
            if (edited.Picture == "")
            {
                if (stored.Picture != "")
                {
                    BlobStorage storage = new BlobStorage(Settings.Default.ProfilePictureStorageContainer);
                    storage.RemoveBlob(stored.Picture);
                }
            }
        }
        /// <summary>
        /// Validates the account edited by the user.
        /// </summary>
        /// <param name="edited">The account edited by the user.</param>
        /// <param name="stored">The account currently stored in the database.</param>
        /// <param name="createdAccount">The "edited" account is the account to be created.</param>
        /// <returns>True, if account is valid, false, otherwise</returns>
        private bool ValidateAccount(Account edited, Account stored, bool createdAccount)
        {
            bool status = true;
            //Check whether the name of the edited user is already in use.
            int countSameNames = memberManager.Accounts.Where(x => x.Nickname.ToLower() == edited.Nickname.ToLower()).Count();
            if (countSameNames > 1 && edited.Nickname != stored.Nickname)
            {
                ViewBag.UsernameMessage = Resource.UsernameAlreadyInUse;
                status = false;
            }
            else if (countSameNames > 0 && createdAccount == true)
            {
                ViewBag.UsernameMessage = Resource.UsernameAlreadyInUse;
                status = false;
            }
            //Check whether the EMail of the edited user is already in use.
            int countSameEmails = memberManager.Accounts.Where(x => x.EMail.ToLower() == edited.EMail.ToLower()).Count();
            if (countSameEmails > 1 && edited.EMail != stored.EMail)
            {
                ViewBag.EMailMessage = Resource.EMailAlreadyInUse;
                status = false;
            }
            else if (countSameEmails > 0 && createdAccount == true)
            {
                ViewBag.UsernameMessage = Resource.UsernameAlreadyInUse;
                status = false;
            }
            return status;
        }
        /// <summary>
        /// Validates and stores the changes. Shows the Details view for the given coin partner afterwards. 
        /// </summary>
        /// <param name="model">The model containing a partner to store.</param>
        /// <returns>The DetailsView for the partner.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditPartner(PartnerConfigurationModel model)
        {
            CoinPartner partner = model.Account;
            //Check whether the model is valid.
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            CoinPartner account = memberManager.GetAccountById(partner.Id) as CoinPartner;
            //Check if edited account is valid.
            if (!ValidateAccount(partner, account, false))
            {
                return View(model);
            }

            if (account != null)
            {
                //Set the properties and logo of the stored account to the properties of an edited account.
                account.Description = partner.Description;
                account.Link = partner.Link;
                if (partner.SecurityQuestion != null && partner.SecurityQuestion != "" && partner.SecurityAnswer != null && partner.SecurityAnswer != "")
                {
                    account.SecurityQuestion = partner.SecurityQuestion;
                    account.SecurityAnswer = partner.SecurityAnswer;
                }
                if (partner.Logo == "")
                {
                    if (account.Logo != "")
                    {
                        BlobStorage storage = new BlobStorage(Settings.Default.PartnerStorageContainer);
                        storage.RemoveBlob(account.Logo);
                    }
                }
                //Edit account properties.
                EditAccount(partner, account);

                memberManager.SaveAccount(account);
            }
            return View("Details", account);
        }
        /// <summary>
        /// Validates and stores the changes. Shows the Details view for the given sponsor afterwards. 
        /// </summary>
        /// <param name="model">The model containing sponsor to edit.</param>
        /// <returns>The DetailsView for the sponsor.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditSponsor(SponsorConfigurationModel model)
        {
            Sponsor sponsor = model.Account;
            //Check whether the model is valid.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Sponsor account = memberManager.GetAccountById(sponsor.Id) as Sponsor;
            //Check if edited account is valid.
            if (!ValidateAccount(sponsor, account, false))
            {
                return View(model);
            }

            if (account != null)
            {
                //Set the properties, banner and logo of the stored account to the properties of an edited account.
                account.Description = sponsor.Description;
                account.Link = sponsor.Link;
                if (sponsor.Logo == "")
                {
                    if (account.Logo != "")
                    {
                        BlobStorage storage = new BlobStorage(Settings.Default.PartnerStorageContainer);
                        storage.RemoveBlob(account.Logo);
                    }
                }
                if (sponsor.Banner == "")
                {
                    if (account.Banner != "")
                    {
                        BlobStorage storage = new BlobStorage(Settings.Default.PartnerStorageContainer);
                        storage.RemoveBlob(account.Banner);
                    }
                }
                //Edit account properties
                EditAccount(sponsor, account);

                memberManager.SaveAccount(account);
            }
            return View("Details", account);
        }
        /// <summary>
        /// Validates and stores the changes. Shows the Details view for the given team member afterwards.
        /// </summary>
        /// <param name="model">The model containing sGrid team member to edit.</param>
        /// <returns>The DetailsView for the team member.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditTeamMember(TeamMemberConfigurationModel model)
        {
            SGridTeamMember member = model.Account;
            //Check whether the model is valid.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SGridTeamMember account = memberManager.GetAccountById(member.Id) as SGridTeamMember;
            //Check if edited account is valid.
            if (!ValidateAccount(member, account, false))
            {
                return View(model);
            }
            //Edit account properties.
            EditAccount(member, account);
            memberManager.SaveAccount(account);

            return View("Details", member);
        }
        /// <summary>
        /// Validates and stores the changes. Shows the Details view for the given user afterwards.
        /// </summary>
        /// <param name="model">The model containing user to edit.</param>
        /// <returns>The DetailsView for the user.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        [HttpPost]
        public ActionResult EditUser(UserConfigurationModel model)
        {
            User user = model.Account as User;
            User account;
            bool userIsNew;
            if (user.Id != -1)
            {
                account = memberManager.GetAccountById(user.Id) as User;
                userIsNew = false;
            }
            else
            {
                account = new User();
                userIsNew = true;
            }
            String toElevate = model.SiteRole;
            //ModelState
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else if (model.NewPassword != null && model.NewPassword != "" && (model.NewPassword.Length < 6 || model.NewPassword != model.NewPasswordConfirm))
            {
                return View(model);
            }
            else if (userIsNew && model.NewPassword == null || model.NewPassword == "")
            {
                return View(model);
            }
            //Check if edited account is valid.
            else if (!ValidateAccount(user, account, userIsNew))
            {
                return View(model);
            }           
            

            if (account != null)
            {

                EditAccount(user, account);
                if (user.Id == -1)
                {
                    account.Picture = IdProviderController.GetDefaultProfilePicture();
                    memberManager.CreateUser(account);
                }
                else
                {
                    memberManager.SaveAccount(account);
                }
                    //Elevate user to Sponsor, Team Member or Coin Partner, if toElevate is setted.a
                if (toElevate != null)
                {
                    if (toElevate.ToLower() == "coinpartner")
                    {
                        memberManager.ElevateToCoinPartner(account);
                    }
                    else if (toElevate.ToLower() == "sponsor")
                    {
                        memberManager.ElevateToSponsor(account);
                    }
                    else if (toElevate.ToLower() == "sgridteammember")
                    {
                        memberManager.ElevateToTeamMember(account);
                    }
                }
                if (model.NewPassword != null && model.NewPassword != "")
                {
                    if (model.NewPassword == model.NewPasswordConfirm)
                    {
                        memberManager.SetPassword(account, model.NewPassword);
                    }
                }
            }
            return View("Details", account);
        }
        /// <summary>
        /// Returns the partial ListView containing a list of accounts according to the given parameters.
        /// </summary>
        /// <param name="searchName">If this parameter is set, only accounts which names match the given string are returned.</param>
        /// <param name="userType">if this parameter is set, only accounts with the specific userType are returned.</param>
        /// <param name="page">if this parameter is set, only accounts on the specific page are shown.</param>
        /// <returns>The partial ListView according to the given parameter.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult List(String searchName, String userType, int? page)
        {
            MemberManager manager = new MemberManager();
            //first page is shown by default.
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Account> accounts = manager.Accounts;
            //Get accounts which name translation match the given string.
            if (searchName != null && searchName != "")
            {
                accounts = accounts.Where(u => u.Nickname.ToLowerInvariant().Contains(searchName.ToLowerInvariant()) || u.EMail.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
            }
            //Get accounts of the specified user type
            if (userType != "All")
            {
                if (userType == "Users")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.User));
                }
                else if (userType == "Administrators")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.Admin));
                }
                else if (userType == "Sponsors")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.Sponsor));
                }
                else if (userType == "CoinPartners")
                {
                    accounts = accounts.Where(u => u.UserPermission == (SiteRoles.CoinPartner));
                }
            }
            //Get the account for the specific page number.
            accounts = accounts.Skip(6 * (page.Value - 1)).Take(6);
            return PartialView(accounts);
        }
        /// <summary>
        /// Shows the MemberOverviewView.
        /// </summary>
        /// <returns>The MemberOverviewView.</returns>
        [SGridAuthorize(RequiredPermissions = SiteRoles.Admin)]
        public ActionResult MemberOverview()
        {
            return View();
        }
    }
}
