using System;

namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Returns information and links relating to a single account.
/// </summary>
public class Account : ITransactionBuilderAccount
{
    /// <summary>
    ///     Class constructor.
    /// </summary>
    /// <param name="accountId">KeyPair associated with this Account</param>
    /// <param name="sequenceNumber">
    ///     Current sequence number of the account (can be obtained using dotnet-stellar-sdk or
    ///     horizon server)
    /// </param>
    public Account(string accountId, long sequenceNumber)
    {
        ArgumentException.ThrowIfNullOrEmpty(accountId);
        ArgumentNullException.ThrowIfNull(sequenceNumber);
        AccountId = accountId;
        SequenceNumber = sequenceNumber;
    }

    /// <summary>
    ///     Class constructor.
    /// </summary>
    /// <param name="muxedAccount">KeyPair associated with this Account</param>
    /// <param name="sequenceNumber">
    ///     Current sequence number of the account (can be obtained using dotnet-stellar-sdk or
    ///     horizon server)
    /// </param>
    public Account(MuxedAccount muxedAccount, long sequenceNumber)
    {
        ArgumentNullException.ThrowIfNull(sequenceNumber);
        AccountId = muxedAccount.Address;
        SequenceNumber = sequenceNumber;
    }

    /// <summary>
    ///     Returns the AccountID of the account.
    /// </summary>
    public string AccountId { get; init; }

    /// <summary>
    ///     Returns the KeyPair of the account.
    /// </summary>
    public KeyPair KeyPair
    {
        get
        {
            switch (MuxedAccount)
            {
                case KeyPair kp:
                    return kp;
                case MuxedAccount ma:
                    return ma.Key;
                default:
                    throw new Exception("Invalid Account MuxedAccount type");
            }
        }
    }

    /// <summary>
    ///     The sequence number
    /// </summary>
    public long SequenceNumber { get; private set; }

    /// <summary>
    ///     Returns the Sequence number incremented by one.
    /// </summary>
    /// <returns>SequenceNumber + 1</returns>
    public long IncrementedSequenceNumber => SequenceNumber + 1;

    /// <summary>
    ///     Increments sequence number in this object by one.
    /// </summary>
    public void IncrementSequenceNumber()
    {
        SequenceNumber++;
    }
}