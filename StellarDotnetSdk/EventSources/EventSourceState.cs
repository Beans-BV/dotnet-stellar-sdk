/*
 *  Copyright 2014 Jonathan Bradshaw. All rights reserved.
 *  Redistribution and use in source and binary forms, with or without modification, is permitted.
 */

namespace StellarDotnetSdk.EventSources;

/// <summary>
///     Provides types for Server-Sent Events (SSE) used by the Stellar Horizon streaming API.
/// </summary>
public sealed partial class EventSource
{
    /// <summary>
    ///     The possible values of the readyState property.
    /// </summary>
    public enum EventSourceState
    {
        /// <summary>The connection is being established.</summary>
        CONNECTING = 0,

        /// <summary>The connection is open and receiving events.</summary>
        OPEN = 1,

        /// <summary>The connection has been closed.</summary>
        CLOSED = 2,

        /// <summary>The connection has been permanently shut down.</summary>
        SHUTDOWN = 3,

        /// <summary>The connection is in raw mode.</summary>
        RAW = 4,
    }
}