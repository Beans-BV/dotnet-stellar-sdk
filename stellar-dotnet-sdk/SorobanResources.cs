using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class SorobanResources
{
    public LedgerFootprint Footprint { get; set; } = new();
    public uint Instructions { get; set; }
    public uint ReadBytes { get; set; }
    public uint WriteBytes { get; set; }

    public xdr.SorobanResources ToXdr()
    {
        return new xdr.SorobanResources
        {
            Footprint = Footprint.ToXdr(),
            Instructions = new Uint32(Instructions),
            ReadBytes = new Uint32(ReadBytes),
            WriteBytes = new Uint32(WriteBytes)
        };
    }

    public static SorobanResources FromXdr(xdr.SorobanResources xdr)
    {
        return new SorobanResources
        {
            Footprint = LedgerFootprint.FromXdr(xdr.Footprint),
            Instructions = xdr.Instructions.InnerValue,
            ReadBytes = xdr.ReadBytes.InnerValue,
            WriteBytes = xdr.WriteBytes.InnerValue
        };
    }
}