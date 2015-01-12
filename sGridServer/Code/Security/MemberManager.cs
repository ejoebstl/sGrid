using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.DataAccessLayer;
using System.Text;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace sGridServer.Code.Security
{
    /// <summary>
    /// The member manager is the main class of the member management module
    /// It provides member management functions. 
    /// </summary>
    public class MemberManager
    {
        /// <summary>
        /// Database context.
        /// </summary>
        private SGridDbContext dbContext;

        /// <summary>
        /// Gets all accounts. 
        /// </summary>
        public IEnumerable<Account> Accounts
        {
            get { return dbContext.Accounts.AsNoTracking(); }
        }

        /// <summary>
        /// Gets all coin partners. 
        /// </summary>
        public IEnumerable<CoinPartner> CoinPartners
        {
            get
            {
                return Accounts
                    .Where(a => a.UserPermission == (SiteRoles.CoinPartner))
                    .Cast<CoinPartner>();
            }
        }

        /// <summary>
        /// Gets all partners.
        /// </summary>
        public IEnumerable<Partner> Partners
        {
            get
            {
                return Accounts
                    .Where(a => a.UserPermission == (SiteRoles.CoinPartner)
                             || a.UserPermission == (SiteRoles.Sponsor))
                    .Cast<Partner>();
            }
        }

        /// <summary>
        /// Gets all sGrid team members. 
        /// </summary>
        public IEnumerable<SGridTeamMember> SGridMembers
        {
            get
            {
                return Accounts
                    .Where(a => a.UserPermission == (SiteRoles.Admin))
                    .Cast<SGridTeamMember>();
            }
        }

        /// <summary>
        /// Gets all sponsors.
        /// </summary>
        public IEnumerable<Sponsor> Sponsors
        {
            get
            {
                return Accounts
                    .Where(a => a.UserPermission == SiteRoles.Sponsor)
                    .Cast<Sponsor>();
            }
        }

        /// <summary>
        /// Gets all users. 
        /// </summary>
        public IEnumerable<User> Users
        {
            get
            {
                return Accounts
                    .Where(a => a.UserPermission == SiteRoles.User)
                    .Cast<User>();
            }
        }

        /// <summary>
        /// This event is raised whenever a new friendship is added. 
        /// </summary>
        public static event EventHandler<FriendshipAddedEventArgs> FriendshipAdded;

        /// <summary>
        /// This event is raised whenever a new user is created.
        /// </summary>
        public static event EventHandler<UserCreadedEventArgs> UserCreated;

        /// <summary>
        /// Creates a new instance of this class and initializes the DB context.
        /// </summary>
        public MemberManager()
        {
            dbContext = new SGridDbContext();
        }

        /// <summary>
        /// Marks the given account as active. 
        /// </summary>
        /// <param name="account">The account to activate.</param>
        public void ActivateAccount(Account account)
        {
            Account toActivate = (from a in dbContext.Accounts
                                    where a.Id == account.Id
                                    select a).First();

            toActivate.Active = true;
            account.Active = true;

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Adds a friend request from user sender to user receiver, 
        /// if an equal friend request already exists, an ArgumentException 
        /// is thrown. 
        /// </summary>
        /// <param name="sender">The user who requested the friendship.</param>
        /// <param name="receiver">The user who receives the friend request. </param>
        public void AddFriendRequest(User sender, User receiver)
        {
            if ((from r in dbContext.FriendRequests
                 where r.Receiver.Id == receiver.Id && r.Requester.Id == sender.Id
                 select r).Any())
            {
                throw new ArgumentException("A friend request from the given sender to the given receiver does already exist.");
            }


            dbContext.FriendRequests.Add(new FriendRequest()
            {
                ReceiverId = receiver.Id,
                RequesterId = sender.Id,
                Rejected = false,
                Timestamp = DateTime.Now
            });

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Checks whether the two given users are befriended. 
        /// </summary>
        /// <param name="a">The first user.</param>
        /// <param name="b">The second user.</param>
        /// <returns>A bool indicating whether the two given users are befriended.</returns>
        public bool AreFriends(User a, User b)
        {
            return (from friendship in dbContext.Friendships
                    where (friendship.User.Id == a.Id && friendship.Friend.Id == b.Id)
                    || (friendship.User.Id == b.Id && friendship.Friend.Id == a.Id)
                    select friendship).Any();
                
        }

        /// <summary>
        /// Creates the given user. 
        /// If the id of the given user is any other than -1, 
        /// an InvalidOperationException is thrown. In this case, 
        /// the SaveAccount method should be used. This method also 
        /// invokes the UserCreated event by calling the 
        /// InvokeUserCreated method. 
        /// </summary>
        /// <remarks>
        /// The given User object has to meet the following conditions: 
        /// * The Id has to be equal -1
        /// * The Nickname and the EMail properties have to be set
        /// * The Nickname and the EMail properties have to unique
        /// * The UserPermission property has to equal SiteRoles.User
        /// 
        /// This method automatically creates a CoinAccount and an AuthenticationToken for the given
        /// User object, if these are not set. 
        /// </remarks>
        /// <param name="user">The user to create.</param>
        /// <returns>The created user, including the ID of the user, which is 
        /// generated by the database.</returns>
        public User CreateUser(User user)
        {
            //Perform state checks. 
            if (user.Id != -1)
            {
                throw new ArgumentException("The user which should be created already exists");
            }
            if (user.Nickname == "" || user.Nickname == null)
            {
                throw new ArgumentException("Cannot create a user without a nickname.");
            }
            if (user.EMail == "" || user.EMail == null)
            {
                throw new ArgumentException("Cannot create a user without an e-mail address.");
            }
            if (user.UserPermission != SiteRoles.User)
            {
                throw new ArgumentException("The user which should be created has UserPermissions set to " + Enum.GetName(typeof(SiteRoles), user.UserPermission));
            }
            if (Accounts.Where(x => x.Nickname == user.Nickname).Any())
            {
                throw new ArgumentException("The user which should be created has a nickname which is already in use by another user");
            }
            if (Accounts.Where(x => x.EMail == user.EMail).Any())
            {
                throw new ArgumentException("The user which should be created has an e-mail address which is already in use by another user");
            }

            //User has no coin account - create one. 
            if (user.CoinAccount == null)
            {
                user.CoinAccount = new CoinAccount();
            }

            //User has no authentication token - generate one. 
            if (user.AuthenticationToken == "" || user.AuthenticationToken == null)
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                byte[] data = new byte[32];

                rng.GetBytes(data);

                rng.Dispose();

                string token = Convert.ToBase64String(data) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                token = token.Replace("+", "").Replace(" ", "").Replace("/", "").Replace("=", "");

                user.AuthenticationToken = token;
            }

            //Save the changes
            dbContext.Accounts.Add(user);
            dbContext.SaveChanges();

            //Invoke UserCreated event
            InvokeUserCreated(user);

            //Return the newly created User object. 
            return GetAccountById(user.Id) as User;
        }

        /// <summary>
        /// Marks the given account as inactive and private.
        /// </summary>
        /// <param name="account">The account to inactivate.</param>
        public void DeactivateAccount(Account account)
        {
            Account toInactivate = (from a in dbContext.Accounts
                                   where a.Id == account.Id
                                   select a).First();

            toInactivate.Privacy = PrivacyLevel.Private;
            toInactivate.Active = false;
            account.Active = false;
            account.Privacy = PrivacyLevel.Private;

            dbContext.SaveChanges();
        }

        /// <summary>
        /// Changes the given user to a coin partner by dropping all user 
        /// specific information in the database and changing the role to 
        /// CoinPartner. If the user has any transactions or projects associated 
        /// with him, this function fails with an InvalidOperationException, 
        /// since transactions must be kept for data integrity. In this case, 
        /// the user has to create a new account which is then elevated to a 
        /// coin partner. 
        /// </summary>
        /// <param name="user">The user to elevate.</param>
        /// <returns>The coin partner created from the user.</returns>
        public CoinPartner ElevateToCoinPartner(User user)
        {
            CoinPartner coinPartner = new CoinPartner();

            dbContext.Accounts.Add(coinPartner);
            dbContext.Entry<Account>(coinPartner).CurrentValues.SetValues((Account)user);
            coinPartner.Description = "";
            coinPartner.Description.Reattach(dbContext);
           
            Remove(user);

            dbContext.SaveChanges();

            return coinPartner;
        }

        /// <summary>
        /// Changes the given user to a sponsor by dropping all user 
        /// specific information in the database and changing the 
        /// role to Sponsor. If the user has any transactions or projects 
        /// associated with him, this function fails with an 
        /// InvaildOperationException, since transactions must 
        /// be kept for data integrity. In this case, the user 
        /// has to create a new account which is then elevated to a sponsor. 
        /// </summary>
        /// <param name="user">The user to elevate.</param>
        /// <returns>The sponsor created from the user.</returns>
        public Sponsor ElevateToSponsor(User user)
        {
            Sponsor sponsor = new Sponsor();

            dbContext.Accounts.Add(sponsor);
            dbContext.Entry<Account>(sponsor).CurrentValues.SetValues((Account)user);
            sponsor.Description = "";
            sponsor.Description.Reattach(dbContext);
            
            Remove(user);

            dbContext.SaveChanges();

            return sponsor;
        }

        /// <summary>
        /// Changes the given user to a sGrid team member by dropping all 
        /// user specific information in the database and changing the
        /// role to Admin. If the user has any transactions or projects
        /// associated with him, this function fails with an 
        /// InvalidOperationException, since transactions must be kept 
        /// for data integrity. In this case, the user has to create a 
        /// new account which is then elevated to a sGrid team member. 
        /// </summary>
        /// <param name="user">The user to elevate.</param>
        /// <returns>The sGrid team member created from the user.</returns>
        public SGridTeamMember ElevateToTeamMember(User user)
        {
            SGridTeamMember teamMember = new SGridTeamMember();

            dbContext.Accounts.Add(teamMember);
            dbContext.Entry<Account>(teamMember).CurrentValues.SetValues((Account)user);

            Remove(user);

            dbContext.SaveChanges();

            return teamMember;
        }

        /// <summary>
        /// Removes the given account, if it exists.
        /// If the account does not exist, a ArgumentException is thrown. 
        /// </summary>
        /// <param name="toRemove">The account to remove.</param>
        private void Remove(Account toRemove)
        {
            Account account = (from a in dbContext.Accounts
                               where a.Id == toRemove.Id
                               select a).FirstOrDefault();

            if (account == null)
            {
                throw new ArgumentException("The given account does not exist in the database");
            }

            dbContext.Accounts.Remove(account);
        }

        /// <summary>
        /// Gets an account by its id.
        /// </summary>
        /// <param name="id">The id to get the account for.</param>
        /// <returns>The account for the given id.</returns>
        public Account GetAccountById(int id)
        {
            return (from user in Accounts
                    where user.Id == id
                    select user).FirstOrDefault();
        }

        /// <summary>
        /// Gets an account by its AccountToken property.
        /// </summary>
        /// <param name="token">The token to get the account for.</param>
        /// <returns>The account for the given token.</returns>
        public Account GetAccountByToken(string token)
        {
            return (from account in Accounts
                    where account.AccountToken == token
                    select account).FirstOrDefault();
        }

        /// <summary>
        /// Gets the friend request from sender to receiver, if any. 
        /// </summary>
        /// <param name="sender">The sender of the friend request to find.</param>
        /// <param name="receiver">The receiver of the friend request to find.</param>
        /// <returns>The friend request between the two users, or null, if no friend request was found. </returns>
        public FriendRequest GetFriendRequest(User sender, User receiver)
        {
            return (from request in dbContext.FriendRequests.AsNoTracking()
                    where request.RequesterId == sender.Id &&
                    request.ReceiverId == receiver.Id
                    select request).FirstOrDefault();
        }

        /// <summary>
        /// Gets the friend request from sender to receiver, if any. 
        /// </summary>
        /// <param name="receiver">The user to search the requests for.</param>
        /// <returns>All friend requests which were sent to the given user. </returns>
        public IEnumerable<FriendRequest> GetFriendRequests(User receiver)
        {
            return from request in dbContext.FriendRequests.AsNoTracking()
                   where request.ReceiverId == receiver.Id
                   select request;
        }

        /// <summary>
        /// Gets all friends of the given user. 
        /// </summary>
        /// <param name="user">The user to get all friends for.</param>
        /// <returns>An enumeration of all friends of the given user.</returns>
        public IEnumerable<User> GetFriends(User user)
        {
            IEnumerable<User> first = from friendship in dbContext.Friendships.AsNoTracking()
                   where friendship.User.Id == user.Id
                   select friendship.Friend;

            IEnumerable<User> second = from friendship in dbContext.Friendships.AsNoTracking()
                                      where friendship.Friend.Id == user.Id
                                      select friendship.User;

            return first.Union(second);
        }

        /// <summary>
        /// Gets all friends which were invited by the given user. 
        /// </summary>
        /// <param name="user">The user to get all friends which were invited for.</param>
        /// <returns>An enumeration of all friends which were invited by the given user.</returns>
        public IEnumerable<User> GetInvitedFriends(User user)
        {
            return from friendship in dbContext.Friendships.AsNoTracking()
                   where friendship.User.Id == user.Id
                   select friendship.User;
        }

        /// <summary>
        /// Generates a url which can be sent to individuals to invite them. 
        /// This function includes an identifier of the inviter into the url, 
        /// so the inviter can be identified when the new user decides to join 
        /// sGrid. The invitation url points directly to the registration page. 
        /// </summary>
        /// <param name="inviter">The user who initiated the invitation.</param>
        /// <param name="context">The controller context to use for url generation.</param>
        /// <returns>The generated invitation url. </returns>
        public string GetInvitationUrl(User inviter, System.Web.Mvc.ControllerContext context)
        {
            return new UrlHelper(context.RequestContext).Action("InvitationLanding", "IdProviderHelper",
                new { inviterId = inviter.Id }, context.HttpContext.Request.Url.Scheme);
        }

        /// <summary>
        /// Registers a friendship between user a and user b. 
        /// The friendship relation is bidirectional. 
        /// If there are any pending friend requests between a and b, 
        /// the friend requests are removed. This function also 
        /// invokes the FriendshipAdded event by calling the 
        /// InvokeFriendshipAdded method. 
        /// </summary>
        /// <param name="a">The first user of the friendship relation.</param>
        /// <param name="b">The second user of the friendship relation.</param>
        /// <param name="wasInvited">A bool specifying whether user b was invited by user a.</param>
        public void RegisterFriendship(User a, User b, bool wasInvited = false)
        {
            //Check whether a and b are already befriended. 
            if (AreFriends(a, b))
            {
                throw new InvalidOperationException("The given users are already friends");
            }

            //Remove existing friend requests between a and b.
            RemoveFriendRequest(a, b);
            RemoveFriendRequest(b, a);

            //Add the friendship.
            dbContext.Friendships.Add(new Friendship()
            {
                FriendId = b.Id,
                UserId = a.Id,
                WasInvited = wasInvited
            });

            //Save changes.
            dbContext.SaveChanges();

            //Invoke the FriendshipAdded event.
            InvokeFriendshipAdded(a, b, wasInvited);
        }

        /// <summary>
        /// Removes the friend request from a to b, if any. 
        /// </summary>
        /// <param name="a">The sender of the request.</param>
        /// <param name="b">The receiver of the request.</param>
        private void RemoveFriendRequest(User a, User b)
        {
            FriendRequest request = (from r in dbContext.FriendRequests
                          where r.Requester.Id == a.Id &&
                          r.Receiver.Id == b.Id
                          select r).FirstOrDefault();

            if (request != null)
            {
                dbContext.FriendRequests.Remove(request);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Marks the pending friend request from sender to receiver as rejected. 
        /// If the friend request does not exist, an ArgumentException is thrown. 
        /// </summary>
        /// <param name="sender">The sender of the friend request to find.</param>
        /// <param name="receiver">The receiver of the friend request to find.</param>
        public void RejectFriendRequest(User sender, User receiver)
        {
            FriendRequest request = (from r in dbContext.FriendRequests
                                     where r.RequesterId == sender.Id &&
                                           r.ReceiverId == receiver.Id
                                     select r).FirstOrDefault();
            if (request != null)
            {
                request.Rejected = true;
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Removes the friendship between user a and user b. 
        /// </summary>
        /// <param name="a">The first user of the friendship relation.</param>
        /// <param name="b">The second user of the friendship relation.</param>
        public void RemoveFriendship(User a, User b)
        {
            Friendship friendshipToDelete = (from friendship in dbContext.Friendships
                                             where (friendship.User.Id == a.Id && friendship.Friend.Id == b.Id)
                                             || (friendship.User.Id == b.Id && friendship.Friend.Id == a.Id)
                                             select friendship).FirstOrDefault();

            if (friendshipToDelete != null)
            {
                dbContext.Friendships.Remove(friendshipToDelete);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Saves a Partner.
        /// </summary>
        /// <param name="partner">The Partner to save.</param>
        public void SavePartner(Partner partner)
        {
            partner.Description.Reattach(dbContext);

            SaveAccount(partner);
        }

        /// <summary>
        /// Saves a Sponsor.
        /// </summary>
        /// <param name="sponsor">The Sponsor to save.</param>
        public void SaveSponsor(Sponsor sponsor)
        {
            SavePartner(sponsor);
        }

        /// <summary>
        /// Saves a CoinPartner.
        /// </summary>
        /// <param name="partner">The CoinPartner to save.</param>
        public void SaveCoinPartner(CoinPartner partner)
        {
            SavePartner(partner);
        }

        /// <summary>
        /// Saves a team member.
        /// </summary>
        /// <param name="teamMember">The team member to save.</param>
        public void SaveTeamMember(SGridTeamMember teamMember)
        {
            SaveAccount(teamMember);
        }

        /// <summary>
        /// Saves all pending changes of the account object to the database. 
        /// </summary>
        /// <remarks>
        /// The given Account object has to meet the following conditions: 
        /// * The Nickname and the EMail properties have to be set
        /// * The Nickname and the EMail properties have to unique
        /// </remarks>
        /// <param name="account">The account to save changes for.</param>
        public void SaveAccount(Account account)
        {
            //Check whether Nickname and EMail properties are set. 
            if (account.Nickname == "" || account.Nickname == null)
            {
                throw new ArgumentException("Cannot save an account without a nickname.");
            }

            if (account.EMail == "" || account.EMail == null)
            {
                throw new ArgumentException("Cannot save an account without an e-mail address.");
            }

            //Fetches the record to change from the database. 
            Account toSave = (from a in dbContext.Accounts
                              where a.Id == account.Id
                              select a).First();

            //Check whether Nickname and EMail properties have changed and are still unique. 
            if (account.Nickname != toSave.Nickname && Accounts.Where(x => x.Nickname == account.Nickname).Any())
            {
                throw new ArgumentException("The user which should be created has a nickname which is already in use by another user");
            }
            if (account.EMail != toSave.EMail && Accounts.Where(x => x.EMail == account.EMail).Any())
            {
                throw new ArgumentException("The user which should be created has an e-mail address which is already in use by another user");
            }

            //Sets the updated values. 
            dbContext.Entry(toSave).CurrentValues.SetValues(account);

            //Saves all changes. 
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Sets the password for the given account by storing its SHA512 hash into the 
        /// AuthenticationToken property and committing the change to the database. 
        /// Other changes are not committed. 
        /// <remarks>If this method is called for an Account 
        /// which uses another id provider than the 
        /// EMailIdProvider class, this method fails by 
        /// throwing an ArgumentException. </remarks>
        /// </summary>
        /// <param name="account">The account to set the password for.</param>
        /// <param name="newPassword">The new password to set for the given account.</param>
        public void SetPassword(Account account, string newPassword)
        {
            //Checks whether the given account supports password authentication. 
            if (account.IdType != Controllers.EMailIdProviderController.ProviderIdentifier)
            {
                throw new InvalidOperationException("Cannot set a password for a user not authenticated via e-mail");
            }

            // Use a crypto random number generator to generate salt.
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            byte[] salt = new byte[160];
            rand.GetBytes(salt);

            rand.Dispose();

            // Get the unicode bytes, then compute the hash. 
            byte[] password = Encoding.Unicode.GetBytes(newPassword);

            byte[] hash = ComputeHash(salt, password);

            //Store the hash in the following format: hash;salt
            string hashString = Convert.ToBase64String(hash) + ";" + Convert.ToBase64String(salt);

            Account accountToSave = (from a in dbContext.Accounts
                                    where a.Id == account.Id
                                    select a).First();

            //Set the new hash
            accountToSave.AccountToken = hashString;
            account.AccountToken = hashString;

            //Save all changes
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Computes the SHA512 password from the given data.
        /// </summary>
        /// <param name="salt">The salt to use.</param>
        /// <param name="password">The password as unicode bytes to use.</param>
        /// <returns>The hash.</returns>
        private byte[] ComputeHash(byte[] salt, byte[] password)
        {
            //Get a byte array to work with.
            byte[] digest = new byte[salt.Length + password.Length];

            //Copy salt and passwort into the digest array.
            salt.CopyTo(digest, 0);
            password.CopyTo(digest, salt.Length);

            //Compute the hash. 
            SHA512CryptoServiceProvider sha = new SHA512CryptoServiceProvider();

            sha.Initialize();

            byte[] hash = sha.ComputeHash(digest);

            sha.Dispose();

            //Return the hash. 
            return hash;
        }

        /// <summary>
        /// Validates whether the given password matches the given Account object.
        /// <remarks>If this method is called for an Account 
        /// which uses another id provider than the 
        /// EMailIdProvider class, this method fails by 
        /// throwing an ArgumentException. </remarks>
        /// </summary>
        /// <param name="account">The account object to check.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>True, if the password matched, false if not.</returns>
        public bool ValidatePassword(Account account, string password)
        {
            //Checks whether the given account supports password authentication. 
            if (account.IdType != Controllers.EMailIdProviderController.ProviderIdentifier) 
            {
                throw new InvalidOperationException("Cannot validate a password for a user not authenticated via e-mail");
            }

            // Split the auth token to get hash and salt
            string[] split = account.AccountToken.Split(';');
            string hashString = split[0];
            string saltString = split[1];

            byte[] salt = Convert.FromBase64String(saltString);
            byte[] goodHash = Convert.FromBase64String(hashString);

            // Calculate the hash using the given password.
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            byte[] hash = ComputeHash(salt, passwordBytes);

            bool valid = true;

            // Compare the hashes. Fast comparision (breaking the loop after a miss) is intentionally omitted for defense against time attacks. 
            for (int i = 0; i < goodHash.Length; i++)
            {
                if (hash[i] != goodHash[i])
                    valid = false;
            }

            //Return the result. 
            return valid;
        }

        /// <summary>
        /// Checks whether the given auth token matches the given user id.
        /// <remarks>
        /// This function is used for client authentication via auth token. 
        /// </remarks>
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <param name="userid">The user id to check the token for.</param>
        /// <returns>The account with the given id or null, if the token did not match the id. </returns>
        public Account ValidateAuthToken(string token, int userid)
        {
            return (from account in dbContext.Accounts.AsNoTracking()
                    where account.AuthenticationToken == token && account.Id == userid
                    select account).FirstOrDefault();
        }

        /// <summary>
        /// nvokes the FriendshipAdded event using the given parameters. 
        /// </summary>
        /// <param name="a">The first user of the friendship relation.</param>
        /// <param name="b">The second user of the friendship relation.</param>
        /// <param name="wasInvited">A bool specifying whether user b was invited by user a. </param>
        protected static void InvokeFriendshipAdded(User a, User b, bool wasInvited)
        {
            if (FriendshipAdded != null)
            {
                FriendshipAdded(null, new FriendshipAddedEventArgs(a, b, wasInvited));
            }
        }

        /// <summary>
        /// Invokes the UserCreated event using the given parameters.
        /// </summary>
        /// <param name="user">The created user. </param>
        protected static void InvokeUserCreated(User user)
        {
            if (UserCreated != null)
            {
                UserCreated(null, new UserCreadedEventArgs(user));
            }
        }


    }
}
