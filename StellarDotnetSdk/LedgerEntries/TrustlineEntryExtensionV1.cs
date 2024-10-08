﻿using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class TrustlineEntryExtensionV1
{
    private TrustlineEntryExtensionV1(Liabilities liabilities)
    {
        Liabilities = liabilities;
    }

    public Liabilities Liabilities { get; }

    public TrustLineEntryExtensionV2? TrustlineExtensionV2 { get; private set; }

    public static TrustlineEntryExtensionV1 FromXdr(TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new TrustlineEntryExtensionV1(Liabilities.FromXdr(xdrExtensionV1.Liabilities));
        if (xdrExtensionV1.Ext.Discriminant == 2)
        {
            entryExtensionV1.TrustlineExtensionV2 = TrustLineEntryExtensionV2.FromXdr(xdrExtensionV1.Ext.V2);
        }

        return entryExtensionV1;
    }
}