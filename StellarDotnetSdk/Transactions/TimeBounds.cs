using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Transactions;

/// <summary>
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/stellar-data-structures/operations-and-transactions#time-bounds">
///         Time
///         bounds
///     </a>
/// </summary>
public class TimeBounds
{
    private readonly ulong _maxTime;
    private readonly ulong _minTime;

    /// <summary>
    ///     Time bounds constructor.
    /// </summary>
    /// <param name="minTime"> 64bit Unix timestamp, 0 if unset</param>
    /// <param name="maxTime"> 64bit Unix timestamp, 0 if unset</param>
    public TimeBounds(ulong minTime, ulong maxTime)
    {
        if (maxTime != TransactionPreconditions.TimeoutInfinite && minTime >= maxTime)
        {
            throw new ArgumentException("minTime must be < maxTime");
        }

        _minTime = minTime;
        _maxTime = maxTime;
    }

    /// <summary>
    ///     Initializes time bounds from signed 64-bit Unix timestamps. Values must be non-negative,
    ///     and <paramref name="minTime" /> must be less than <paramref name="maxTime" /> unless
    ///     <paramref name="maxTime" /> is 0 (infinite).
    /// </summary>
    /// <param name="minTime">The minimum time as a Unix timestamp in seconds (must be &gt;= 0).</param>
    /// <param name="maxTime">The maximum time as a Unix timestamp in seconds (must be &gt;= 0; 0 means no upper bound).</param>
    /// <exception cref="ArgumentException">Thrown when time values are negative or minTime &gt;= maxTime.</exception>
    public TimeBounds(long minTime, long maxTime)
    {
        if (minTime < 0)
        {
            throw new ArgumentException("minTime must be >= 0");
        }
        if (maxTime < 0)
        {
            throw new ArgumentException("maxTime must be >= 0");
        }
        if (maxTime != 0 && minTime >= maxTime)
        {
            throw new ArgumentException("minTime must be < maxTime");
        }
        _minTime = (ulong)minTime;
        _maxTime = (ulong)maxTime;
    }

    /// <summary>
    ///     Time bounds constructor.
    /// </summary>
    /// <param name="minTime"> earliest time the transaction is valid from</param>
    /// <param name="maxTime"> latest time the transaction is valid to</param>
    public TimeBounds(DateTimeOffset? minTime = null, DateTimeOffset? maxTime = null)
    {
        if (minTime >= maxTime)
        {
            throw new ArgumentException("minTime must be < maxTime");
        }

        var minEpoch = minTime?.ToUnixTimeSeconds() ?? 0;
        var maxEpoch = maxTime?.ToUnixTimeSeconds() ?? 0;

        _minTime = (ulong)minEpoch;
        _maxTime = (ulong)maxEpoch;
    }

    /// <summary>
    ///     Time bounds constructor.
    /// </summary>
    /// <param name="minTime">earliest time the transaction is valid from</param>
    /// <param name="duration">duration window the transaction is valid for</param>
    public TimeBounds(DateTimeOffset minTime, TimeSpan duration) : this(minTime, minTime.Add(duration))
    {
    }

    /// <summary>
    ///     Time bounds constructor.
    /// </summary>
    /// <param name="duration">duration window the transaction is valid for from now</param>
    public TimeBounds(TimeSpan duration) : this(DateTimeOffset.UtcNow, duration)
    {
    }

    /// <summary>Gets the minimum time (earliest valid time) as a Unix timestamp in seconds. 0 if unset.</summary>
    public long MinTime => (long)_minTime;

    /// <summary>Gets the maximum time (latest valid time) as a Unix timestamp in seconds. 0 if unset (infinite).</summary>
    public long MaxTime => (long)_maxTime;

    /// <summary>
    ///     Creates a <see cref="TimeBounds" /> from its XDR representation.
    /// </summary>
    /// <param name="timeBounds">The XDR time bounds object.</param>
    /// <returns>A new <see cref="TimeBounds" /> instance.</returns>
    public static TimeBounds FromXdr(Xdr.TimeBounds timeBounds)
    {
        return new TimeBounds(
            timeBounds.MinTime.InnerValue.InnerValue,
            timeBounds.MaxTime.InnerValue.InnerValue
        );
    }

    /// <summary>
    ///     Converts this instance to its XDR <see cref="Xdr.TimeBounds" /> representation.
    /// </summary>
    /// <returns>An XDR time bounds object.</returns>
    public Xdr.TimeBounds ToXdr()
    {
        return new Xdr.TimeBounds
        {
            MinTime = new TimePoint(new Uint64(_minTime)),
            MaxTime = new TimePoint(new Uint64(_maxTime)),
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? o)
    {
        if (this == o)
        {
            return true;
        }
        if (o == null || GetType() != o.GetType())
        {
            return false;
        }

        var that = (TimeBounds)o;

        if (MinTime != that.MinTime)
        {
            return false;
        }
        return MaxTime == that.MaxTime;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Start.Hash(MinTime).Hash(MaxTime);
    }
}