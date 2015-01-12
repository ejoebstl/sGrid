using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.Rewards.RewardRes;
using sGridServer.Code.Security;

namespace sGridServer.Code.Rewards
{
    /// <summary>
    /// This abstract class represents a reward as specified in the
    /// data access layer. It provides the basic functionality of a
    /// reward, but RewardItems have additionnal functionnalities
    /// to. A reward that has been bought by a user can be handed
    /// out to this user using this class. The class deriving from
    /// the RewardItem class has to describe how this reward can be
    /// handed out to the user.
    /// 
    /// While a new RewardItem can be created dynamically new types
    /// of rewards need to be implemented.
    /// </summary>
    public abstract class RewardItem
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
        /// This is the string identifying the property Amount.
        /// </summary>
        protected const string AmountName = "Amount";

        /// <summary>
        /// This is the string identifying the property Begin.
        /// </summary>
        protected const string BeginName = "Begin";

        /// <summary>
        /// This is the string identifying the property Cost.
        /// </summary>
        protected const string CostName = "Cost";

        /// <summary>
        /// This is the string identifying the property End.
        /// </summary>
        protected const string EndName = "End";

        /// <summary>
        /// This is the string identifying the property Url.
        /// </summary>
        protected const string URLName = "URL";

        /// <summary>
        /// The reward this RewardItem is representing.
        /// </summary>
        public Reward Reward { get; private set; }

        /// <summary>
        /// Gets or sets the id of the reward.
        /// </summary>
        public int Id
        {
            get { return Reward.Id; }
            set { Reward.Id = value; }
        }

        /// <summary>
        /// Gets or sets an integer indicating the number of available items of this reward. 
        /// </summary>
        public int Amount
        {
            get { return Reward.Amount; }
            set { Reward.Amount = value; }
        }

        /// <summary>
        /// Gets or sets a bool indicating whether a reward was approved by sGrid team.
        /// </summary>
        public bool Approved
        {
            get { return Reward.Approved; }
            set { Reward.Approved = value; }
        }

        /// <summary>
        /// Gets or sets a timestamp which specifies from when the reward should be available.
        /// </summary>
        public DateTime Begin
        {
            get { return Reward.Begin; }
            set { Reward.Begin = value; }
        }

        /// <summary>
        /// Gets or sets the costs of the reward in coins.
        /// </summary>
        public int Cost
        {
            get { return Reward.Cost; }
            set { Reward.Cost = value; }
        }

        /// <summary>
        /// Gets or sets the description of the reward.
        /// </summary>
        public virtual MultiLanguageString Description
        {
            get { return Reward.Description; }
            set { Reward.Description = value; }
        }

        /// <summary>
        /// Gets or sets a timestamp which specifies until when the reward should be available.
        /// </summary>
        public DateTime End
        {
            get { return Reward.End; }
            set { Reward.End = value; }
        }

        /// <summary>
        /// Gets or sets the parameters which are specific for this reward type 
        /// and are needed to create a new reward of this type as url encoded string.
        /// </summary>
        public String ExtendedParameters
        {
            get { return Reward.ExtendedParameters; }
            set { Reward.ExtendedParameters = value; }
        }

        /// <summary>
        /// Gets or sets the name of the reward.
        /// </summary>
        public virtual MultiLanguageString Name
        {
            get { return Reward.Name; }
            set { Reward.Name = value; }
        }

        /// <summary>
        /// Gets or sets the url of the picture of the reward.
        /// </summary>
        public String Picture
        {
            get { return Reward.Picture; }
            set { Reward.Picture = value; }
        }


        /// <summary>
        /// Gets or sets the type of the reward.
        /// </summary>
        public String RewardType
        {
            get { return Reward.RewardType; }
            set { Reward.RewardType = value; }
        }

        /// <summary>
        /// Gets or sets the short description of the reward.
        /// </summary>
        public virtual MultiLanguageString ShortDescription
        {
            get { return Reward.ShortDescription; }
            set { Reward.ShortDescription = value; }
        }

        /// <summary>
        /// Gets or sets the url of the reward.
        /// </summary>
        public String URL
        {
            get { return Reward.URL; }
            set { Reward.URL = value; }
        }


        /// <summary>
        /// Gets or sets the id of the coin partner which provides the reward.
        /// </summary>
        public int CoinPartnerId
        {
            get { return Reward.CoinPartnerId; }
            set { Reward.CoinPartnerId = value; }
        }

        /// <summary>
        /// Gets or sets the coin partner which provides the reward.
        /// </summary>
        public virtual CoinPartner CoinPartner
        {
            get { return Reward.CoinPartner; }
            set { Reward.CoinPartner = value; }
        }

        /// <summary>
        /// Gets or sets the list of purchases where the reward was bought.
        /// </summary>
        public virtual List<Purchase> Purchases
        {
            get { return Reward.Purchases; }
        }

        /// <summary>
        /// Gets or sets the list of rating where the reward was rated by users.
        /// </summary>
        public virtual List<Rating> Ratings
        {
            get { return Reward.Ratings; }
        }

        /// <summary>
        /// This enumeration stores the names of the Properties that can be
        /// changed by the CoinPartner. It is meant to be initialited during
        /// the constructor.
        /// This enumeration is specific for every RewardItem subclass.
        /// </summary>
        public IEnumerable<String> PropertyNames { get; protected set; }

        /// <summary>
        /// Initializes the RewardItem.
        /// </summary>
        protected void BasicInitialization()
        {
            Reward = new Reward();
            Id = -1;
            Approved = false;
            Begin = DateTime.Now;
            End = DateTime.Now;
            Picture = "";
            URL = "";
        }

        /// <summary>
        /// Initializes the value of the property ExtendedParameters
        /// using the parameters that are special to this specific
        /// RewardItem.
        /// </summary>
        protected abstract bool CreateExtendedParametersString(out string errorMessage);

        /// <summary>
        /// Initializes the value of the parameters that are special to
        /// this specific RewardItem from the property ExtendedParameters.
        /// </summary>
        protected abstract void CreateParametersFromString();

        /// <summary>
        /// Hands out the reward represented by this RewardItem to the
        /// User. This method should describe in which ways the reward
        /// is obtained and delivered.
        /// </summary>
        /// <param name="purchase">The purchase describing who has bought
        /// this reward and when.</param>
        /// <returns>An url.</returns>
        public abstract string ObtainReward(Purchase purchase);

        /// <summary>
        /// Takes the necessary arrangements in order that the
        /// RewardItem can be saved into the Database.
        /// </summary>
        public bool PrepareForSaving(out string errorIdentifier)
        {
            this.Approved = false;

            //tests if the propertys have valid values
            //if not the errorIdentifyer is set
            if (this.Amount < 1)
            {
                errorIdentifier = String.Format(Resource.HasToBePositive, Resource.Amount);
                return false;
            }
            if (this.Cost < 1)
            {
                errorIdentifier = String.Format(Resource.HasToBePositive, Resource.Cost);
                return false;
            }
            if (this.End < this.Begin)
            {
                errorIdentifier = Resource.EndTooEarly;
                return false;
            }
            //this tests if the coin partner exists
            MemberManager memMan = new MemberManager();
            if (!(from cp in memMan.CoinPartners where cp.Id == this.CoinPartnerId select cp.Id).Any())
            {
                errorIdentifier = Resource.WrongCoinPartner;
                return false;
            }
            if (!Uri.IsWellFormedUriString(URL, UriKind.RelativeOrAbsolute))
            {
                errorIdentifier = Resource.IncorrectUrl;
                return false;
            }

            //no error occured
            return CreateExtendedParametersString(out errorIdentifier);
        }

        /// <summary>
        /// Converts the given Achievement into an AchievementItem which
        /// has the same Properties.
        /// </summary>
        /// <param name="reward">The Achievement the AchievementItem
        /// is made from.</param>
        /// <returns>The AchievementItem made from the Achievement</returns>
        public static RewardItem ToRewardItem(Reward reward)
        {
            RewardManager manager = new RewardManager();
            RewardItem item = manager.GetRewardFromType(reward.RewardType);

            item.Reward = reward;

            item.CreateParametersFromString();
            return item;
        }
    }
}
