// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TransactionMetaV2
//  {
//      LedgerEntryChanges txChangesBefore; // tx level changes before operations
//                                          // are applied if any
//      OperationMeta operations<>;         // meta for each operation
//      LedgerEntryChanges txChangesAfter;  // tx level changes after operations are
//                                          // applied if any
//  };

//  ===========================================================================
public class TransactionMetaV2
{
    public LedgerEntryChanges TxChangesBefore { get; set; }
    public OperationMeta[] Operations { get; set; }
    public LedgerEntryChanges TxChangesAfter { get; set; }

    public static void Encode(XdrDataOutputStream stream, TransactionMetaV2 encodedTransactionMetaV2)
    {
        LedgerEntryChanges.Encode(stream, encodedTransactionMetaV2.TxChangesBefore);
        var operationssize = encodedTransactionMetaV2.Operations.Length;
        stream.WriteInt(operationssize);
        for (var i = 0; i < operationssize; i++)
        {
            OperationMeta.Encode(stream, encodedTransactionMetaV2.Operations[i]);
        }
        LedgerEntryChanges.Encode(stream, encodedTransactionMetaV2.TxChangesAfter);
    }

    public static TransactionMetaV2 Decode(XdrDataInputStream stream)
    {
        var decodedTransactionMetaV2 = new TransactionMetaV2();
        decodedTransactionMetaV2.TxChangesBefore = LedgerEntryChanges.Decode(stream);
        var operationssize = stream.ReadInt();
        decodedTransactionMetaV2.Operations = new OperationMeta[operationssize];
        for (var i = 0; i < operationssize; i++)
        {
            decodedTransactionMetaV2.Operations[i] = OperationMeta.Decode(stream);
        }
        decodedTransactionMetaV2.TxChangesAfter = LedgerEntryChanges.Decode(stream);
        return decodedTransactionMetaV2;
    }
}