using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Operation successful.
/// </summary>
public class InflationSuccess : InflationResult
{
    public override bool IsSuccess => true;

    /// <summary>
    ///     Inflation Payouts.
    /// </summary>
    public InflationPayout[] Payouts { get; set; }

    public class InflationPayout
    {
        /// <summary>
        ///     Account receiving inflation.
        /// </summary>
        public KeyPair Destination { get; set; }

        /// <summary>
        ///     Amount set to account.
        /// </summary>
        public string Amount { get; set; }

        public static InflationPayout FromXdr(Xdr.InflationPayout payout)
        {
            return new InflationPayout
            {
                Amount = StellarDotnetSdk.Amount.FromXdr(payout.Amount.InnerValue),
                Destination = KeyPair.FromXdrPublicKey(payout.Destination.InnerValue)
            };
        }
    }
}