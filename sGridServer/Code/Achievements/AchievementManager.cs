using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using sGridServer.Code.Security;
using sGridServer.Code.GridProviders;
using sGridServer.Code.CoinExchange;
using sGridServer.Code.Utilities;
using System.Collections.Concurrent;
using System.Web.Mvc;
using sGridServer.Code.Achievements.ImplementedAchievements;

namespace sGridServer.Code.Achievements
{
    /// <summary>
    /// This class provides various methods to get information about
    /// achievements and to interact with them. Additionally this class
    /// manages incoming events that might trigger an achievement and it
    /// raises an event if an achievement gets obtained.
    /// </summary>
    public class AchievementManager
    {
        /// <summary>
        /// Maps the description of the achievement types to the
        /// Achievement objects themselves. It is initialized during the
        /// static constructor and then it remains unchanged. The
        /// contained Achievement objects are used in order to get a new
        /// instance of a certain achievement type.
        /// </summary>
        private static ConcurrentDictionary<String, Type> achievementTypes;

        /// <summary>
        /// A timer throwing an event all 10 minutes in order that
        /// certain achievements can be tested.
        /// </summary>
        private static Timer timer;

        /// <summary>
        /// static constructor
        /// </summary>
        static AchievementManager()
        {
            achievementTypes = new ConcurrentDictionary<string,Type>();
            InitAchievementTypes();
            timer = new Timer(new TimeSpan(0, 10, 0));
            RegisterEvents();
        }
        
        /// <summary>
        /// Initializes the Dictionary AchievementTypes. This method is
        ///executed by the static constructor.
        /// </summary>
        private static void InitAchievementTypes() 
        {
            //achievementTypes[new DummyAchievement().AchievementType] = typeof(DummyAchievement);
            achievementTypes[new CalculatePackagesGeneral().AchievementType] = typeof(CalculatePackagesGeneral);
            achievementTypes[new CalculatePackagesForProject().AchievementType] = typeof(CalculatePackagesForProject);
            achievementTypes[new FastCalculator().AchievementType] = typeof(FastCalculator);
            achievementTypes[new MultipleClients().AchievementType] = typeof(MultipleClients);
            achievementTypes[new BuyRewardsGeneral().AchievementType] = typeof(BuyRewardsGeneral);
            achievementTypes[new BuyRewardFromPartner().AchievementType] = typeof(BuyRewardFromPartner);
            achievementTypes[new SpendCoins().AchievementType] = typeof(SpendCoins);
            achievementTypes[new SpendCoinsFast().AchievementType] = typeof(SpendCoinsFast);
            achievementTypes[new CoinsEarned().AchievementType] = typeof(CoinsEarned);
            achievementTypes[new CoinAccountBalance().AchievementType] = typeof(CoinAccountBalance);
            achievementTypes[new MembershipTime().AchievementType] = typeof(MembershipTime);
            achievementTypes[new HighscorePlacing().AchievementType] = typeof(HighscorePlacing);
            achievementTypes[new FriendsInvited().AchievementType] = typeof(FriendsInvited);
            achievementTypes[new AchievementsObtained().AchievementType] = typeof(AchievementsObtained);
        }

        /// <summary>
        /// Binds the event handlers with the different events used by
        /// the AchievementManager. This method is executed by the
        /// static constructor.
        /// </summary>
        private static void RegisterEvents()
        {
            AchievementItem.AchievementObtained += new EventHandler<AchievementEventArgs>(OnAchievementAccomplished);
            GridProvider.ResultStateChanged += new EventHandler<ResultStateChangedEventArgs>(OnClientCountChanged);
            CoinExchange.CoinExchange.TransactionDone += new EventHandler<TransactionEventArgs>(OnCoinTransfer);
            MemberManager.FriendshipAdded += new EventHandler<FriendshipAddedEventArgs>(OnFriendshipAdded);
            timer.Tick += new EventHandler(OnTenMinutesUp);
        }

        /// <summary>
        /// This event handler is triggered by the AchievementManager
        /// class when an achievement has been accomplished.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the achievement and the user.</param>
        private static void OnAchievementAccomplished(Object sender, AchievementEventArgs data)
        {
            //Gather all achievements types which match the rised event. 
            SGridDbContext dbContext = new SGridDbContext();
            List<String> list = new List<String>();
            list.Add(new AchievementsObtained().AchievementType);

            //Now get all achievements for the type and test each for accomplishment. 
            IEnumerable<Achievement> achis = new AchievementManager()
                .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

            foreach (Achievement ach in achis)
            {
                AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                achItem.Test((User)data.User);
            }
        }

        /// <summary>
        /// This event handler is triggered by the GridProvider class
        /// when the data of a user related to a grid provider changes,
        /// for example the number of clients registered with the
        /// provider. 
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the user, the ProjectPerformanceData
        /// object and the GridPerformanceData object.</param>
        private static void OnClientCountChanged(Object sender, ResultStateChangedEventArgs data)
        {
            //Gather all achievements types which match the rised event. 
            SGridDbContext dbContext = new SGridDbContext();
            List<String> list = new List<String>();
            list.Add(new MultipleClients().AchievementType);

            //Now get all achievements for the type and test each for accomplishment. 
            IEnumerable<Achievement> achis = new AchievementManager()
                .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

            foreach (Achievement ach in achis)
            {
                AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                achItem.Test((User)data.User);
            }
        }

        /// <summary>
        /// This event handler is triggered by the CoinExchange class
        /// when a coin transfer happens. 
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the description of the coin transfer, the
        /// amount of coins transferred and the source and destination
        /// accounts.</param>
        private static void OnCoinTransfer(Object sender, TransactionEventArgs data)
        {
            //Decide whether coins were granted or spent. 
            SGridDbContext dbContext = new SGridDbContext();
            List<String> list = new List<String>();
            if (data.Destination is User)
            {
                //Gather all achievements types which match the rised event. 
                list.Add(new CalculatePackagesGeneral().AchievementType);
                list.Add(new CalculatePackagesForProject().AchievementType);
                list.Add(new CoinAccountBalance().AchievementType);
                list.Add(new CoinsEarned().AchievementType);
                list.Add(new FastCalculator().AchievementType);
                list.Add(new HighscorePlacing().AchievementType);

                //Now get all achievements for the type and test each for accomplishment. 
                IEnumerable<Achievement> achis = new AchievementManager()
                    .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

                foreach (Achievement ach in achis)
                {
                    AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                    achItem.Test((User)data.Destination);
                }
            }
            else if (data.Source is User)
            {
                //Gather all achievements types which match the rised event. 
                list.Add(new BuyRewardsGeneral().AchievementType);
                list.Add(new BuyRewardFromPartner().AchievementType);
                list.Add(new SpendCoins().AchievementType);
                list.Add(new SpendCoinsFast().AchievementType);

                //Now get all achievements for the type and test each for accomplishment. 
                IEnumerable<Achievement> achis = new AchievementManager()
                    .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

                foreach (Achievement ach in achis)
                {
                    AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                    achItem.Test((User)data.Source);
                }
            }
        }

        /// <summary>
        /// This event handler is triggered by the MemberManager class
        /// when a friendship between two users is added. 
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">The arguments that are transferred with
        /// this event, namely the two Users and the information if one
        /// of the users invited the other one.</param>
        private static void OnFriendshipAdded(Object sender, FriendshipAddedEventArgs data)
        {
            //Gather all achievements types which match the rised event. 
            SGridDbContext dbContext = new SGridDbContext();
            List<String> list = new List<String>();
            list.Add(new FriendsInvited().AchievementType);

            //Now get all achievements for the type and test each for accomplishment. 
            IEnumerable<Achievement> achis = new AchievementManager()
                .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

            foreach (Achievement ach in achis)
            {
                AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                achItem.Test((User)data.A);
            }
        }

        /// <summary>
        /// This event handler is triggered by the Timer object when ten
        /// minutes since the last time this event has occurred have
        /// elapsed. 
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="data">Placeholder, just in case this event
        /// later needs arguments to be passed.</param>
        private static void OnTenMinutesUp(Object sender, EventArgs data)
        {
            //Gather all achievements types which match the rised event. 
            SGridDbContext dbContext = new SGridDbContext();
            List<String> list = new List<String>();
            list.Add(new MembershipTime().AchievementType);

            //Now get all achievements for the type and test each for accomplishment. 
            IEnumerable<Achievement> achis = new AchievementManager()
                .GetAllExistingAchievements().Where(a => list.Contains(a.AchievementType));

            MemberManager memMan = new MemberManager();
            foreach (Achievement ach in achis)
            {
                AchievementItem achItem = AchievementItem.ToAchievementItem(ach);
                foreach (User user in memMan.Users)
                    achItem.Test(user);
            }
        }

        /// <summary>
        /// Creates a new achievement an saves it.
        /// Note: This method is meant to be called by the Submit
        /// method in the AchievementController.
        /// </summary>
        /// <param name="achItem">The achievement to be saved.</param>
        public void CreateAchievement(AchievementItem achItem)
        {
            SGridDbContext dbContext = new SGridDbContext();
            Achievement achievement = achItem.Achievement;

            //Reattach the achivement and it's multi language strings. 
            dbContext.Achievements.Add(achievement);
            achievement.Name.Reattach(dbContext);
            achievement.Description.Reattach(dbContext);

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Changes an existing achievement.
        /// Note: This method is meant to be called by the Submit
        /// method in the AchievementController.
        /// </summary>
        /// <param name="achItem">The achievement to be changed.</param>
        public void EditAchievement(AchievementItem achItem)
        {
            SGridDbContext dbContext = new SGridDbContext();
            Achievement achievement = achItem.Achievement;

            Achievement ach = (from Achievement a in dbContext.Achievements
                                   where a.AchievementId == achievement.AchievementId
                                   select a).FirstOrDefault();

            //Reattach the achivement and it's multi language strings. 
            dbContext.Entry(ach).CurrentValues.SetValues(achievement);
            achievement.Name.Reattach(dbContext);
            achievement.Description.Reattach(dbContext);

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Gets an Achievement with a given id.
        /// </summary>
        /// <param name="id">The id of the achievement.</param>
        /// <returns>The requested achievement, or null, if for this id
        /// no achievement exists.</returns>
        public Achievement GetAchievementById(int id)
        {
            SGridDbContext dbContext = new SGridDbContext();
            IEnumerable<Achievement> achis = (from Achievement in dbContext.Achievements.AsNoTracking()
                                              where Achievement.AchievementId == id
                                              select Achievement);
            if (achis.Any())
            {
                return achis.Single();
            }
            return null;
        }

        /// <summary>
        /// Returns an AchievementItem from a given type.
        /// </summary>
        /// <param name="achievementType">The type an
        /// AchievementItem is searched for.</param>
        /// <returns>An AchievementItem of the given type or null,
        /// if the type is not known.</returns>
        public AchievementItem GetAchievementFromType(String achievementType)
        {
            if (achievementTypes.ContainsKey(achievementType))
            {
                AchievementItem item = (AchievementItem)Activator.CreateInstance(achievementTypes[achievementType]);
                item.Description = new MultiLanguageString();
                item.Name = new MultiLanguageString();

                return item;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and returns an enumeration of the achievements a
        /// user has achieved. 
        /// </summary>
        /// <param name="user">The user for whom the achievements are
        /// searched for. </param>
        /// <returns>The achievements of the user.</returns>
        public IEnumerable<Achievement> GetAchievements(User user)
        {
            SGridDbContext dbContext = new SGridDbContext();
            return (from obtainedAchievement in dbContext.ObtainedAchievements.AsNoTracking()
                    where obtainedAchievement.User.Id == user.Id
                    select obtainedAchievement.Achievement);
        }

        /// <summary>
        /// Creates and returns an enumeration of the different existing
        /// types of AchievementItems described by strings.
        /// </summary>
        /// <returns>An enumeration of the descriptions of all existing
        /// types of AchievementItem objects.</returns>
        public IEnumerable<String> GetAllAchievementTypes()
        {
            return achievementTypes.Keys;
        }

        /// <summary>
        /// Creates and returns an enumeration containing every achievement.
        /// </summary>
        /// <returns>An enumeration of all Achievements.</returns>
        public IEnumerable<Achievement> GetAllExistingAchievements()
        {
            SGridDbContext dbContext = new SGridDbContext();
            return dbContext.Achievements
                .AsNoTracking();
        }

        /// <summary>
        /// Gets the url pointing at the AchievementCreateForm method of
        /// the AchievementController. This method is used when an
        /// achievement should be created.
        /// </summary>
        /// <param name="returnUrl">The url to return to after the
        /// achievement creation finishes.</param>
        /// <param name="achievementType">The achievement type to create.</param>
        /// <param name="context">The context of the controller invoking
        /// this method.</param>
        /// <returns>The url pointing at the AchievementCreateForm method
        /// of the AchievementController.</returns>
        public String GetCreateUrl(String returnUrl, String achievementType, ControllerContext context)
        {
            UrlHelper u = new UrlHelper(context.RequestContext);
            return u.Action("AchievementCreateForm", "Achievement",
                new { returnUrl = returnUrl,
                    achievementType = achievementType,
                    achievementId = -1 });
        }

        /// <summary>
        /// Gets the url pointing at the AchievementCreateForm method of
        /// the AchievementController. This method is used when an
        /// achievement should be edited.
        /// </summary>
        /// <param name="returnUrl">The url to return to after the
        /// achievement editing finishes.</param>
        /// <param name="achievement">The achivement to change.</param>
        /// <param name="context">The context of the controller invoking
        /// this method.</param>
        /// <returns>The url pointing at the AchievementCreateForm method
        /// of the AchievementController.</returns>
        public String GetEditUrl(String returnUrl, Achievement achievement, ControllerContext context)
        {
            UrlHelper u = new UrlHelper(context.RequestContext);
            return u.Action("AchievementCreateForm", "Achievement",
                new
                {
                    returnUrl = returnUrl,
                    achievementType = achievement.AchievementType,
                    achievementId = achievement.AchievementId
                });
        }

        /// <summary>
        /// Gets an achievement the given user has received but has not
        /// yet been informed of. It returns the oldest achievement.
        /// </summary>
        /// <param name="user">The user the achievement is searched for.</param>
        /// <returns>An AchievementItem which has not yet been shown to
        /// the User, or null if none exists.</returns>
        public Achievement GetNextUnshownAchievement(User user)
        {
            SGridDbContext dbContext = new SGridDbContext();
            return (from obtainedAchievement in dbContext.ObtainedAchievements.AsNoTracking().OrderBy(a => a.AchievementTimestamp)
                    where (obtainedAchievement.User.Id == user.Id)
                    && (!obtainedAchievement.AlreadyShown)
                    select obtainedAchievement.Achievement)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Gets all the users that have achieved a specific achievement.
        /// </summary>
        /// <param name="achievement">The achievement to get all users for.</param>
        /// <returns>An enumeration of the users which have this achievement.</returns>
        public IEnumerable<User> GetUsers(Achievement achievement)
        {
            SGridDbContext dbContext = new SGridDbContext();
            return from obtainedAchievement in dbContext.ObtainedAchievements.AsNoTracking()
                   where obtainedAchievement.Achievement.AchievementId == achievement.AchievementId
                   select obtainedAchievement.User;
        }

        /// <summary>
        /// Notifies that a user has been informed about having obtained
        /// an achievement.
        /// </summary>
        /// <param name="user">The user who has been informed about the
        /// achievement.</param>
        /// <param name="achievement">The achievement the user has been
        /// informed of.</param>
        public void SetAchievementShown(User user, Achievement achievement)
        {
            SGridDbContext dbContext = new SGridDbContext();
            ObtainedAchievement toSetShown = (from a in dbContext.ObtainedAchievements
                                      where (a.AchievementId == achievement.AchievementId)
                                      && (a.UserId == user.Id)
                                      select a).First();

            toSetShown.AlreadyShown = true;

            dbContext.SaveChanges();
        }
    }
}
