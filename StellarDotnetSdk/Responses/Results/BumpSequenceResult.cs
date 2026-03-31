using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.BumpSequenceResultCode.BumpSequenceResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a bump sequence operation.
/// </summary>
public class BumpSequenceResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="BumpSequenceResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR bump sequence result.</param>
    /// <returns>A <see cref="BumpSequenceResult" /> instance representing the operation outcome.</returns>
    public static BumpSequenceResult FromXdr(Xdr.BumpSequenceResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.BUMP_SEQUENCE_SUCCESS => new BumpSequenceSuccess(),
            ResultCodeEnum.BUMP_SEQUENCE_BAD_SEQ => new BumpSequenceBadSeq(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown BumpSequenceResult type."),
        };
    }
}

/// <summary>
///     Represents a successful bump sequence operation result.
/// </summary>
public class BumpSequenceSuccess : BumpSequenceResult
{
    /// <inheritdoc />
    public override bool IsSuccess => true;
}

/// <summary>
///     The specified bumpTo sequence number is not a valid sequence number. It must be between 0 and INT64_MAX.
/// </summary>
public class BumpSequenceBadSeq : BumpSequenceResult;