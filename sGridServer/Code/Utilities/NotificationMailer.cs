using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using sGridServer.Code.Achievements;
using sGridServer.Code.GridProviders;
using System.Net.Mail;
using sGridServer.Code.DataAccessLayer.Models;
using Resource = sGridServer.Resources.Utilities.NotificationMailer;

namespace sGridServer.Code.Utilities
{
    /// <summary>
    /// This static class registers itself on the AchiementManager.AchievementObtained,
    /// CoinExchange.TransactionDone and the GridProviderManager.ProjectChanged event 
    /// using its static constructor. If any of the given events is raised, the 
    /// NotificationMailer class checks the settings of the associated user and 
    /// sends out a notification mail if applicable. 
    /// </summary>
    public class NotificationMailer
    {
        /// <summary>
        /// Gets the sender address.
        /// </summary>
        public static string SenderAddress
        {
            get { return Properties.Settings.Default.NotificationMailFromAddress;  }
        }

        /// <summary>
        /// Registers with the desired events.
        /// </summary>
        static NotificationMailer()
        {
            //Register with events (one-time). 
            AchievementItem.AchievementObtained += new EventHandler<AchievementEventArgs>(OnAchievementUnlocked);
            CoinExchange.CoinExchange.TransactionDone += new EventHandler<CoinExchange.TransactionEventArgs>(OnCoinTransaction);
            GridProviderManager.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(OnProjectChanged);
        }

        /// <summary>
        /// Is called whenever the a user changes his associated project 
        /// and sends out a notification mail if desired by the 
        /// corresponding user, using the SendMail method. 
        /// </summary>
        /// <param name="sender">The object which invoked the event.</param>
        /// <param name="e">The event arguments associated with the event. </param>
        private static void OnProjectChanged(object sender, ProjectChangedEventArgs e)
        {
            if(e.IsAttach && e.User.NotifyOnProjectChanged && IsValidEMailAddress(e.User.EMail))
            {
                //If desired, send out the mail. 
                LanguageManager.SetThreadCulture(e.User); //Set the language according to the user. 

                string message = String.Format(Resource.ProjectChangedMail, e.User.Nickname, e.Project.Name);

                SendMail(e.User.EMail, Resource.ProjectChangedHeader, message);

                LanguageManager.SetThreadCulture();
            }
        }

        /// <summary>
        /// Returns a bool indicating whether the given e-mail address is valid. 
        /// </summary>
        /// <param name="address">The address string.</param>
        /// <returns>True, if address is a vaild addrss, else false.</returns>
        private static bool IsValidEMailAddress(string address)
        {
            try
            {
                MailAddress a = new MailAddress(address);
                return true;
            }
            catch 
            {
                return false;
            }
        
        }

        /// <summary>
        /// Is called whenever a coin transaction is done and sends out a 
        /// notification mail if desired by the corresponding user, 
        /// using the SendMail method. 
        /// </summary>
        /// <param name="sender">The object which invoked the event.</param>
        /// <param name="e">The event arguments associated with the event.</param>
        private static void OnCoinTransaction(object sender, CoinExchange.TransactionEventArgs e)
        {
            Account dest = e.Destination;
            Account src = e.Destination;

            if (dest != null && dest.NotifyOnCoinBalanceChanged && IsValidEMailAddress(dest.EMail))
            {
                //If desired, send out a mail for the destination account. 
                LanguageManager.SetThreadCulture(dest); //Set the language according to the user. 

                string message = String.Format(Resource.CoinTransactionDestinationMail, dest.Nickname, e.Value, dest.CoinAccount.CurrentBalance);

                SendMail(dest.EMail, Resource.CoinTransactionHeader, message);

                LanguageManager.SetThreadCulture();
            }


            if (src != null && src.NotifyOnCoinBalanceChanged && IsValidEMailAddress(src.EMail))
            {
                //If desired, send out a mail for the destination account. 
                LanguageManager.SetThreadCulture(src); //Set the language according to the user. 

                string message = String.Format(Resource.CoinTransactionSourceMail, dest.Nickname, e.Value, dest.CoinAccount.CurrentBalance);

                SendMail(dest.EMail, Resource.CoinTransactionHeader, message);

                LanguageManager.SetThreadCulture();
            }
        }

        /// <summary>
        /// Is called whenever an achievement is unlocked and sends out a notification mail if desired 
        /// by the corresponding user, using the SendMail method. 
        /// </summary>
        /// <param name="sender">The object which invoked the event.</param>
        /// <param name="e">The event arguments associated with the event.</param>
        private static void OnAchievementUnlocked(object sender, AchievementEventArgs e)
        {
            if (e.User.NotifyOnAchievementReached && IsValidEMailAddress(e.User.EMail))
            {
                //If desired, send out a mail. 
                LanguageManager.SetThreadCulture(e.User); //Set the language according to the user. 

                string message = String.Format(Resource.AchievementUnlockedMail, e.User.Nickname, e.Achievement.Name);

                SendMail(e.User.EMail, Resource.AchievementUnlockedHeader, message);

                LanguageManager.SetThreadCulture();
            }
        }

        /// <summary>
        /// Sends out an e-mail using the localhost mail server.
        /// </summary>
        /// <param name="recipient">The recipient of the message.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body text of the message.</param>
        private static void SendMail(string recipient, string subject, string body)
        {
            MailMessage message = new MailMessage(SenderAddress, recipient, subject, body);

            SmtpClient mailer = new SmtpClient("localhost");

            mailer.Send(message);
        }
    }
}