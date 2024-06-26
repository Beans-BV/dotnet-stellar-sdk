// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ConfigSettingContractEventsV0
//  {
//      // Maximum size of events that a contract call can emit.
//      uint32 txMaxContractEventsSizeBytes;
//      // Fee for generating 1KB of contract events.
//      int64 feeContractEvents1KB;
//  };

//  ===========================================================================
public class ConfigSettingContractEventsV0
{
    public Uint32 TxMaxContractEventsSizeBytes { get; set; }
    public Int64 FeeContractEvents1KB { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        ConfigSettingContractEventsV0 encodedConfigSettingContractEventsV0)
    {
        Uint32.Encode(stream, encodedConfigSettingContractEventsV0.TxMaxContractEventsSizeBytes);
        Int64.Encode(stream, encodedConfigSettingContractEventsV0.FeeContractEvents1KB);
    }

    public static ConfigSettingContractEventsV0 Decode(XdrDataInputStream stream)
    {
        var decodedConfigSettingContractEventsV0 = new ConfigSettingContractEventsV0();
        decodedConfigSettingContractEventsV0.TxMaxContractEventsSizeBytes = Uint32.Decode(stream);
        decodedConfigSettingContractEventsV0.FeeContractEvents1KB = Int64.Decode(stream);
        return decodedConfigSettingContractEventsV0;
    }
}