// This source file is part of BoincGuiRpc.Net
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://boincguirpc.codeplex.com
//
// Licensed under the MIT License
//
// (c) 2012-2013

using System;
using System.Collections.Generic;
using System.Text;

namespace Boinc
{
    /// <summary>
    /// This class wraps Boinc proxy settings. 
    /// </summary>
    public class ProxySettings
    {
        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; private set; }
        /// <summary>
        /// Gets the hostname of the proxy server.
        /// </summary>
        public string ProxyServer { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the parameters into their corresponding properties. 
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="proxyServer">The hostname of the proxy server.</param>
        /// <param name="username">The username.</param>
        public ProxySettings(string password, string proxyServer, string username)
        {
            this.Password = password;
            this.ProxyServer = proxyServer;
            this.Username = username;
        }
    }
}
