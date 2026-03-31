using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.InvokeHostFunctionResultCode.InvokeHostFunctionResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of an invoke host function operation.
/// </summary>
public class InvokeHostFunctionResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="InvokeHostFunctionResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR invoke host function result.</param>
    /// <returns>An <see cref="InvokeHostFunctionResult" /> instance representing the operation outcome.</returns>
    public static InvokeHostFunctionResult FromXdr(Xdr.InvokeHostFunctionResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.INVOKE_HOST_FUNCTION_SUCCESS
                => new InvokeHostFunctionSuccess(),
            ResultCodeEnum.INVOKE_HOST_FUNCTION_MALFORMED
                => new InvokeHostFunctionMalformed(),
            ResultCodeEnum.INVOKE_HOST_FUNCTION_TRAPPED
                => new InvokeHostFunctionTrapped(),
            ResultCodeEnum.INVOKE_HOST_FUNCTION_RESOURCE_LIMIT_EXCEEDED
                => new InvokeHostFunctionResourceLimitExceeded(),
            ResultCodeEnum.INVOKE_HOST_FUNCTION_ENTRY_ARCHIVED
                => new InvokeHostFunctionEntryArchived(),
            ResultCodeEnum.INVOKE_HOST_FUNCTION_INSUFFICIENT_REFUNDABLE_FEE
                => new InvokeHostFunctionInsufficientRefundableFee(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown InvokeHostFunctionResult type."),
        };
    }
}

/// <summary>
///     Represents a successful invoke host function operation result.
/// </summary>
public class InvokeHostFunctionSuccess : InvokeHostFunctionResult
{
    /// <inheritdoc />
    public override bool IsSuccess => true;
}

/// <summary>
///     One or more of the inputs to the operation was malformed.
/// </summary>
public class InvokeHostFunctionMalformed : InvokeHostFunctionResult;

/// <summary>
///     The function invocation trapped in the Soroban runtime.
/// </summary>
public class InvokeHostFunctionTrapped : InvokeHostFunctionResult;

/// <summary>
///     The function invocation could not complete within the currently configured resource constraints of the network.
/// </summary>
public class InvokeHostFunctionResourceLimitExceeded : InvokeHostFunctionResult;

/// <summary>
///     A ledger entry required for this function's footprint is in an archived state, and must be restored.
/// </summary>
public class InvokeHostFunctionEntryArchived : InvokeHostFunctionResult;

/// <summary>
///     The refundable Soroban fee provided was not sufficient to pay for the compute resources required by this function
///     invocation.
/// </summary>
public class InvokeHostFunctionInsufficientRefundableFee : InvokeHostFunctionResult;