using System;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.LedgerEntries;

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

    public ExtensionPoint ExtensionPoint { get; }
    public uint NInstructions { get; }
    public uint NFunctions { get; }
    public uint NGlobals { get; }
    public uint NTableEntries { get; }
    public uint NTypes { get; }
    public uint NDataSegments { get; }
    public uint NElemSegments { get; }
    public uint NImports { get; }
    public uint NExports { get; }
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