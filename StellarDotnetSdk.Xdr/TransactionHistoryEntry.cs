// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TransactionHistoryEntry
//  {
//      uint32 ledgerSeq;
//      TransactionSet txSet;
//  
//      // when v != 0, txSet must be empty
//      union switch (int v)
//      {
//      case 0:
//          void;
//      case 1:
//          GeneralizedTransactionSet generalizedTxSet;
//      }
//      ext;
//  };

//  ===========================================================================
public class TransactionHistoryEntry
{
    public Uint32 LedgerSeq { get; set; }
    public TransactionSet TxSet { get; set; }
    public TransactionHistoryEntryExt Ext { get; set; }

    public static void Encode(XdrDataOutputStream stream, TransactionHistoryEntry encodedTransactionHistoryEntry)
    {
        Uint32.Encode(stream, encodedTransactionHistoryEntry.LedgerSeq);
        TransactionSet.Encode(stream, encodedTransactionHistoryEntry.TxSet);
        TransactionHistoryEntryExt.Encode(stream, encodedTransactionHistoryEntry.Ext);
    }

    public static TransactionHistoryEntry Decode(XdrDataInputStream stream)
    {
        var decodedTransactionHistoryEntry = new TransactionHistoryEntry();
        decodedTransactionHistoryEntry.LedgerSeq = Uint32.Decode(stream);
        decodedTransactionHistoryEntry.TxSet = TransactionSet.Decode(stream);
        decodedTransactionHistoryEntry.Ext = TransactionHistoryEntryExt.Decode(stream);
        return decodedTransactionHistoryEntry;
    }

    public class TransactionHistoryEntryExt
    {
        public int Discriminant { get; set; }

        public GeneralizedTransactionSet GeneralizedTxSet { get; set; }

        public static void Encode(XdrDataOutputStream stream,
            TransactionHistoryEntryExt encodedTransactionHistoryEntryExt)
        {
            stream.WriteInt(encodedTransactionHistoryEntryExt.Discriminant);
            switch (encodedTransactionHistoryEntryExt.Discriminant)
            {
                case 0:
                    break;
                case 1:
                    GeneralizedTransactionSet.Encode(stream, encodedTransactionHistoryEntryExt.GeneralizedTxSet);
                    break;
            }
        }

        public static TransactionHistoryEntryExt Decode(XdrDataInputStream stream)
        {
            var decodedTransactionHistoryEntryExt = new TransactionHistoryEntryExt();
            var discriminant = stream.ReadInt();
            decodedTransactionHistoryEntryExt.Discriminant = discriminant;
            switch (decodedTransactionHistoryEntryExt.Discriminant)
            {
                case 0:
                    break;
                case 1:
                    decodedTransactionHistoryEntryExt.GeneralizedTxSet = GeneralizedTransactionSet.Decode(stream);
                    break;
            }

            return decodedTransactionHistoryEntryExt;
        }
    }
}