using System;
using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of an inflation operation.
/// </summary>
public class InflationResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="InflationResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR inflation result.</param>
    /// <returns>An <see cref="InflationResult" /> instance representing the operation outcome.</returns>
    public static InflationResult FromXdr(Xdr.InflationResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            InflationResultCode.InflationResultCodeEnum.INFLATION_SUCCESS
                => new InflationSuccess(result.Payouts.Select(InflationSuccess.InflationPayout.FromXdr).ToArray()),
            InflationResultCode.InflationResultCodeEnum.INFLATION_NOT_TIME
                => new InflationNotTime(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown InflationResult type."),
        };
    }
}

/// <summary>
///     Represents a successful inflation operation result.
/// </summary>
public class InflationSuccess : InflationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InflationSuccess" /> class.
    /// </summary>
    /// <param name="payouts">The array of inflation payouts distributed to destination accounts.</param>
    public InflationSuccess(InflationPayout[] payouts)
    {
        Payouts = payouts;
    }

    /// <inheritdoc />
    public override bool IsSuccess => true;

    /// <summary>
    ///     Inflation Payouts.
    /// </summary>
    public InflationPayout[] Payouts { get; }

    /// <summary>
    ///     Represents a single inflation payout to a destination account.
    /// </summary>
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

        /// <summary>
        ///     Creates a new <see cref="InflationPayout" /> from the given XDR representation.
        /// </summary>
        /// <param name="payout">The XDR inflation payout.</param>
        /// <returns>A new <see cref="InflationPayout" /> instance.</returns>
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