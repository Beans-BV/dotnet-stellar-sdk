using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

public class SorobanResources
{
    public SorobanResources(LedgerFootprint footprint, uint instructions, uint readBytes, uint writeBytes)
    {
        Footprint = footprint;
        Instructions = instructions;
        ReadBytes = readBytes;
        WriteBytes = writeBytes;
    }

    public LedgerFootprint Footprint { get; }
    public uint Instructions { get; }
    public uint ReadBytes { get; }
    public uint WriteBytes { get; }

    public Xdr.SorobanResources ToXdr()
    {
        return new Xdr.SorobanResources
        {
            Footprint = Footprint.ToXdr(),
            Instructions = new Uint32(Instructions),
            ReadBytes = new Uint32(ReadBytes),
            WriteBytes = new Uint32(WriteBytes)
        };
    }

    public static SorobanResources FromXdr(Xdr.SorobanResources xdr)
    {
        return new SorobanResources(LedgerFootprint.FromXdr(xdr.Footprint), xdr.Instructions.InnerValue,
            xdr.ReadBytes.InnerValue, xdr.WriteBytes.InnerValue);
    }
}