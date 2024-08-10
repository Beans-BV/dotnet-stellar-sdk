using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ManageDataResultCode.ManageDataResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ManageDataResult : OperationResult
{
    public static ManageDataResult FromXdr(Xdr.ManageDataResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.MANAGE_DATA_SUCCESS => new ManageDataSuccess(),
            ResultCodeEnum.MANAGE_DATA_NOT_SUPPORTED_YET => new ManageDataNotSupportedYet(),
            ResultCodeEnum.MANAGE_DATA_NAME_NOT_FOUND => new ManageDataNameNotFound(),
            ResultCodeEnum.MANAGE_DATA_LOW_RESERVE => new ManageDataLowReserve(),
            ResultCodeEnum.MANAGE_DATA_INVALID_NAME => new ManageDataInvalidName(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ManageDataResult type."),
        };
    }
}

public class ManageDataSuccess : ManageDataResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The network hasn't moved to this protocol change yet. This failure means the network doesn't support this feature
///     yet.
/// </summary>
public class ManageDataNotSupportedYet : ManageDataResult;

/// <summary>
///     Trying to remove a Data Entry that isn't there. This will happen if Name is set (and Value isn't) but the Account
///     doesn't have a DataEntry with that Name.
/// </summary>
public class ManageDataNameNotFound : ManageDataResult;

/// <summary>
///     This account does not have enough XLM to satisfy the minimum XLM reserve increase caused by adding a subentry and
///     still satisfy its XLM selling liabilities. For every new DataEntry added to an account, the minimum reserve of XLM
///     that account must hold increases.
/// </summary>
public class ManageDataLowReserve : ManageDataResult;

/// <summary>
///     Name not a valid string.
/// </summary>
public class ManageDataInvalidName : ManageDataResult;