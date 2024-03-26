﻿namespace stellar_dotnet_sdk;

public interface ITransactionBuilderAccount
{
    string AccountId { get; }

    KeyPair KeyPair { get; }
    IAccountId MuxedAccount { get; }
    long SequenceNumber { get; }

    /// <summary>
    ///     Returns sequence number incremented by one, but does not increment internal counter.
    /// </summary>
    long IncrementedSequenceNumber { get; }

    void SetSequenceNumber(long sequenceNumber);

    /// <summary>
    ///     Increments sequence number in this object by one.
    /// </summary>
    void IncrementSequenceNumber();
}