// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ConfigSettingContractLedgerCostV0
//  {
//      // Maximum number of ledger entry read operations per ledger
//      uint32 ledgerMaxReadLedgerEntries;
//      // Maximum number of bytes that can be read per ledger
//      uint32 ledgerMaxReadBytes;
//      // Maximum number of ledger entry write operations per ledger
//      uint32 ledgerMaxWriteLedgerEntries;
//      // Maximum number of bytes that can be written per ledger
//      uint32 ledgerMaxWriteBytes;
//  
//      // Maximum number of ledger entry read operations per transaction
//      uint32 txMaxReadLedgerEntries;
//      // Maximum number of bytes that can be read per transaction
//      uint32 txMaxReadBytes;
//      // Maximum number of ledger entry write operations per transaction
//      uint32 txMaxWriteLedgerEntries;
//      // Maximum number of bytes that can be written per transaction
//      uint32 txMaxWriteBytes;
//  
//      int64 feeReadLedgerEntry;  // Fee per ledger entry read
//      int64 feeWriteLedgerEntry; // Fee per ledger entry write
//  
//      int64 feeRead1KB;  // Fee for reading 1KB
//  
//      // The following parameters determine the write fee per 1KB.
//      // Write fee grows linearly until bucket list reaches this size
//      int64 bucketListTargetSizeBytes;
//      // Fee per 1KB write when the bucket list is empty
//      int64 writeFee1KBBucketListLow;
//      // Fee per 1KB write when the bucket list has reached `bucketListTargetSizeBytes` 
//      int64 writeFee1KBBucketListHigh;
//      // Write fee multiplier for any additional data past the first `bucketListTargetSizeBytes`
//      uint32 bucketListWriteFeeGrowthFactor;
//  };

//  ===========================================================================
public class ConfigSettingContractLedgerCostV0
{
    public Uint32 LedgerMaxReadLedgerEntries { get; set; }
    public Uint32 LedgerMaxReadBytes { get; set; }
    public Uint32 LedgerMaxWriteLedgerEntries { get; set; }
    public Uint32 LedgerMaxWriteBytes { get; set; }
    public Uint32 TxMaxReadLedgerEntries { get; set; }
    public Uint32 TxMaxReadBytes { get; set; }
    public Uint32 TxMaxWriteLedgerEntries { get; set; }
    public Uint32 TxMaxWriteBytes { get; set; }
    public Int64 FeeReadLedgerEntry { get; set; }
    public Int64 FeeWriteLedgerEntry { get; set; }
    public Int64 FeeRead1KB { get; set; }
    public Int64 BucketListTargetSizeBytes { get; set; }
    public Int64 WriteFee1KBBucketListLow { get; set; }
    public Int64 WriteFee1KBBucketListHigh { get; set; }
    public Uint32 BucketListWriteFeeGrowthFactor { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        ConfigSettingContractLedgerCostV0 encodedConfigSettingContractLedgerCostV0)
    {
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.LedgerMaxReadLedgerEntries);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.LedgerMaxReadBytes);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.LedgerMaxWriteLedgerEntries);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.LedgerMaxWriteBytes);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.TxMaxReadLedgerEntries);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.TxMaxReadBytes);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.TxMaxWriteLedgerEntries);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.TxMaxWriteBytes);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.FeeReadLedgerEntry);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.FeeWriteLedgerEntry);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.FeeRead1KB);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.BucketListTargetSizeBytes);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.WriteFee1KBBucketListLow);
        Int64.Encode(stream, encodedConfigSettingContractLedgerCostV0.WriteFee1KBBucketListHigh);
        Uint32.Encode(stream, encodedConfigSettingContractLedgerCostV0.BucketListWriteFeeGrowthFactor);
    }

    public static ConfigSettingContractLedgerCostV0 Decode(XdrDataInputStream stream)
    {
        var decodedConfigSettingContractLedgerCostV0 = new ConfigSettingContractLedgerCostV0();
        decodedConfigSettingContractLedgerCostV0.LedgerMaxReadLedgerEntries = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.LedgerMaxReadBytes = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.LedgerMaxWriteLedgerEntries = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.LedgerMaxWriteBytes = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.TxMaxReadLedgerEntries = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.TxMaxReadBytes = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.TxMaxWriteLedgerEntries = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.TxMaxWriteBytes = Uint32.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.FeeReadLedgerEntry = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.FeeWriteLedgerEntry = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.FeeRead1KB = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.BucketListTargetSizeBytes = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.WriteFee1KBBucketListLow = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.WriteFee1KBBucketListHigh = Int64.Decode(stream);
        decodedConfigSettingContractLedgerCostV0.BucketListWriteFeeGrowthFactor = Uint32.Decode(stream);
        return decodedConfigSettingContractLedgerCostV0;
    }
}