using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace sGridServer.Code.Rewards
{
    /// <summary>
    /// This class provides various methods to manage and get
    /// information about rewards. 
    /// </summary>
    public class RewardManager
    {
        /// <summary>
        /// Maps the description of the reward types to the Rewards
        /// themselves. It is initialized during the static constructor
        /// and then it remains unchanged. The contained Reward objects
        /// are meant to be cloned, in order to get a new instance of a
        /// certain reward type. 
        /// </summary>
        private static ConcurrentDictionary<String, Type> rewardTypes;

        /// <summary>
        /// static constructor
        /// </summary>
        static RewardManager()
        {
            rewardTypes = new ConcurrentDictionary<string, Type>();
            InitRewardTypes();
        }

        /// <summary>
        /// Initializes the Dictionary RewardTypes. This method is
        /// executed by the static constructor.
        /// </summary>
        private static void InitRewardTypes()
        {
            rewardTypes[new DummyReward().RewardType] = typeof(DummyReward);
        }

        /// <summary>
        /// Sets a reward to approved. This method should only be
        /// invoked my methods  controlled by admins.
        /// </summary>
        public void ApproveReward(Reward reward)
        {
            SGridDbContext dbContext = new SGridDbContext();

            //get reward
            Reward rew = (from r in dbContext.Rewards
                          where r.Id == reward.Id
                          select r).Single();

            //perform changes
            rew.Approved = true;

            //save changes
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Locks the reward, tests, if the reward if buyable and adjusts
        /// the amount of available rewards. If the reward is not buyable
        /// in this amount an exception is trown.
        /// </summary>
        /// <param name="user">The user, who wanted to buy the rewards.
        /// His coin account is supposed to be locked.</param>
        /// <param name="reward">The reward that has to be bought.</param>
        /// <param name="amount">The amount of rewards to be bought.</param>
        /// <returns>The purchase object describing the buying.</returns>
        public Purchase BuyReward(User user, Reward reward, int amount)
        {
            if (System.Transactions.Transaction.Current == null)
            {
                throw new InvalidOperationException("A reward cannot be bought without a transaction.");
            }

            SGridDbContext dbContext = new SGridDbContext();

            //lock row
            reward = dbContext.Rewards.SqlQuery("SELECT TOP 1 * FROM Rewards WITH (UPDLOCK) WHERE id = @id",
                new SqlParameter("id", reward.Id)).Single();

            //test is this amount can be purchased
            if (!IsActive(reward))
            {
                throw new ArgumentException("This reward can not be bought currently.");
            }
            if (amount > reward.Amount)
            {
                throw new ArgumentException("There aren't enough instances of this reward that can be bought.");
            }

            //adjust amount
            reward.Amount -= amount;

            //create purchases and store them into the database
            Purchase purchase = new Purchase();
            purchase.Amount = amount;
            purchase.RewardId = reward.Id;
            purchase.Timestamp = DateTime.Now;
            purchase.UserId = user.Id;

            dbContext.Purchases.Add(purchase);

            //release lock and return the purchases
            dbContext.SaveChanges();

            return purchase;
        }
        
        /// <summary>
        /// Tests if the given reward is currently in sale and
        /// therefore cannot be changed.
        /// </summary>
        /// <param name="reward">The reward to be tested.</param>
        /// <returns>True, if the reward is approved and in the 
        /// active interval.</returns>
        public bool IsActive(Reward reward)
        {
            return ((reward.Begin <= DateTime.Now) && (DateTime.Now < reward.End) && reward.Approved);
        }

        /// <summary>
        /// Creates a new reward an saves it.
        /// Note: This method is meant to be called by the Submit
        /// method in the RewardController.
        /// </summary>
        /// <param name="rewItem">The reward to be saved.</param>
        public void CreateReward(RewardItem rewItem)
        {
            SGridDbContext dbContext = new SGridDbContext();
            Reward reward = rewItem.Reward;

            dbContext.Rewards.Add(reward);
            reward.Name.Reattach(dbContext);
            reward.Description.Reattach(dbContext);
            reward.ShortDescription.Reattach(dbContext);

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Changes an existing reward, if it can be changed.
        /// Note: This method is meant to be called by the Submit
        /// method in the RewardController.
        /// </summary>
        /// <param name="rewItem">The reward to be changed.</param>
        public void EditReward(RewardItem rewItem)
        {
            SGridDbContext dbContext = new SGridDbContext();
            Reward reward = rewItem.Reward;

            //get reward
            Reward rew = (from r in dbContext.Rewards
                          where r.Id == reward.Id
                          select r).Single();

            //perform changes
            dbContext.Entry(rew).CurrentValues.SetValues(reward);
            reward.Name.Reattach(dbContext);
            reward.Description.Reattach(dbContext);
            reward.ShortDescription.Reattach(dbContext);

            //save changes
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Creates and returns an enumeration of all rewards.
        /// </summary>
        /// <returns>An enumeration containing all rewards.</returns>
        public IEnumerable<Reward> GetAllExistingRewards()
        {
            SGridDbContext dbContext = new SGridDbContext();
            return dbContext.Rewards.AsNoTracking();
        }

        /// <summary>
        /// Creates and returns an enumeration of the different existing
        /// types of rewards.
        /// </summary>
        /// <returns>An enumeration of the descriptions of all existing
        /// types of reward items.</returns>
        public IEnumerable<String> GetAllRewardTypes()
        {
            return rewardTypes.Keys;
        }

        /// <summary>
        /// Gets the url pointing at the RewardCreateForm method of the
        /// RewardController. This method is used to create a Reward.
        /// </summary>
        /// <param name="returnUrl">The url to return to after the
        /// reward creation finishes.</param>
        /// <param name="rewardType">The description of the type the
        /// reward to create will be.</param>
        /// <param name="context">The context of the controller invoking
        /// this method.</param>
        /// <returns>The url pointing at the RewardCreateForm method of
        /// the RewardController.</returns>
        public String GetCreateUrl(String returnUrl, String rewardType, ControllerContext context)
        {
            UrlHelper u = new UrlHelper(context.RequestContext);
            return u.Action("RewardCreateForm", "Reward",
                new { returnUrl = returnUrl,
                    rewardType = rewardType,
                    rewardId = -1 });
        }

        /// <summary>
        /// Gets the url pointing at the RewardCreateForm method of the
        /// RewardController. This method is used to edit a Reward.
        /// </summary>
        /// <param name="returnUrl">The url to return to after the
        /// reward editing finishes.</param>
        /// <param name="reward">The Reward which has to be edited.</param>
        /// <param name="context">The context of the controller invoking
        /// this method.</param>
        /// <returns>The url pointing at the RewardCreateForm method of
        /// the RewardController.</returns>
        public String GetEditUrl(String returnUrl, Reward reward, ControllerContext context)
        {
            UrlHelper u = new UrlHelper(context.RequestContext);
            return u.Action("RewardCreateForm", "Reward",
                new
                {
                    returnUrl = returnUrl,
                    rewardType = reward.RewardType,
                    rewardId = reward.Id
                });
        }

        /// <summary>
        /// Gets the purchases which describe which user bought which
        /// reward in which quantity at which time.
        /// </summary>
        /// <returns>An enumeration of Purchase objects. </returns>
        public IEnumerable<Purchase> GetPurchases()
        {
            SGridDbContext dbContext = new SGridDbContext();
            return dbContext.Purchases
                .AsNoTracking();
        }

        /// <summary>
        /// Gets a Reward with a given id.
        /// </summary>
        /// <param name="id">The id of the reward.</param>
        /// <returns>The requested reward, or null, if for this id no
        /// reward exists.</returns>
        public Reward GetRewardById(int id)
        {
            SGridDbContext dbContext = new SGridDbContext();
            IEnumerable<Reward> rewis = (from Reward in dbContext.Rewards.AsNoTracking()
                                             where Reward.Id == id
                                             select Reward);
            if (rewis.Any())
            {
                return rewis.Single();
            }
            return null;
        }

        /// <summary>
        /// Returns a RewardItem from a given type.
        /// </summary>
        /// <param name="rewardType">The type a for which a RewardItem
        /// should be created. </param>
        /// <returns>A RewardItem of the given type or null, if the type
        /// is not known.</returns>
        public RewardItem GetRewardFromType(String rewardType)
        {
            if (rewardTypes.ContainsKey(rewardType))
            {
                RewardItem item = (RewardItem)Activator.CreateInstance(rewardTypes[rewardType]);
                item.Description = new MultiLanguageString();
                item.Name = new MultiLanguageString();
                item.ShortDescription = new MultiLanguageString();

                return item;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and returns an enumeration of the rewards a given
        /// user has bought.
        /// </summary>
        /// <param name="user">The user to get the rewards for. </param>
        /// <returns>An enumeration of the Reward objects for the user.</returns>
        public IEnumerable<Reward> GetRewards(User user)
        {
            SGridDbContext dbContext = new SGridDbContext();
            return (from purchase in dbContext.Purchases.AsNoTracking()
                    where purchase.UserId == user.Id
                    select purchase.Reward);
        }

        /// <summary>
        /// Sends in a rating for a given reward.
        /// If the user has already rated this reward, his new
        /// decision will override his old one.
        /// </summary>
        /// <param name="reward">The reward to be rated.</param>
        /// <param name="user">The user, who rated the reward.</param>
        /// <param name="ratedWith">The value describing the rating
        /// of the reward.</param>
        public void RateReward(Reward reward, User user, int ratedWith)
        {
            SGridDbContext dbContext = new SGridDbContext();
            Rating rating = (from r in dbContext.Ratings
                             where (r.RewardId == reward.Id) && (r.UserId == user.Id)
                             select r).SingleOrDefault();

            if (rating == null)
            {
                //add a new rating
                rating = new Rating();

                rating.RatedWith = ratedWith;
                rating.RewardId = reward.Id;
                rating.Timestamp = DateTime.Now;
                rating.UserId = user.Id;

                dbContext.Ratings.Add(rating);
            }
            else
            {
                //simply change the rating
                rating.RatedWith = ratedWith;
                rating.Timestamp = DateTime.Now;
            }
            dbContext.SaveChanges();
        }

        /// <summary>
        /// After a reward has been purchased this method is
        /// executed in order that the user receives his reward.
        /// </summary>
        /// <param name="purchase">A purchase, which is obtained
        /// through calling the BuyReward method.</param>
        /// <returns>An url.</returns>
        public string ReceiveReward(Purchase purchase)
        {
            RewardItem rewardItem = RewardItem.ToRewardItem(purchase.Reward);
            return rewardItem.ObtainReward(purchase);
        }
    }
}
