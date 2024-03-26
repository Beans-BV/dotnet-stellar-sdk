using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Transactions;

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
            throw new ArgumentException("minTime must be < maxTime");

        _minTime = minTime;
        _maxTime = maxTime;
    }

    public TimeBounds(long minTime, long maxTime)
    {
        if (minTime < 0)
            throw new ArgumentException("minTime must be >= 0");
        if (maxTime < 0)
            throw new ArgumentException("maxTime must be >= 0");
        if (maxTime != 0 && minTime >= maxTime)
            throw new ArgumentException("minTime must be < maxTime");
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
            throw new ArgumentException("minTime must be < maxTime");

        var minEpoch = minTime?.ToUnixTimeSeconds() ?? 0;
        var maxEpoch = maxTime?.ToUnixTimeSeconds() ?? 0;

        _minTime = (ulong)minEpoch;
        _maxTime = (ulong)maxEpoch;
    }

    /// <summary>
    ///     Timebounds constructor.
    /// </summary>
    /// <param name="minTime">earliest time the transaction is valid from</param>
    /// <param name="duration">duration window the transaction is valid for</param>
    public TimeBounds(DateTimeOffset minTime, TimeSpan duration) : this(minTime, minTime.Add(duration))
    {
    }

    /// <summary>
    ///     Timebounds constructor.
    /// </summary>
    /// <param name="duration">duration window the transaction is valid for from now</param>
    public TimeBounds(TimeSpan duration) : this(DateTimeOffset.UtcNow, duration)
    {
    }

    public long MinTime => (long)_minTime;
    public long MaxTime => (long)_maxTime;

    public static TimeBounds FromXdr(Xdr.TimeBounds timeBounds)
    {
        if (timeBounds == null) return null;

        return new TimeBounds(
            timeBounds.MinTime.InnerValue.InnerValue,
            timeBounds.MaxTime.InnerValue.InnerValue
        );
    }

    public Xdr.TimeBounds ToXdr()
    {
        var timeBounds = new Xdr.TimeBounds();
        var minTime = new Uint64();
        var maxTime = new Uint64();
        minTime.InnerValue = _minTime;
        maxTime.InnerValue = _maxTime;
        timeBounds.MinTime = new TimePoint(minTime);
        timeBounds.MaxTime = new TimePoint(maxTime);
        return timeBounds;
    }

    public override bool Equals(object? o)
    {
        if (this == o) return true;
        if (o == null || GetType() != o.GetType()) return false;

        var that = (TimeBounds)o;

        if (MinTime != that.MinTime) return false;
        return MaxTime == that.MaxTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Start
            .Hash(MinTime)
            .Hash(MaxTime);
    }
}