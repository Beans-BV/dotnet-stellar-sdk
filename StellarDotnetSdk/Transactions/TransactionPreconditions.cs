using System.Collections.Generic;
using System.Linq;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Transactions;

public class TransactionPreconditions
{
    public static long MaxExtraSignersCount = 2;
    public static ulong TimeoutInfinite = 0;

    public LedgerBounds LedgerBounds { get; set; }
    public ulong MinSeqAge { get; set; }
    public long? MinSeqNumber { get; set; }
    public uint MinSeqLedgerGap { get; set; }
    public List<SignerKey> ExtraSigners { get; set; } = new();
    public TimeBounds TimeBounds { get; set; }

    public void IsValid()
    {
        if (TimeBounds == null) throw new FormatException("Invalid preconditions, must define time bounds.");

        if (ExtraSigners.Count > MaxExtraSignersCount)
            throw new FormatException("Invalid preconditions, too many extra signers, can only have up to " +
                                      MaxExtraSignersCount);
    }

    public bool HasV2()
    {
        return LedgerBounds != null ||
               MinSeqLedgerGap > 0 ||
               MinSeqAge > 0 ||
               MinSeqNumber != null ||
               ExtraSigners.Count != 0;
    }

    public static TransactionPreconditions FromXDR(Preconditions preconditions)
    {
        var transactionPreconditions = new TransactionPreconditions();

        if (preconditions.Discriminant.InnerValue == PreconditionType.PreconditionTypeEnum.PRECOND_V2)
        {
            //Time Bounds
            if (preconditions.V2.TimeBounds != null)
                transactionPreconditions.TimeBounds = new TimeBounds(
                    preconditions.V2.TimeBounds.MinTime.InnerValue.InnerValue,
                    preconditions.V2.TimeBounds.MaxTime.InnerValue.InnerValue);

            //Extra Signers
            if (preconditions.V2.ExtraSigners != null && preconditions.V2.ExtraSigners.Length > 0)
                transactionPreconditions.ExtraSigners = preconditions.V2.ExtraSigners.ToList();

            //Min Seq Age
            if (preconditions.V2.MinSeqAge != null)
                transactionPreconditions.MinSeqAge = preconditions.V2.MinSeqAge.InnerValue.InnerValue;

            //Ledger Bounds
            if (preconditions.V2.LedgerBounds != null)
                transactionPreconditions.LedgerBounds = LedgerBounds.FromXdr(preconditions.V2.LedgerBounds);

            //Min Seq Num
            if (preconditions.V2.MinSeqNum != null)
                transactionPreconditions.MinSeqNumber = preconditions.V2.MinSeqNum.InnerValue.InnerValue;

            //Min Seq Ledger Gap
            if (preconditions.V2.MinSeqLedgerGap != null)
                transactionPreconditions.MinSeqLedgerGap = preconditions.V2.MinSeqLedgerGap.InnerValue;
        }
        else
        {
            if (preconditions.TimeBounds != null)
                transactionPreconditions.TimeBounds = new TimeBounds(
                    preconditions.TimeBounds.MinTime.InnerValue.InnerValue,
                    preconditions.TimeBounds.MaxTime.InnerValue.InnerValue);
        }

        return transactionPreconditions;
    }

    public Preconditions ToXDR()
    {
        var preconditions = new Preconditions();

        if (HasV2())
        {
            preconditions.Discriminant.InnerValue = PreconditionType.PreconditionTypeEnum.PRECOND_V2;

            var preconditionsV2 = new PreconditionsV2();
            preconditions.V2 = preconditionsV2;

            preconditionsV2.ExtraSigners = ExtraSigners.ToArray();
            preconditionsV2.MinSeqAge = new Duration(new Uint64(MinSeqAge));

            if (LedgerBounds != null)
            {
                var ledgerBoundsXDR = new Xdr.LedgerBounds();
                ledgerBoundsXDR.MinLedger = new Uint32(LedgerBounds.MinLedger);
                ledgerBoundsXDR.MaxLedger = new Uint32(LedgerBounds.MaxLedger);

                preconditionsV2.LedgerBounds = ledgerBoundsXDR;
            }

            if (MinSeqNumber != null) preconditionsV2.MinSeqNum = new SequenceNumber(new Int64(MinSeqNumber.Value));

            preconditionsV2.MinSeqLedgerGap = new Uint32(MinSeqLedgerGap);

            if (TimeBounds != null) preconditionsV2.TimeBounds = TimeBounds.ToXdr();
        }
        else
        {
            if (TimeBounds == null)
            {
                preconditions.Discriminant.InnerValue = PreconditionType.PreconditionTypeEnum.PRECOND_NONE;
            }
            else
            {
                preconditions.Discriminant.InnerValue = PreconditionType.PreconditionTypeEnum.PRECOND_TIME;
                preconditions.TimeBounds = TimeBounds.ToXdr();
            }
        }

        return preconditions;
    }
}