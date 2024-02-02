using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class TransactionMetaV3
{
    public ExtensionPoint ExtensionPoint { get; set; } = new ExtensionPointZero();

    public LedgerEntryChange[] TransactionChangesBefore { get; set; } = Array.Empty<LedgerEntryChange>();
    public LedgerEntryChange[] TransactionChangesAfter { get; set; } = Array.Empty<LedgerEntryChange>();
    public LedgerEntryChange[][] Operations { get; set; } = Array.Empty<LedgerEntryChange[]>();

    public SorobanTransactionMeta? SorobanMeta { get; set; }

    public xdr.TransactionMetaV3 ToXdrMetaV3()
    {
        return new xdr.TransactionMetaV3
        {
            Ext = ExtensionPoint.ToXdr(),
            TxChangesBefore = new LedgerEntryChanges(TransactionChangesBefore.Select(x => x.ToXdr()).ToArray()),
            Operations = Operations.Select(x => new OperationMeta
            {
                Changes = new LedgerEntryChanges(x.Select(y => y.ToXdr()).ToArray())
            }).ToArray(),
            TxChangesAfter = new LedgerEntryChanges(TransactionChangesAfter.Select(x => x.ToXdr()).ToArray()),
            SorobanMeta = SorobanMeta?.ToXdr()
        };
    }

    public static TransactionMetaV3 FromXdr(xdr.TransactionMetaV3 xdr)
    {
        return new TransactionMetaV3
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdr.Ext),
            TransactionChangesBefore = xdr.TxChangesBefore.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray(),
            TransactionChangesAfter = xdr.TxChangesAfter.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray(),
            Operations = xdr.Operations.Select(x => x.Changes.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray())
                .ToArray(),
            SorobanMeta = SorobanTransactionMeta.FromXdr(xdr.SorobanMeta)
        };
    }

    /// <summary>
    ///     Creates a new <see cref="TransactionMetaV3" /> object from the given TransactionMetaV3 XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns><see cref="xdr.TransactionMetaV3" /> object</returns>
    public static TransactionMetaV3 FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = TransactionMeta.Decode(reader);
        return FromXdr(thisXdr.V3);
    }
}