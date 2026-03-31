using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Validation error thrown when the challenge transaction has invalid time bounds.
/// </summary>
public class ChallengeValidationErrorInvalidTimeBounds : ChallengeValidationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChallengeValidationErrorInvalidTimeBounds" /> class.
    /// </summary>
    /// <param name="minTime">The minimum time (start) of the challenge transaction's time bounds.</param>
    /// <param name="maxTime">The maximum time (end) of the challenge transaction's time bounds.</param>
    /// <param name="currentTime">The current time at which validation was performed.</param>
    /// <param name="message">The error message describing the time bounds validation failure.</param>
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

    /// <summary>
    ///     The minimum time (start) of the challenge transaction's time bounds.
    /// </summary>
    public DateTimeOffset MinTime { get; }

    /// <summary>
    ///     The maximum time (end) of the challenge transaction's time bounds.
    /// </summary>
    public DateTimeOffset MaxTime { get; }

    /// <summary>
    ///     The current time at which the validation was performed.
    /// </summary>
    public DateTimeOffset CurrentTime { get; }
}