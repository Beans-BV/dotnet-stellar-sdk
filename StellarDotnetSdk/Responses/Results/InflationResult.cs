using System;
using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class InflationResult : OperationResult
{
    public static InflationResult FromXdr(Xdr.InflationResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            InflationResultCode.InflationResultCodeEnum.INFLATION_SUCCESS
                => new InflationSuccess(result.Payouts.Select(InflationSuccess.InflationPayout.FromXdr).ToArray()),
            InflationResultCode.InflationResultCodeEnum.INFLATION_NOT_TIME
                => new InflationNotTime(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown InflationResult type.")
        };
    }
}

public class InflationSuccess : InflationResult
{
    public InflationSuccess(InflationPayout[] payouts)
    {
        Payouts = payouts;
    }

    public override bool IsSuccess => true;

    /// <summary>
    ///     Inflation Payouts.
    /// </summary>
    public InflationPayout[] Payouts { get; }

    public class InflationPayout
    {
        private InflationPayout(KeyPair destination, string amount)
        {
            Destination = destination;
            Amount = amount;
        }

        /// <summary>
        ///     Account receiving inflation.
        /// </summary>
        public KeyPair Destination { get; }

        /// <summary>
        ///     Amount set to account.
        /// </summary>
        public string Amount { get; }

        public static InflationPayout FromXdr(Xdr.InflationPayout payout)
        {
            return new InflationPayout(
                KeyPair.FromXdrPublicKey(payout.Destination.InnerValue),
                StellarDotnetSdk.Amount.FromXdr(payout.Amount.InnerValue));
        }
    }
}

/// <summary>
///     Not time for inflation.
/// </summary>
public class InflationNotTime : InflationResult;