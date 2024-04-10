using System;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ExtendFootprintTTLResultCode.ExtendFootprintTTLResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ExtendFootprintTtlResult : OperationResult
{
    public static ExtendFootprintTtlResult FromXdr(ExtendFootprintTTLResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.EXTEND_FOOTPRINT_TTL_SUCCESS
                => new ExtendFootprintTtlSuccess(),
            ResultCodeEnum.EXTEND_FOOTPRINT_TTL_MALFORMED
                => new ExtendFootprintTtlMalformed(),
            ResultCodeEnum.EXTEND_FOOTPRINT_TTL_RESOURCE_LIMIT_EXCEEDED
                => new ExtendFootprintTtlResourceLimitExceeded(),
            ResultCodeEnum.EXTEND_FOOTPRINT_TTL_INSUFFICIENT_REFUNDABLE_FEE
                => new ExtendFootprintTtlInsufficientRefundableFee(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ExtendFootprintTtlResult type.")
        };
    }
}

public class ExtendFootprintTtlSuccess : ExtendFootprintTtlResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class ExtendFootprintTtlMalformed : ExtendFootprintTtlResult;

/// <summary>
///     The TTL extension could not be completed within the currently configured resource constraints of the network.
/// </summary>
public class ExtendFootprintTtlResourceLimitExceeded : ExtendFootprintTtlResult;

/// <summary>
///     The refundable Soroban fee provided was not sufficient to pay for TTL extension of the specified ledger entries.
/// </summary>
public class ExtendFootprintTtlInsufficientRefundableFee : ExtendFootprintTtlResult;