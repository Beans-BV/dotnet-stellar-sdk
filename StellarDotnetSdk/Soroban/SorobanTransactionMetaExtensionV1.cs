using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents version 1 extension metadata for a Soroban transaction, containing fee breakdowns
///     for non-refundable resources, refundable resources, and rent charges.
/// </summary>
public class SorobanTransactionMetaExtensionV1
{
    private SorobanTransactionMetaExtensionV1(
        ExtensionPoint extensionPoint,
        long totalNonRefundableResourceFeeCharged,
        long totalRefundableResourceFeeCharged,
        long rentFeeCharged)
    {
        ExtensionPoint = extensionPoint;
        TotalNonRefundableResourceFeeCharged = totalNonRefundableResourceFeeCharged;
        TotalRefundableResourceFeeCharged = totalRefundableResourceFeeCharged;
        RentFeeCharged = rentFeeCharged;
    }

    /// <summary>
    ///     The total non-refundable resource fee in stroops charged for the transaction.
    /// </summary>
    public long TotalNonRefundableResourceFeeCharged { get; }

    /// <summary>
    ///     The total refundable resource fee in stroops charged for the transaction.
    /// </summary>
    public long TotalRefundableResourceFeeCharged { get; }

    /// <summary>
    ///     The rent fee in stroops charged for the transaction.
    /// </summary>
    public long RentFeeCharged { get; }

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Creates a new <see cref="SorobanTransactionMetaExtensionV1" /> from an XDR
    ///     <see cref="SorobanTransactionMetaExtV1" /> object.
    /// </summary>
    /// <param name="xdrMetaExtV1">The XDR metadata extension to convert.</param>
    /// <returns>A <see cref="SorobanTransactionMetaExtensionV1" /> instance.</returns>
    public static SorobanTransactionMetaExtensionV1 FromXdr(SorobanTransactionMetaExtV1 xdrMetaExtV1)
    {
        return new SorobanTransactionMetaExtensionV1(
            ExtensionPoint.FromXdr(xdrMetaExtV1.Ext),
            xdrMetaExtV1.TotalNonRefundableResourceFeeCharged.InnerValue,
            xdrMetaExtV1.TotalRefundableResourceFeeCharged.InnerValue,
            xdrMetaExtV1.RentFeeCharged.InnerValue
        );
    }
}