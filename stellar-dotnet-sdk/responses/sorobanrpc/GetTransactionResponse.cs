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

    public int ApplicationOrder;

    public long CreatedAt;

    public string? EnvelopeXdr;

    public bool FeeBump;

    public long LatestLedger;

    public long LatestLedgerCloseTime;

    public long Ledger;

    public long OldestLedger;

    public long OldestLedgerCloseTime;

    public string? ResultMetaXdr;

    public string? ResultXdr;
    public GetTransactionStatus Status;

    public SCVal? ResultValue
    {
        get
        {
            if (Status != GetTransactionStatus.SUCCESS || ResultMetaXdr == null) return null;

            var bytes = Convert.FromBase64String(ResultMetaXdr);
            var reader = new XdrDataInputStream(bytes);
            var meta = TransactionMeta.Decode(reader);
            return meta?.V3?.SorobanMeta?.ReturnValue == null ? null : SCVal.FromXdr(meta.V3.SorobanMeta.ReturnValue);
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