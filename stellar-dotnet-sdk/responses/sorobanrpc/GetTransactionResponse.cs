using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class GetTransactionResponse
{
    public enum GetTransactionStatus
    {
        NOT_FOUND,
        SUCCESS,
        FAILED
    }

    public GetTransactionResponse(int applicationOrder, long createdAt, string? envelopeXdr, bool feeBump,
        long latestLedger, long latestLedgerCloseTime, long ledger, long oldestLedger, long oldestLedgerCloseTime,
        string? resultMetaXdr, string? resultXdr, GetTransactionStatus status)
    {
        ApplicationOrder = applicationOrder;
        CreatedAt = createdAt;
        EnvelopeXdr = envelopeXdr;
        FeeBump = feeBump;
        LatestLedger = latestLedger;
        LatestLedgerCloseTime = latestLedgerCloseTime;
        Ledger = ledger;
        OldestLedger = oldestLedger;
        OldestLedgerCloseTime = oldestLedgerCloseTime;
        ResultMetaXdr = resultMetaXdr;
        ResultXdr = resultXdr;
        Status = status;
    }

    public int ApplicationOrder { get; }

    public long CreatedAt { get; }

    public string? EnvelopeXdr { get; }

    public bool FeeBump { get; }

    public long LatestLedger { get; }

    public long LatestLedgerCloseTime { get; }

    public long Ledger { get; }

    public long OldestLedger { get; }

    public long OldestLedgerCloseTime { get; }

    public string? ResultMetaXdr { get; }

    public string? ResultXdr { get; }
    public GetTransactionStatus Status { get; }

    public SCVal? ResultValue
    {
        get
        {
            if (Status != GetTransactionStatus.SUCCESS || ResultMetaXdr == null) return null;

            var bytes = Convert.FromBase64String(ResultMetaXdr);
            var reader = new XdrDataInputStream(bytes);
            var meta = xdr.TransactionMeta.Decode(reader);
            return meta.V3?.SorobanMeta?.ReturnValue == null ? null : SCVal.FromXdr(meta.V3.SorobanMeta.ReturnValue);
        }
    }

    /// <summary>
    ///     Holds the diagnostic information, useful for debugging purpose when the transaction fails.
    /// </summary>
    public TransactionMetaV3? TransactionMeta
    {
        get
        {
            if (ResultMetaXdr == null) return null;
            try
            {
                return TransactionMetaV3.FromXdrBase64(ResultMetaXdr);
            }
            catch
            {
                return null;
            }
        }
    }

    public string? WasmId
    {
        get
        {
            if (ResultValue is SCBytes bytes) return Convert.ToBase64String(bytes.InnerValue);

            return null;
        }
    }

    public string? CreatedContractId
    {
        get
        {
            if (ResultValue is SCContractId contract)
                return contract.InnerValue;
            return null;
        }
    }
}