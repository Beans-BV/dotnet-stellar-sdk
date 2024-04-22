namespace stellar_dotnet_sdk;

/// <summary>
///     Operation threshold level.
/// </summary>
public enum OperationThreshold
{
    /// <summary>
    ///     Low security level.
    ///     <seealso cref="AllowTrustOperation" />
    ///     <seealso cref="BumpSequenceOperation" />
    /// </summary>
    LOW = 1,

    /// <summary>
    ///     Medium security level.
    ///     <seealso cref="ChangeTrustOperation" />
    ///     <seealso cref="CreateAccountOperation" />
    ///     <seealso cref="CreatePassiveSellOfferOperation" />
    ///     <seealso cref="ManageDataOperation" />
    ///     <seealso cref="ManageSellOfferOperation" />
    ///     <seealso cref="PathPaymentStrictSendOperation" />
    ///     <seealso cref="PathPaymentStrictReceiveOperation" />
    ///     <seealso cref="PaymentOperation" />
    /// </summary>
    MEDIUM = 2,

    /// <summary>
    ///     High security level.
    ///     <seealso cref="SetOptionsOperation" />
    ///     <seealso cref="AccountMergeOperation" />
    /// </summary>
    HIGH = 3
}