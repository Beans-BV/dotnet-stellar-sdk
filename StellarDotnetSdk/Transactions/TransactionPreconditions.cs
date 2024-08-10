using System;
using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Xdr;
using FormatException = StellarDotnetSdk.Exceptions.FormatException;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Transactions;

/// <summary>
///     Preconditions are checked first.
///     <p>
///         All preconditions are optional. Time bounds are encouraged, but the other preconditions are used in more
///         specialized circumstances. You can set multiple preconditions as long as the combination is logically sound.
///     </p>
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/stellar-data-structures/operations-and-transactions#preconditions-optional">
///         Preconditions
///     </a>
/// </summary>
public class TransactionPreconditions
{
    private const long MaxExtraSignersCount = 2;
    public const ulong TimeoutInfinite = 0;

    public LedgerBounds? LedgerBounds { get; init; }

    /// <summary>
    ///     Transaction is valid after a particular duration (expressed in seconds) elapses since the account’s sequence number
    ///     age.
    /// </summary>
    public ulong? MinSequenceAge { get; init; } = 0UL;

    public long? MinSequenceNumber { get; init; }
    public uint? MinSequenceLedgerGap { get; init; } = 0U;
    public List<SignerKey>? ExtraSigners { get; init; }

    /// <summary>
    ///     Valid if within set time bounds of the transaction.
    /// </summary>
    public TimeBounds? TimeBounds { get; set; }

    public void IsValid()
    {
        if (ExtraSigners?.Count > MaxExtraSignersCount)
        {
            throw new FormatException(
                $"Invalid preconditions, too many extra signers, can only have up to {MaxExtraSignersCount}.");
        }
    }

    public bool HasV2()
    {
        return LedgerBounds != null ||
               MinSequenceLedgerGap > 0 ||
               MinSequenceAge > 0 ||
               MinSequenceNumber != null ||
               (ExtraSigners != null && ExtraSigners.Count != 0);
    }

    public static TransactionPreconditions? FromXdr(Preconditions preconditions)
    {
        return preconditions.Discriminant.InnerValue switch
        {
            PreconditionType.PreconditionTypeEnum.PRECOND_V2 => new TransactionPreconditions
            {
                LedgerBounds = preconditions.V2.LedgerBounds != null
                    ? LedgerBounds.FromXdr(preconditions.V2.LedgerBounds)
                    : null,
                MinSequenceAge = preconditions.V2.MinSeqAge?.InnerValue.InnerValue,
                MinSequenceNumber = preconditions.V2.MinSeqNum?.InnerValue.InnerValue,
                MinSequenceLedgerGap = preconditions.V2.MinSeqLedgerGap?.InnerValue,
                ExtraSigners = preconditions.V2.ExtraSigners.Length > 0
                    ? preconditions.V2.ExtraSigners.ToList()
                    : null,
                TimeBounds = preconditions.V2.TimeBounds != null
                    ? TimeBounds.FromXdr(preconditions.V2.TimeBounds)
                    : null,
            },
            PreconditionType.PreconditionTypeEnum.PRECOND_TIME => new TransactionPreconditions
            {
                TimeBounds = preconditions.TimeBounds != null
                    ? TimeBounds.FromXdr(preconditions.TimeBounds)
                    : null,
            },
            _ => null,
        };
    }

    public Preconditions ToXdr()
    {
        if (HasV2())
        {
            return new Preconditions
            {
                Discriminant = PreconditionType.Create(PreconditionType.PreconditionTypeEnum.PRECOND_V2),
                V2 = new PreconditionsV2
                {
                    TimeBounds = TimeBounds?.ToXdr(),
                    LedgerBounds = LedgerBounds?.ToXdr(),
                    MinSeqNum = MinSequenceNumber.HasValue
                        ? new SequenceNumber(new Int64(MinSequenceNumber.Value))
                        : null,
                    MinSeqAge = new Duration(new Uint64(MinSequenceAge ?? 0UL)),
                    MinSeqLedgerGap = new Uint32(MinSequenceLedgerGap ?? 0U),
                    ExtraSigners = ExtraSigners?.ToArray() ?? Array.Empty<SignerKey>(),
                },
            };
        }

        if (TimeBounds != null)
        {
            return new Preconditions
            {
                Discriminant = PreconditionType.Create(PreconditionType.PreconditionTypeEnum.PRECOND_TIME),
                TimeBounds = TimeBounds.ToXdr(),
            };
        }
        return new Preconditions
        {
            Discriminant = PreconditionType.Create(PreconditionType.PreconditionTypeEnum.PRECOND_NONE),
        };
    }
}