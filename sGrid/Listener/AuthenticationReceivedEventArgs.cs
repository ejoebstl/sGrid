using System;
using System.Collections.Generic;
using System.Text;
using sGrid.sGridClientApi;

namespace sGrid
{
    /// <summary>
    /// This class represents the arguments of the
    /// AuthenticationReceivedEvent.
    /// </summary>
    public class AuthenticationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the credential of the user. A Credential is made
        /// out of the UserId and the AuthenticationToken.
        /// </summary>
        public Credential Credential { get; private set; }

        /// <summary>
        /// Gets the anti-forgery token. This token has been send
        /// from the client to the server and is used to prevent
        /// external programs from falsifying the credential.
        /// </summary>
        public String antiForgeryToken { get; private set; }

        /// <summary>
        /// Creates a new instance of this class and stores the
        /// given Credential into the Credential property.
        /// </summary>
        /// <param name="authentication">The credential of the user.</param>
        public AuthenticationReceivedEventArgs(Credential authentication, String antiForgeryToken)
        {
            this.Credential = authentication;
            this.antiForgeryToken = antiForgeryToken;
        }
    }
}
