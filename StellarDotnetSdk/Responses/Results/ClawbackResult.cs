using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClawbackResultCode.ClawbackResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Represents the result of a clawback operation.
/// </summary>
public class ClawbackResult : OperationResult
{
    /// <summary>
    ///     Creates the appropriate <see cref="ClawbackResult" /> subclass from the given XDR representation.
    /// </summary>
    /// <param name="result">The XDR clawback result.</param>
    /// <returns>A <see cref="ClawbackResult" /> instance representing the operation outcome.</returns>
    public static ClawbackResult FromXdr(Xdr.ClawbackResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CLAWBACK_MALFORMED => new ClawbackMalformed(),
            ResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED => new ClawbackNotClawbackEnabled(),
            ResultCodeEnum.CLAWBACK_NO_TRUST => new ClawbackNoTrust(),
            ResultCodeEnum.CLAWBACK_SUCCESS => new ClawbackSuccess(),
            ResultCodeEnum.CLAWBACK_UNDERFUNDED => new ClawbackUnderfunded(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ClawbackResult type."),
        };
    }
}

/// <summary>
///     Represents a successful clawback operation result.
/// </summary>
public class ClawbackSuccess : ClawbackResult
{
    /// <inheritdoc />
    public override bool IsSuccess => true;
}

/// <summary>
///     The input to the clawback is invalid.
/// </summary>
public class ClawbackMalformed : ClawbackResult;

/// <summary>
///     The trustline between From and the issuer account for this Asset does not have clawback enabled.
/// </summary>
public class ClawbackNotClawbackEnabled : ClawbackResult;

/// <summary>
///     The From account does not trust the issuer of the asset.
/// </summary>
public class ClawbackNoTrust : ClawbackResult;

/// <summary>
///     The From account does not have a sufficient available balance of the asset (after accounting for selling
///     liabilities).
/// </summary>
public class ClawbackUnderfunded : ClawbackResult;