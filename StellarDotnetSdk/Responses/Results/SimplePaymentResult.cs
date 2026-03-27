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

    public KeyPair Destination { get; }
    public Asset Asset { get; }
    public string Amount { get; }

    public static SimplePaymentResult FromXdr(Xdr.SimplePaymentResult result)
    {
        return new SimplePaymentResult(
            KeyPair.FromXdrPublicKey(result.Destination.InnerValue),
            Asset.FromXdr(result.Asset),
            StellarDotnetSdk.Amount.FromXdr(result.Amount.InnerValue));
    }
}