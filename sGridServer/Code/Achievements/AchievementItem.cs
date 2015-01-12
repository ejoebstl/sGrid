using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using System.Reflection;
using Resource = sGridServer.Resources.Achievements.AchievementRes;

namespace sGridServer.Code.Achievements
{
    /// <summary>
    /// This abstract class represents an Achievement class as
    /// specified in the data access layer. It provides the basic
    /// functionality of an achievement, but AchievementItems
    /// have additionnal functionnalities to use and test
    /// achievements.
    /// 
    /// While a new AchievementItem can be created dynamically, new
    /// types of achievements need to be implemented.
    /// </summary>
    public abstract class AchievementItem
    {
        /// <summary>
        /// The string representing the equality sign for the Url encoding.
        /// </summary>
        protected const string EqualitySign = "=";

        /// <summary>
        /// The string representing the concatenation sign for the Url encoding.
        /// </summary>
        protected const string AndSign = "&";

        /// <summary>
        /// This is the string identifying the property BonusCoins.
        /// </summary>
        protected const string BonusCoinsName = "BonusCoins";

        /// <summary>
        /// The achievement this AchievementItem is representing.
        /// </summary>
        public Achievement Achievement { get; private set; }

        /// <summary>
        /// Gets or sets the achievement id.
        /// </summary>
        public int AchievementId
        {
            get { return Achievement.AchievementId; }
            set { Achievement.AchievementId = value; }
        }

        /// <summary>
        /// Gets or sets the type of the achievement.
        /// </summary>
        public string AchievementType
        {
            get { return Achievement.AchievementType; }
            protected set { Achievement.AchievementType = value; }
        }

        /// <summary>
        /// Gets or sets if the achievement can be obtained.
        /// </summary>
        public bool Active
        {
            get { return Achievement.Active; }
            set { Achievement.Active = value; }
        }

        /// <summary>
        /// Gets or sets the amount of bonus coins to grant for the achievement.
        /// </summary>
        public int BonusCoins
        {
            get { return Achievement.BonusCoins; }
            set { Achievement.BonusCoins = value; }
        }

        /// <summary>
        /// Gets or sets the description of the achievement.
        /// </summary>
        public virtual MultiLanguageString Description
        {
            get { return Achievement.Description; }
            set { Achievement.Description = value; }
        }

        /// <summary>
        /// Gets or sets the parameters which are specific for this achievement type 
        /// and are needed to create a new achievement of this type as url encoded string.
        /// </summary>
        public String ExtendedParameters
        {
            get { return Achievement.ExtendedParameters; }
            protected set { Achievement.ExtendedParameters = value; }
        }

        /// <summary>
        /// Gets or sets the url of the icon of the achievement.
        /// </summary>
        public String Icon
        {
            get { return Achievement.Icon; }
            set { Achievement.Icon = value; }
        }

        /// <summary>
        /// Gets or sets the name of the achievement.
        /// </summary>
        public virtual MultiLanguageString Name
        {
            get { return Achievement.Name; }
            set { Achievement.Name = value; }
        }

        /// <summary>
        /// Gets or sets a list with elements of type ObtainedAchievement
        /// which shows the users who had reached this achievement.
        /// </summary>
        public virtual List<ObtainedAchievement> ObtainedAchievements
        {
            get { return Achievement.ObtainedAchievements; }
        }

        /// <summary>
        /// This event is raised whenever an achievement is obtained.
        /// </summary>
        public static event EventHandler<AchievementEventArgs> AchievementObtained;

        /// <summary>
        /// This enumeration stores the names of the Properties that can be
        /// changed by the administrator. It is meant to be initialited during
        /// the constructor.
        /// This enumeration is specific for every AchievementItem subclass.
        /// </summary>
        public IEnumerable<String> PropertyNames { get; protected set; }

        /// <summary>
        /// Initializes the value of the property ExtendedParameters
        /// using the parameters that are special to this specific
        /// AchievementItem after having tested if the properties
        /// have no invalid values.
        /// </summary>
        /// <returns>True if the tests were successful.</returns>
        protected abstract bool CreateExtendedParametersString(out string errorIdentifier);

        /// <summary>
        /// Initializes the value of the parameters that are special to
        /// this specific AchievementItem from the property
        /// ExtendedParameters.
        /// </summary>
        protected abstract void CreateParametersFromString();

        /// <summary>
        /// Initializes the AchievementItem.
        /// </summary>
        protected void BasicInitialization()
        {
            Achievement = new Achievement();
            AchievementId = -1;
            Icon = "";
            Active = true;
        }

        /// <summary>
        /// Performs the actions that are necessary in case a user
        /// achieves a certain achievement. This includes saving that
        /// the achievement has been granted and informing the
        /// CoinExchangeManager about this event.
        /// </summary>
        /// <param name="user">The user who obtained the achievement.</param>
        protected void GotAchieved(User user)
        {
            //add a new item to the ObtainedAchievements table
            SGridDbContext dbContext = new SGridDbContext();

            ObtainedAchievement obtAch = new ObtainedAchievement();
            obtAch.AchievementId = this.AchievementId;
            obtAch.UserId = user.Id;
            obtAch.AlreadyShown = false;
            obtAch.AchievementTimestamp = DateTime.Now;

            dbContext.ObtainedAchievements.Add(obtAch);
            dbContext.SaveChanges();

            //invoke the CoinExchange
            if (this.BonusCoins > 0)
            {
                CoinExchange.CoinExchange ce = new CoinExchange.CoinExchange(user);
                ce.Grant(Achievement, BonusCoins);
            }

            //raise event
            AchievementObtained(this, new AchievementEventArgs(Achievement, user));
        }

        /// <summary>
        /// Tests if the conditions for achieving an specific
        /// achievement are fulfilled.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ConditionsSatisfied(User user);

        /// <summary>
        /// Takes the necessary arrangements in order that the
        /// Achievement can be saved into the Database and tests if
        /// the values do not take illegal values.
        /// </summary>
        /// <returns>true if the tests were successful.</returns>
        public bool PrepareForSaving(out string errorIdentifier)
        {
            if (this.BonusCoins < 0)
            {
                errorIdentifier = String.Format(Resource.HasToBeNonnegative, Resource.BonucCoins);
                return false;
            }
            return CreateExtendedParametersString(out errorIdentifier);
        }

        /// <summary>
        /// Tests if the given user has already obtained a certain
        /// achievement.
        /// </summary>
        /// <param name="user">The user to be tested.</param>
        /// <returns>A bool indicating if the user has already
        /// obtained this achievement.</returns>
        public bool AlreadyAchieved(User user)
        {
            SGridDbContext dbContext = new SGridDbContext();
            return (from ObtainedAchievement obtAch in dbContext.ObtainedAchievements.AsNoTracking()
                    where (obtAch.AchievementId == this.AchievementId)
                    && (obtAch.UserId == user.Id)
                    select obtAch).Any();
        }

        /// <summary>
        /// This method will first test if the user has already got the
        /// achievement, then it will test if the user now has fulfilled
        /// the conditions to obtain the achievement.
        /// If this is the case, then the GotAchieved method will be called.
        /// </summary>
        /// <param name="user">The user to be tested.</param>
        public void Test(User user)
        {
            if (Active && !AlreadyAchieved(user) && ConditionsSatisfied(user))
            {
                GotAchieved(user);
            }
        }

        /// <summary>
        /// Converts the given Achievement into an AchievementItem which
        /// has the same Properties.
        /// </summary>
        /// <param name="achievement">The Achievement the AchievementItem
        /// is made from.</param>
        /// <returns>The AchievementItem made from the Achievement</returns>
        public static AchievementItem ToAchievementItem(Achievement achievement)
        {
            AchievementManager manager = new AchievementManager();
            AchievementItem item = manager.GetAchievementFromType(achievement.AchievementType);

            item.Achievement = achievement;

            item.CreateParametersFromString();
            return item;
        }
    }
}