using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class BumpSequenceResult : OperationResult
{
    public static BumpSequenceResult FromXdr(Xdr.BumpSequenceResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case BumpSequenceResultCode.BumpSequenceResultCodeEnum.BUMP_SEQUENCE_SUCCESS:
                return new BumpSequenceSuccess();
            case BumpSequenceResultCode.BumpSequenceResultCodeEnum.BUMP_SEQUENCE_BAD_SEQ:
                return new BumpSequenceBadSeq();
            default:
                throw new SystemException("Unknown BumpSequence type");
        }
    }
}