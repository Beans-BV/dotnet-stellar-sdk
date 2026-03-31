/*
 *  Copyright 2014 Jonathan Bradshaw. All rights reserved.
 *  Redistribution and use in source and binary forms, with or without modification, is permitted.
 */

using System;

namespace StellarDotnetSdk.EventSources;

public sealed partial class EventSource
{
    /// <summary>
    ///     Server Sent Event Message Object
    /// </summary>
    public sealed class ServerSentEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerSentEventArgs" /> class with the event data.
        /// </summary>
        /// <param name="data">The event data payload.</param>
        public ServerSentEventArgs(string data)
        {
            Data = data;
        }

        /// <summary>
        ///     Gets the data.
        /// </summary>
        public string Data { get; }
    }
}