using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has invalid time bounds.
/// </summary>
public class ChallengeValidationErrorInvalidTimeBounds : ChallengeValidationException
{
    public ChallengeValidationErrorInvalidTimeBounds(
        DateTimeOffset minTime,
        DateTimeOffset maxTime,
        DateTimeOffset currentTime,
        string message)
        : base(message + $"\nMinTime: {minTime}, MaxTime: {maxTime}, CurrentTime: {currentTime}")
    {
        MinTime = minTime;
        MaxTime = maxTime;
        CurrentTime = currentTime;
    }

    public DateTimeOffset MinTime { get; }
    public DateTimeOffset MaxTime { get; }
    public DateTimeOffset CurrentTime { get; }
}
