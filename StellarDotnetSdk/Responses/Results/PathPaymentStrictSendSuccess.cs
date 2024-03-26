using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Operation successful.
/// </summary>
public class PathPaymentStrictSendSuccess : PathPaymentStrictSendResult
{
    public override bool IsSuccess => true;

    /// <summary>
    ///     Offers claimed in this payment.
    /// </summary>
    public ClaimAtom[] Offers { get; set; }

    /// <summary>
    ///     Payment result.
    /// </summary>
    public SimplePaymentResult Last { get; set; }

    public class SimplePaymentResult
    {
        public KeyPair Destination { get; set; }
        public Asset Asset { get; set; }
        public string Amount { get; set; }

        public static SimplePaymentResult FromXdr(Xdr.SimplePaymentResult result)
        {
            return new SimplePaymentResult
            {
                Destination = KeyPair.FromXdrPublicKey(result.Destination.InnerValue),
                Asset = Asset.FromXdr(result.Asset),
                Amount = StellarDotnetSdk.Amount.FromXdr(result.Amount.InnerValue)
            };
        }
    }
}