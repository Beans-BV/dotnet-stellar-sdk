// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct LedgerCloseMetaV0
//  {
//      LedgerHeaderHistoryEntry ledgerHeader;
//      // NB: txSet is sorted in "Hash order"
//      TransactionSet txSet;
//  
//      // NB: transactions are sorted in apply order here
//      // fees for all transactions are processed first
//      // followed by applying transactions
//      TransactionResultMeta txProcessing<>;
//  
//      // upgrades are applied last
//      UpgradeEntryMeta upgradesProcessing<>;
//  
//      // other misc information attached to the ledger close
//      SCPHistoryEntry scpInfo<>;
//  };

//  ===========================================================================
public class LedgerCloseMetaV0
{
    public LedgerHeaderHistoryEntry LedgerHeader { get; set; }
    public TransactionSet TxSet { get; set; }
    public TransactionResultMeta[] TxProcessing { get; set; }
    public UpgradeEntryMeta[] UpgradesProcessing { get; set; }
    public SCPHistoryEntry[] ScpInfo { get; set; }

    public static void Encode(XdrDataOutputStream stream, LedgerCloseMetaV0 encodedLedgerCloseMetaV0)
    {
        LedgerHeaderHistoryEntry.Encode(stream, encodedLedgerCloseMetaV0.LedgerHeader);
        TransactionSet.Encode(stream, encodedLedgerCloseMetaV0.TxSet);
        var txProcessingsize = encodedLedgerCloseMetaV0.TxProcessing.Length;
        stream.WriteInt(txProcessingsize);
        for (var i = 0; i < txProcessingsize; i++)
        {
            TransactionResultMeta.Encode(stream, encodedLedgerCloseMetaV0.TxProcessing[i]);
        }
        var upgradesProcessingsize = encodedLedgerCloseMetaV0.UpgradesProcessing.Length;
        stream.WriteInt(upgradesProcessingsize);
        for (var i = 0; i < upgradesProcessingsize; i++)
        {
            UpgradeEntryMeta.Encode(stream, encodedLedgerCloseMetaV0.UpgradesProcessing[i]);
        }
        var scpInfosize = encodedLedgerCloseMetaV0.ScpInfo.Length;
        stream.WriteInt(scpInfosize);
        for (var i = 0; i < scpInfosize; i++)
        {
            SCPHistoryEntry.Encode(stream, encodedLedgerCloseMetaV0.ScpInfo[i]);
        }
    }

    public static LedgerCloseMetaV0 Decode(XdrDataInputStream stream)
    {
        var decodedLedgerCloseMetaV0 = new LedgerCloseMetaV0();
        decodedLedgerCloseMetaV0.LedgerHeader = LedgerHeaderHistoryEntry.Decode(stream);
        decodedLedgerCloseMetaV0.TxSet = TransactionSet.Decode(stream);
        var txProcessingsize = stream.ReadInt();
        decodedLedgerCloseMetaV0.TxProcessing = new TransactionResultMeta[txProcessingsize];
        for (var i = 0; i < txProcessingsize; i++)
        {
            decodedLedgerCloseMetaV0.TxProcessing[i] = TransactionResultMeta.Decode(stream);
        }
        var upgradesProcessingsize = stream.ReadInt();
        decodedLedgerCloseMetaV0.UpgradesProcessing = new UpgradeEntryMeta[upgradesProcessingsize];
        for (var i = 0; i < upgradesProcessingsize; i++)
        {
            decodedLedgerCloseMetaV0.UpgradesProcessing[i] = UpgradeEntryMeta.Decode(stream);
        }
        var scpInfosize = stream.ReadInt();
        decodedLedgerCloseMetaV0.ScpInfo = new SCPHistoryEntry[scpInfosize];
        for (var i = 0; i < scpInfosize; i++)
        {
            decodedLedgerCloseMetaV0.ScpInfo[i] = SCPHistoryEntry.Decode(stream);
        }
        return decodedLedgerCloseMetaV0;
    }
}