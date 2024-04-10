using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.BumpSequenceResultCode.BumpSequenceResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class BumpSequenceResult : OperationResult
{
    public static BumpSequenceResult FromXdr(Xdr.BumpSequenceResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.BUMP_SEQUENCE_SUCCESS => new BumpSequenceSuccess(),
            ResultCodeEnum.BUMP_SEQUENCE_BAD_SEQ => new BumpSequenceBadSeq(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown BumpSequenceResult type.")
        };
    }
}

public class BumpSequenceSuccess : BumpSequenceResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The specified bumpTo sequence number is not a valid sequence number. It must be between 0 and INT64_MAX.
/// </summary>
public class BumpSequenceBadSeq : BumpSequenceResult;