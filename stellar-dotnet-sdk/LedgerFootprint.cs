using System;
using System.Linq;

namespace stellar_dotnet_sdk;

public class LedgerFootprint
{
    public LedgerKey[] ReadOnly { get; init; } = Array.Empty<LedgerKey>();
    public LedgerKey[] ReadWrite { get; init; } = Array.Empty<LedgerKey>();

    public xdr.LedgerFootprint ToXdr()
    {
        return new xdr.LedgerFootprint
        {
            ReadOnly = ReadOnly.Select(x => x.ToXdr()).ToArray(),
            ReadWrite = ReadWrite.Select(x => x.ToXdr()).ToArray()
        };
    }

    public static LedgerFootprint FromXdr(xdr.LedgerFootprint xdr)
    {
        return new LedgerFootprint
        {
            ReadOnly = xdr.ReadOnly.Select(LedgerKey.FromXdr).ToArray(),
            ReadWrite = xdr.ReadWrite.Select(LedgerKey.FromXdr).ToArray()
        };
    }
}