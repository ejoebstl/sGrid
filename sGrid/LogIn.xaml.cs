using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using sGrid;
using System.Runtime.InteropServices;
using System.Reflection;

namespace sGridClientUI
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    partial class LogIn : Window
    {
        /// <summary>
        /// The clientController performs the actions requested
        /// through this class.
        /// </summary>
        private ClientController clientController;

        private System.Windows.Forms.WebBrowser browser;

        /// <summary>
        /// The uri called for the login process.
        /// </summary>
        private const string uri = "http://sgrid.azurewebsites.net/ClientLogin/?token={0}&port={1}&disableLayout=true";

        /// <summary>
        /// Creates a new instance of this class and initializes
        /// the values.
        /// </summary>
        /// <param name="clientController">The clientController
        /// performing the actions.</param>
        internal LogIn(ClientController clientController)
        {
            this.InitializeComponent();

            browser = new System.Windows.Forms.WebBrowser();
            browser.Dock = System.Windows.Forms.DockStyle.Fill;
            browser.ScriptErrorsSuppressed = true;

            windowsFormsHost.Child = browser;

            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);

            this.clientController = clientController;
            Title = sGrid.Resources.ClientRes.LogIn;
        }

        /// <summary>
        /// Initializes the browser performing the login.
        /// This method is invoked every time a new user logs in.
        /// <param name="resetSession">True, to reset the session of the user, else, false.</param>
        /// </summary>
        public void InitializeBrowser(bool resetSession = false)
        {
            // Use a crypto random number generator to generate the anti-forgery token.
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            byte[] aftBytes = new byte[64];
            rand.GetBytes(aftBytes);
            string aft = Convert.ToBase64String(aftBytes);
            aft = Convert.ToBase64String(aftBytes);
            aft = aft.Replace("+", "");
            aft = aft.Replace("/", "");
            aft = aft.Replace("=", "");
            clientController.AntiForgeryToken = aft.Replace(" ", "");

            browser.Navigate("about:blank");
            //invoke Browser

            string targetUrl = String.Format(uri, clientController.AntiForgeryToken, ClientEventController.Port);

            if (resetSession)
            {
                targetUrl = targetUrl + "&resetSession=true";
            }

            browser.Navigate(new Uri(targetUrl));
        }


        /// <summary>
        /// This event is triggered, when this window is closed.
        /// </summary>
        /// <param name="sender">The object which triggered the event.</param>
        /// <param name="e">The arguments that are transferred with
        /// this event, namely a value indicating if the closing
        /// process should be cancelled.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
