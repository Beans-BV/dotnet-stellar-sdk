using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents the resource limits for a Soroban transaction, including the ledger footprint,
///     CPU instructions, and I/O byte limits.
/// </summary>
public class SorobanResources
{
    /// <summary>
    ///     Initializes a new <see cref="SorobanResources" /> with the specified resource limits.
    /// </summary>
    /// <param name="footprint">The ledger footprint declaring read and read-write keys.</param>
    /// <param name="instructions">The maximum number of CPU instructions allowed.</param>
    /// <param name="diskReadBytes">The maximum number of bytes that can be read from disk.</param>
    /// <param name="writeBytes">The maximum number of bytes that can be written.</param>
    public SorobanResources(LedgerFootprint footprint, uint instructions, uint diskReadBytes, uint writeBytes)
    {
        Footprint = footprint;
        Instructions = instructions;
        DiskReadBytes = diskReadBytes;
        WriteBytes = writeBytes;
    }

    /// <summary>
    ///     The ledger footprint declaring which ledger entries will be read and/or written.
    /// </summary>
    public LedgerFootprint Footprint { get; }

    /// <summary>
    ///     The maximum number of CPU instructions the transaction is allowed to consume.
    /// </summary>
    public uint Instructions { get; }

    /// <summary>
    ///     The maximum number of bytes the transaction is allowed to read from disk.
    /// </summary>
    public uint DiskReadBytes { get; }

    /// <summary>
    ///     The maximum number of bytes the transaction is allowed to write.
    /// </summary>
    public uint WriteBytes { get; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanResources" /> XDR object.</returns>
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

    /// <summary>
    ///     Creates a new <see cref="SorobanResources" /> from an XDR <see cref="Xdr.SorobanResources" /> object.
    /// </summary>
    /// <param name="xdr">The XDR Soroban resources to convert.</param>
    /// <returns>A <see cref="SorobanResources" /> instance.</returns>
    public static SorobanResources FromXdr(Xdr.SorobanResources xdr)
    {
        return new SorobanResources(LedgerFootprint.FromXdr(xdr.Footprint), xdr.Instructions.InnerValue,
            xdr.DiskReadBytes.InnerValue, xdr.WriteBytes.InnerValue);
    }
}