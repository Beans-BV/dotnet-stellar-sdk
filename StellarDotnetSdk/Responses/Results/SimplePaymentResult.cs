using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a simple payment, containing the destination, asset, and amount.
/// </summary>
public class SimplePaymentResult
{
    private SimplePaymentResult(KeyPair destination, Asset asset, string amount)
    {
        Destination = destination;
        Asset = asset;
        Amount = amount;
    }

    /// <summary>
    ///     The destination account that received the payment.
    /// </summary>
    public KeyPair Destination { get; }

    /// <summary>
    ///     The asset that was sent.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     The amount of the asset that was sent.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     Creates a new <see cref="SimplePaymentResult" /> from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR simple payment result.</param>
    /// <returns>A new <see cref="SimplePaymentResult" /> instance.</returns>
    public static SimplePaymentResult FromXdr(Xdr.SimplePaymentResult result)
    {
        return new SimplePaymentResult(
            KeyPair.FromXdrPublicKey(result.Destination.InnerValue),
            Asset.FromXdr(result.Asset),
            StellarDotnetSdk.Amount.FromXdr(result.Amount.InnerValue));
    }
}