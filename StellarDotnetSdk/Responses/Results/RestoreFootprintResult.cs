using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.RestoreFootprintResultCode.RestoreFootprintResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class RestoreFootprintResult : OperationResult
{
    public static RestoreFootprintResult FromXdr(Xdr.RestoreFootprintResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.RESTORE_FOOTPRINT_SUCCESS
                => new RestoreFootprintSuccess(),
            ResultCodeEnum.RESTORE_FOOTPRINT_MALFORMED
                => new RestoreFootprintMalformed(),
            ResultCodeEnum.RESTORE_FOOTPRINT_RESOURCE_LIMIT_EXCEEDED
                => new RestoreFootprintResourceLimitExceeded(),
            ResultCodeEnum.RESTORE_FOOTPRINT_INSUFFICIENT_REFUNDABLE_FEE
                => new RestoreFootprintInsufficientRefundableFee(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown RestoreFootprintResult type."),
        };
    }
}

public class RestoreFootprintSuccess : RestoreFootprintResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class RestoreFootprintMalformed : RestoreFootprintResult;

/// <summary>
///     The archive restoration could not be completed within the currently configured resource constraints of the network.
/// </summary>
public class RestoreFootprintResourceLimitExceeded : RestoreFootprintResult;

/// <summary>
///     The refundable Soroban fee provided was not sufficient to pay for archive restoration of the specified ledger
///     entries.
/// </summary>
public class RestoreFootprintInsufficientRefundableFee : RestoreFootprintResult;