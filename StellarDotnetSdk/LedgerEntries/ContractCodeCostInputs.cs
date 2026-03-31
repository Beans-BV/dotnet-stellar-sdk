using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the cost input metrics for a Soroban smart contract, including counts of WASM module elements used in
///     fee estimation.
/// </summary>
public class ContractCodeCostInputs
{
    private ContractCodeCostInputs(ExtensionPoint extensionPoint, uint nInstructions, uint nFunctions, uint nGlobals,
        uint nTableEntries, uint nTypes, uint nDataSegments, uint nElemSegments, uint nImports, uint nExports,
        uint nDataSegmentBytes)
    {
        ExtensionPoint = extensionPoint;
        NInstructions = nInstructions;
        NFunctions = nFunctions;
        NGlobals = nGlobals;
        NTableEntries = nTableEntries;
        NTypes = nTypes;
        NDataSegments = nDataSegments;
        NElemSegments = nElemSegments;
        NImports = nImports;
        NExports = nExports;
        NDataSegmentBytes = nDataSegmentBytes;
    }

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Number of WASM instructions in the contract code.
    /// </summary>
    public uint NInstructions { get; }

    /// <summary>
    ///     Number of functions defined in the WASM module.
    /// </summary>
    public uint NFunctions { get; }

    /// <summary>
    ///     Number of global variables in the WASM module.
    /// </summary>
    public uint NGlobals { get; }

    /// <summary>
    ///     Number of table entries in the WASM module.
    /// </summary>
    public uint NTableEntries { get; }

    /// <summary>
    ///     Number of type definitions in the WASM module.
    /// </summary>
    public uint NTypes { get; }

    /// <summary>
    ///     Number of data segments in the WASM module.
    /// </summary>
    public uint NDataSegments { get; }

    /// <summary>
    ///     Number of element segments in the WASM module.
    /// </summary>
    public uint NElemSegments { get; }

    /// <summary>
    ///     Number of imports in the WASM module.
    /// </summary>
    public uint NImports { get; }

    /// <summary>
    ///     Number of exports in the WASM module.
    /// </summary>
    public uint NExports { get; }

    /// <summary>
    ///     Total size in bytes of all data segments in the WASM module.
    /// </summary>
    public uint NDataSegmentBytes { get; }

    /// <summary>
    ///     Creates the corresponding <c>ContractCodeCostInputs</c> object from an <c>Xdr.ContractCodeCostInputs</c> object.
    /// </summary>
    /// <param name="xdrCostInputs">An <c>Xdr.ContractCodeCostInputs</c> object to be converted.</param>
    /// <returns>A <c>ContractCodeCostInputs</c> object. Returns null if the provided object is null.</returns>
    public static ContractCodeCostInputs FromXdr(Xdr.ContractCodeCostInputs xdrCostInputs)
    {
        return new ContractCodeCostInputs(
            ExtensionPoint.FromXdr(xdrCostInputs.Ext),
            xdrCostInputs.NInstructions.InnerValue,
            xdrCostInputs.NFunctions.InnerValue,
            xdrCostInputs.NGlobals.InnerValue,
            xdrCostInputs.NTableEntries.InnerValue,
            xdrCostInputs.NTypes.InnerValue,
            xdrCostInputs.NDataSegments.InnerValue,
            xdrCostInputs.NElemSegments.InnerValue,
            xdrCostInputs.NImports.InnerValue,
            xdrCostInputs.NExports.InnerValue,
            xdrCostInputs.NDataSegmentBytes.InnerValue);
    }
}