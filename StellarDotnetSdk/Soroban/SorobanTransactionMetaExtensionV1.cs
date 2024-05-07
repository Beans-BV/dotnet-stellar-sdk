using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

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

    public long TotalNonRefundableResourceFeeCharged { get; }
    public long TotalRefundableResourceFeeCharged { get; }
    public long RentFeeCharged { get; }
    public ExtensionPoint ExtensionPoint { get; }

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