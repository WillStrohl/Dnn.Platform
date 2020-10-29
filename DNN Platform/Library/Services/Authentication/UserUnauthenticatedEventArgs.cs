// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;

    using DotNetNuke.Entities.Users;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserUnauthenticatedEventArgs class provides a custom EventArgs object for the
    /// UserUnauthenticated event.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class UserUnauthenticatedEventArgs : EventArgs
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserUnauthenticatedEventArgs"/> class.
        /// All properties Constructor.
        /// </summary>
        /// <param name="user">The user being authenticated.</param>
        /// <param name="type">The type of Authentication.</param>
        /// -----------------------------------------------------------------------------
        public UserUnauthenticatedEventArgs(UserInfo user, string type)
        {
            this.Message = string.Empty;
            this.User = user;
            this.AuthenticationType = type;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Authentication Type.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Message.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Message { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public UserInfo User { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Username.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string UserName {
            get
            {
                return User.Username;
            }
        }
    }
}
