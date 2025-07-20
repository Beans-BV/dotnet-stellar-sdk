using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

public class SorobanResources
{
    public SorobanResources(LedgerFootprint footprint, uint instructions, uint diskReadBytes, uint writeBytes)
    {
        Footprint = footprint;
        Instructions = instructions;
        DiskReadBytes = diskReadBytes;
        WriteBytes = writeBytes;
    }

    public LedgerFootprint Footprint { get; }
    public uint Instructions { get; }
    public uint DiskReadBytes { get; }
    public uint WriteBytes { get; }

    public Xdr.SorobanResources ToXdr()
    {
        return new Xdr.SorobanResources
        {
            Footprint = Footprint.ToXdr(),
            Instructions = new Uint32(Instructions),
            DiskReadBytes = new Uint32(DiskReadBytes),
            WriteBytes = new Uint32(WriteBytes),
        };
    }

    public static SorobanResources FromXdr(Xdr.SorobanResources xdr)
    {
        return new SorobanResources(LedgerFootprint.FromXdr(xdr.Footprint), xdr.Instructions.InnerValue,
            xdr.DiskReadBytes.InnerValue, xdr.WriteBytes.InnerValue);
    }
}