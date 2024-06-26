﻿using System;
using System.Runtime.Serialization;

namespace StellarDotnetSdk.Exceptions;

[Serializable]
public class TooManySignaturesException : Exception
{
    public TooManySignaturesException()
    {
    }

    public TooManySignaturesException(string message) : base(message)
    {
    }

    public TooManySignaturesException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected TooManySignaturesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}