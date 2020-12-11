﻿using System;

namespace GPNBot.API.WS.Model
{
    public class RemoteException
    {
        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        /// <value>The exception message.</value>
        public string Message { get; set; } = $"A remote exception occured";

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteException"/> class.
        /// </summary>
        public RemoteException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteException"/> class.
        /// </summary>
        /// <param name="exception">The exception that occured.</param>
        public RemoteException(Exception exception)
        {
            Message = $"A remote exception occured: '{exception.Message}'.";
        }
    }
}
