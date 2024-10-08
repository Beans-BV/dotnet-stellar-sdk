// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ConfigUpgradeSet {
//      ConfigSettingEntry updatedEntry<>;
//  };

//  ===========================================================================
public class ConfigUpgradeSet
{
    public ConfigSettingEntry[] UpdatedEntry { get; set; }

    public static void Encode(XdrDataOutputStream stream, ConfigUpgradeSet encodedConfigUpgradeSet)
    {
        var updatedEntrysize = encodedConfigUpgradeSet.UpdatedEntry.Length;
        stream.WriteInt(updatedEntrysize);
        for (var i = 0; i < updatedEntrysize; i++)
        {
            ConfigSettingEntry.Encode(stream, encodedConfigUpgradeSet.UpdatedEntry[i]);
        }
    }

    public static ConfigUpgradeSet Decode(XdrDataInputStream stream)
    {
        var decodedConfigUpgradeSet = new ConfigUpgradeSet();
        var updatedEntrysize = stream.ReadInt();
        decodedConfigUpgradeSet.UpdatedEntry = new ConfigSettingEntry[updatedEntrysize];
        for (var i = 0; i < updatedEntrysize; i++)
        {
            decodedConfigUpgradeSet.UpdatedEntry[i] = ConfigSettingEntry.Decode(stream);
        }
        return decodedConfigUpgradeSet;
    }
}