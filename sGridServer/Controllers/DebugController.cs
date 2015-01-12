using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sGridServer.Code.Security;
using sGridServer.Code.DataAccessLayer.Models;
using System.ComponentModel.DataAnnotations;
using sGridServer.Code.Achievements;
using sGridServer.Code.Rewards;
using sGridServer.Code.Achievements.ImplementedAchievements;
using sGridServer.Code.GridProviders;
using sGridServer.Code.GridProviders.WorldCommunityGrid;
using sGridServer.Code.DataAccessLayer;
//******************************************************
// This source file is only here for testing/debugging
// and will be deleted. 
//******************************************************
namespace sGridServer.Controllers
{
    /// <summary>
    /// TODO EMI: Delete when finished
    /// </summary>
    public class DebugController : Controller
    {
        private MemberManager memberManager;

        public DebugController()
        {
            memberManager = new MemberManager();
        }

        //
        // GET: /Debug/

        public ActionResult Index()
        {
            return View("Index", new DebugModel());
        }

        public ActionResult CreateDebugEnvironment()
        {
            if (memberManager.Users.Where(x => x.Nickname == "Frederik A.").Any())
            {
                return Redirect("Index"); 
            }

            User u1 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Frederik A.",
                EMail = "fred@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            }); 
            
            User u2 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Klaus U.",
                EMail = "klaus@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });
            
            User u3 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Heinz Y.",
                EMail = "heinz@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            }); 
            
            User u4 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Anna X.",
                EMail = "anna@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "en-US",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });

            User u5 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Lisa M.",
                EMail = "lisa@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });

            User u6 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Jerome U.",
                EMail = "jerome@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });

            User u7 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Ainara A.",
                EMail = "ainara@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });

            User u8 = memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Emi J.",
                EMail = "emi@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture(),
                ShowInHighScore = true,
                RegistrationDate = DateTime.Now.AddYears(-1)
            });

            memberManager.SetPassword(u1, "123456");
            memberManager.SetPassword(u2, "123456");
            memberManager.SetPassword(u3, "123456");
            memberManager.SetPassword(u4, "123456");

            CoinPartner c1 = memberManager.ElevateToCoinPartner(memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Bill G.",
                EMail = "bill@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "en-US",
                Picture = EMailIdProviderController.GetDefaultProfilePicture()
            }));

            memberManager.SetPassword(c1, "123456");
            c1.Description = "Coin Partner Bill Gates";
            c1.SecurityQuestion = "What is the first name of George W. Bush";
            c1.SecurityAnswer = "George";

            memberManager.SaveAccount(c1);

            Sponsor s1 = memberManager.ElevateToSponsor(memberManager.CreateUser(new User()
            {
                Active = true,
                Nickname = "Murphy D.",
                EMail = "dropkickmurphy@kit.edu",
                IdType = EMailIdProviderController.ProviderIdentifier,
                Culture = "de-DE",
                Picture = EMailIdProviderController.GetDefaultProfilePicture()
            }));

            memberManager.SetPassword(s1, "123456");

            AchievementManager achManager = new AchievementManager();
            string dummy;

            CoinsEarned ach1 = achManager.GetAchievementFromType("CoinsEarned") as CoinsEarned;
            ach1.Description = "Earn 100 Coins";
            ach1.BonusCoins = 50;
            ach1.AmountOfCoins = 100;
            ach1.Name = "Coin Earner";
            ach1.PrepareForSaving(out dummy);
            ach1.Icon = AchievementController.GetDefaultAchievementIcon();
            achManager.CreateAchievement(ach1);

            CoinsEarned ach2 = achManager.GetAchievementFromType("CoinsEarned") as CoinsEarned;
            ach2.Description = "Earn 200 Coins";
            ach2.BonusCoins = 50;
            ach2.AmountOfCoins = 200;
            ach2.Name = "Big Coin Earner";
            ach2.PrepareForSaving(out dummy);
            ach2.Icon = AchievementController.GetDefaultAchievementIcon();
            achManager.CreateAchievement(ach2);

            SpendCoins ach3 = achManager.GetAchievementFromType("SpendCoins") as SpendCoins;
            ach3.Description = "Spend 100 Coins";
            ach3.BonusCoins = 50;
            ach3.AmountOfCoins = 100;
            ach3.Name = "Shopper";
            ach3.PrepareForSaving(out dummy);
            ach3.Icon = AchievementController.GetDefaultAchievementIcon();
            achManager.CreateAchievement(ach3);

            RewardManager rewManager = new RewardManager();

            DummyReward rew1 = rewManager.GetRewardFromType("DummyReward") as DummyReward;
            rew1.Amount = 200;
            rew1.Begin = DateTime.Now;
            rew1.End = DateTime.Now.AddYears(1);
            rew1.Description = "Funny Dummy Reward";
            rew1.Name = "Dummy";
            rew1.ShortDescription = "Funny Dummy Reward";
            rew1.Cost = 10;
            rew1.CoinPartnerId = c1.Id;
            rew1.Approved = true;
            rew1.URL = "http://www.kit.edu";
            rew1.Picture = RewardController.GetDefaultRewardIcon();
            rew1.PrepareForSaving(out dummy);

            rewManager.CreateReward(rew1);
            rewManager.ApproveReward(rew1.Reward);

            DummyReward rew2 = rewManager.GetRewardFromType("DummyReward") as DummyReward;
            rew2.Amount = 200;
            rew2.Begin = DateTime.Now;
            rew2.End = DateTime.Now.AddYears(1);
            rew2.Description = "Funny Dummy Reward 2";
            rew2.Name = "Dummy 2";
            rew2.ShortDescription = "Funny Dummy Reward 2";
            rew2.Cost = 10;
            rew2.CoinPartnerId = c1.Id;
            rew2.Approved = true;
            rew2.URL = "http://www.kit.edu";
            rew2.Picture = RewardController.GetDefaultRewardIcon();
            rew2.PrepareForSaving(out dummy);

            rewManager.CreateReward(rew2);
            rewManager.ApproveReward(rew2.Reward);

            GridProjectDescription[] descs = sGridServer.Code.GridProviders.BoincProviders.SGridDemoProvider.Description.AvailableProjects;
            SGridDbContext db = new SGridDbContext();

            Random rand = new Random();
            User[] users = new User[] { u1, u2, u3, u4, u5, u6, u7, u8 };

            for (int j = 0; j < descs.Length; j++)
            {
                int resultCount = 0;
                int runtime = 0;

                for (DateTime dt = DateTime.Now.AddYears(-1); dt < DateTime.Now; dt = dt.AddHours(8))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (rand.Next(0, 100) > 95)
                        {
                            db.CalculatedResults.Add(new CalculatedResult() {
                                ServerReceivedTimestamp = dt,
                                ClientSentTimestamp = dt,
                                ProjectShortName = descs[j].ShortName,
                                ServerSentTimestamp = dt.AddDays(-3),
                                ClientReceivedTimestamp = dt.AddDays(-3),
                                ValidatedTimestamp = dt,
                                UserId = users[i].Id,
                                Valid = true
                            });

                            runtime += descs[j].AverageCalculationTime;
                            resultCount++;
                        }
                    }
                }
            }

            db.SaveChanges();

            return Redirect("Index");
        }

        public ActionResult LoginAsUser()
        {
            User u = memberManager.Users.FirstOrDefault();

            if (u == null)
            {
                ViewBag.ErrorMessage = "Kein User vorhanden";
            }
            else
            {
                SecurityProvider.LogIn(new UserContext(u));
            }

            return Redirect("Index");
        }

        public ActionResult LoginAsPartner()
        {
            CoinPartner p = memberManager.CoinPartners.FirstOrDefault();

            if (p == null)
            {
                ViewBag.ErrorMessage = "Kein Partner vorhanden";
            }
            else
            {
                SecurityProvider.LogIn(new UserContext(p));
            }

            return Redirect("Index");
        }

        public ActionResult LoginAsSponsor()
        {
            Sponsor s = memberManager.Sponsors.FirstOrDefault();

            if (s == null)
            {
                ViewBag.ErrorMessage = "Kein Sponsor vorhanden";
            }
            else
            {
                SecurityProvider.LogIn(new UserContext(s));
            }

            return Redirect("Index");
        }

        public ActionResult LoginAsAdmin()
        {
            SGridTeamMember s = memberManager.SGridMembers.FirstOrDefault();

            if (s == null)
            {
                ViewBag.ErrorMessage = "Kein Admin vorhanden";
            }
            else
            {
                SecurityProvider.LogIn(new UserContext(s));
            }

            return Redirect("Index");
        }

        [SGridAuthorize(RequiredPermissions = SiteRoles.CoinPartner)]
        public ActionResult SetSecurityQuestion(string question, string answer)
        {
            CoinPartner partner = SecurityProvider.CurrentUser as CoinPartner;

            partner.SecurityQuestion = question;
            partner.SecurityAnswer = answer;

            memberManager.SaveAccount(partner);

            return Redirect("Index");
        }

        public ActionResult LoginAsNewUser(DebugModel model)
        {
            User u = CreateUser(model);

            SecurityProvider.LogIn(new UserContext(u));

            return new EmptyResult();
        }

        public ActionResult LoginAsNewPartner(DebugModel model)
        {
            User u = CreateUser(model);

            CoinPartner p = memberManager.ElevateToCoinPartner(u);

            SecurityProvider.LogIn(new UserContext(p));

            return new EmptyResult();
        }

        public ActionResult LoginAsNewSponsor(DebugModel model)
        {
            User u = CreateUser(model);

            Sponsor s = memberManager.ElevateToSponsor(u);

            SecurityProvider.LogIn(new UserContext(s));

            return new EmptyResult();
        }

        public ActionResult LoginAsNewAdmin(DebugModel model)
        {
            User u = CreateUser(model);

            SGridTeamMember s = memberManager.ElevateToTeamMember(u);

            SecurityProvider.LogIn(new UserContext(s));

            return new EmptyResult();
        }

        public ActionResult ElevateMeToPartner()
        {
            User u = SecurityProvider.CurrentUser as User;

            CoinPartner p = memberManager.ElevateToCoinPartner(u);

            SecurityProvider.LogIn(new UserContext(p));

            return Redirect("Index");
        }

        public ActionResult ElevateMeToSponsor()
        {
            User u = SecurityProvider.CurrentUser as User;

            Sponsor s = memberManager.ElevateToSponsor(u);

            SecurityProvider.LogIn(new UserContext(s));

            return Redirect("Index");
        }

        public ActionResult ElevateMeToAdmin()
        {
            User u = SecurityProvider.CurrentUser as User;

            SGridTeamMember s = memberManager.ElevateToTeamMember(u);

            SecurityProvider.LogIn(new UserContext(s));

            return Redirect("Index");
        }

        private User CreateUser(DebugModel model)
        {
            User u =  memberManager.CreateUser(new User() { EMail = model.Address, Nickname = model.Name, Active = true });
          
            return u;
        }

        public ActionResult GiveCoins()
        {
            new sGridServer.Code.CoinExchange.CoinExchange(SecurityProvider.CurrentUser as User).Grant(new sGridServer.Code.GridProviders.GridProjectDescription("Debug", "Debug", "Debug", "", "", "", "", 0, 0), 100);

            return Redirect("Index");
        
        }
    }

    public class DebugModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Address { get; set; }

    }
}
