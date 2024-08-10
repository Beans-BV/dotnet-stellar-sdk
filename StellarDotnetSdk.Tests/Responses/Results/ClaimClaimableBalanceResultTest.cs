using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Xdr;
using ClaimClaimableBalanceResult = StellarDotnetSdk.Xdr.ClaimClaimableBalanceResult;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ClaimClaimableBalanceResultCode.ClaimClaimableBalanceResultCodeEnum;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ClaimClaimableBalanceResultTest
{
    [TestMethod]
    public void TestCannotClaim()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_CANNOT_CLAIM);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceCannotClaim), false);
    }

    [TestMethod]
    public void TestDoesNotExist()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_DOES_NOT_EXIST);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceDoesNotExist), false);
    }

    [TestMethod]
    public void TestLineFull()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_LINE_FULL);

        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceLineFull), false);
    }

    [TestMethod]
    public void TestNotAuthorized()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NOT_AUTHORIZED);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceNotAuthorized), false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_NO_TRUST);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceNoTrust), false);
    }


    [TestMethod]
    public void TestSuccess()
    {
        var operationResultTr = CreateOperationResultTr(ResultCodeEnum.CLAIM_CLAIMABLE_BALANCE_SUCCESS);
        Utils.AssertResultOfType(Utils.CreateTransactionResultXdr(operationResultTr),
            typeof(ClaimClaimableBalanceSuccess), true);
    }

    private static OperationResult.OperationResultTr CreateOperationResultTr(ResultCodeEnum type)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceResult = new ClaimClaimableBalanceResult
            {
                Discriminant = ClaimClaimableBalanceResultCode.Create(type),
            },
        };
    }
}