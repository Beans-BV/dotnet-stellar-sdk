/*
 *  Copyright 2014 Jonathan Bradshaw. All rights reserved.
 *  Redistribution and use in source and binary forms, with or without modification, is permitted.
 */

namespace StellarDotnetSdk.EventSources;

public sealed partial class EventSource
{
    /// <summary>
    ///     The possible values of the readyState property.
    /// </summary>
    public enum EventSourceState
    {
        CONNECTING = 0,
        OPEN = 1,
        CLOSED = 2,
        SHUTDOWN = 3,
        RAW = 4,
    }
}