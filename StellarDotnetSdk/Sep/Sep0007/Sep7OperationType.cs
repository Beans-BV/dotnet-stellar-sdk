namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>The operations a SEP-7 URI can request.</summary>
public enum Sep7OperationType
{
    /// <summary>
    ///     <c>web+stellar:tx</c> — asks the wallet to sign (and submit) a given transaction envelope.
    /// </summary>
    Tx,

    /// <summary>
    ///     <c>web+stellar:pay</c> — asks the wallet to pay a destination, leaving the transaction details (such as
    ///     the source account and path) to the wallet.
    /// </summary>
    Pay,
}
