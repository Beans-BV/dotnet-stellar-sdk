using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses.results;

public class BumpSequenceResult : OperationResult
{
    public static BumpSequenceResult FromXdr(xdr.BumpSequenceResult result)
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