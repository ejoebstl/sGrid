using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using sGridServer.Code.DataAccessLayer.Models;
using sGridServer.Code.Achievements;
using sGridServer.Code.Rewards;

namespace sGridServer.Code.DataAccessLayer
{
    /// <summary>
    /// This class represents a context of the database and provides the access to it.
    /// </summary>
    public class SGridDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets a database set containing the authentication information for grid providers.
        /// </summary>
        public DbSet<GridProviderAuthenticationData> DataForGridProvider { get; set; }

        /// <summary>
        /// Gets or sets a database set containing user attached projects.
        /// </summary>
        public DbSet<AttachedProject> AttachedProjects { get; set; }

        /// <summary>
        /// Gets a database set containing calculated results.
        /// </summary>
        public DbSet<CalculatedResult> CalculatedResults { get; set; }

        /// <summary>
        /// Gets a database set containing all friend requests between users.
        /// </summary>
        public DbSet<FriendRequest> FriendRequests { get; set; }

        /// <summary>
        /// Gets a database set containing the friendships information between all users.
        /// </summary>
        public DbSet<Friendship> Friendships { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all accounts.
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all achievements.
        /// </summary>
        public DbSet<Achievement> Achievements { get; set; }

        /// <summary>
        /// Gets or sets a database set containing achievements already obtained by users.
        /// </summary>
        public DbSet<ObtainedAchievement> ObtainedAchievements { get; set; }

        /// <summary>
        /// Gets or sets database set containing all coin accounts.
        /// </summary>
        public DbSet<CoinAccount> CoinAccounts { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all purchases which were made using the coin exchange.
        /// </summary>
        public DbSet<Purchase> Purchases { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all reward ratings by users.
        /// </summary>
        public DbSet<Rating> Ratings { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all rewards in the database.
        /// </summary>
        public DbSet<Reward> Rewards { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all transactions in the database.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all transactions in the database.
        /// </summary>
        public DbSet<MultiLanguageString> Strings { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all transactions in the database.
        /// </summary>
        public DbSet<Translation> Translations { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all errors.
        /// </summary>
        public DbSet<Error> Errors { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all news.
        /// </summary>
        public DbSet<News> News { get; set; }

        /// <summary>
        /// Gets or sets a database set containing all support messages.
        /// </summary>
        public DbSet<Message> Messages { get; set; }


        /// <summary>
        /// Initializes a database set of this context class.
        /// </summary>
        /// <param name="modelBuilder"> Defines the model for the context being created.</param> 
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Prevent errors on delete due to circular dependencies. 
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();

            //Correct table mapping for inherited classes. 
            modelBuilder.Ignore<AchievementItem>();
            modelBuilder.Ignore<RewardItem>();
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<CoinPartner>().ToTable("CoinPartners");
            modelBuilder.Entity<Partner>().ToTable("Partners");
            modelBuilder.Entity<SGridTeamMember>().ToTable("SGridTeamMembers");
            modelBuilder.Entity<Sponsor>().ToTable("Sponsors");
            modelBuilder.Entity<User>().ToTable("Users");
        }
    }
}