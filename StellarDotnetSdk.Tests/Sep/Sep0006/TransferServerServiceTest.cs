using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0006;
using StellarDotnetSdk.Sep.Sep0006.Exceptions;
using StellarDotnetSdk.Sep.Sep0006.Requests;

namespace StellarDotnetSdk.Tests.Sep.Sep0006;

/// <summary>
///     Tests for TransferServerService class functionality.
/// </summary>
[TestClass]
public class TransferServerServiceTest
{
    private const string ServiceAddress = "http://api.stellar.org/transfer";

    private const string JwtToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJHQTZVSVhYUEVXWUZJTE5VSVdBQzM3WTRRUEVaTVFWREpIREtWV0ZaSjJLQ1dVQklVNUlYWk5EQSIsImp0aSI6IjE0NGQzNjdiY2IwZTcyY2FiZmRiZGU2MGVhZTBhZDczM2NjNjVkMmE2NTg3MDgzZGFiM2Q2MTZmODg1MTkwMjQiLCJpc3MiOiJodHRwczovL2ZsYXBweS1iaXJkLWRhcHAuZmlyZWJhc2VhcHAuY29tLyIsImlhdCI6MTUzNDI1Nzk5NCwiZXhwIjoxNTM0MzQ0Mzk0fQ.8nbB83Z6vGBgC1X9r3N6oQCFTBzDiITAfCJasRft0z0";

    private const string AccountId = "GBWMCCC3NHSKLAOJDBKKYW7SSH2PFTTNVFKWSGLWGDLEBKLOVP5JLBBP";

    private static HttpClient CreateMockHttpClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Utils.CreateFakeHttpClient(content, statusCode);
    }

    private static async Task<string> LoadTestDataAsync(string fileName)
    {
        var path = Utils.GetTestDataAbsolutePath($"Sep/Sep0006/{fileName}");
        return await File.ReadAllTextAsync(path);
    }

    /// <summary>
    ///     Verifies that InfoAsync retrieves anchor capabilities correctly.
    /// </summary>
    [TestMethod]
    public async Task InfoAsync_WithValidResponse_ReturnsInfoResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("info.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);

        // Act
        var response = await service.InfoAsync();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.DepositAssets);
        Assert.AreEqual(2, response.DepositAssets.Count);
        Assert.IsTrue(response.DepositAssets.ContainsKey("USD"));
        Assert.IsTrue(response.DepositAssets.ContainsKey("ETH"));

        var usdDeposit = response.DepositAssets["USD"];
        Assert.IsTrue(usdDeposit.Enabled);
        Assert.IsTrue(usdDeposit.AuthenticationRequired);
        Assert.AreEqual(0.1m, usdDeposit.MinAmount);
        Assert.AreEqual(1000.0m, usdDeposit.MaxAmount);
        Assert.AreEqual(0.25m, usdDeposit.FeeFixed);
        Assert.AreEqual(0.5m, usdDeposit.FeePercent);
        Assert.IsNotNull(usdDeposit.FundingMethods);
        Assert.AreEqual(3, usdDeposit.FundingMethods.Count);
        Assert.IsTrue(usdDeposit.FundingMethods.Contains("SEPA"));
        Assert.IsTrue(usdDeposit.FundingMethods.Contains("SWIFT"));
        Assert.IsTrue(usdDeposit.FundingMethods.Contains("cash"));
        Assert.IsNotNull(usdDeposit.Fields);
        Assert.AreEqual(4, usdDeposit.Fields.Count);
        Assert.IsTrue(usdDeposit.Fields.ContainsKey("email_address"));
        Assert.IsTrue(usdDeposit.Fields["email_address"].Optional);
        Assert.IsTrue(usdDeposit.Fields.ContainsKey("country_code"));
        Assert.IsNotNull(usdDeposit.Fields["country_code"].Choices);
        Assert.IsTrue(usdDeposit.Fields["country_code"].Choices?.Contains("USA"));

        Assert.IsNotNull(response.DepositExchangeAssets);
        Assert.IsTrue(response.DepositExchangeAssets.ContainsKey("USD"));
        var usdDepositExchange = response.DepositExchangeAssets["USD"];
        Assert.IsFalse(usdDepositExchange.Enabled);
        Assert.IsTrue(usdDepositExchange.AuthenticationRequired);
        Assert.IsNotNull(usdDepositExchange.FundingMethods);
        Assert.AreEqual(2, usdDepositExchange.FundingMethods.Count);
        Assert.IsTrue(usdDepositExchange.FundingMethods.Contains("SEPA"));
        Assert.IsTrue(usdDepositExchange.FundingMethods.Contains("SWIFT"));
        Assert.IsNotNull(usdDepositExchange.Fields);
        Assert.IsTrue(usdDepositExchange.Fields.ContainsKey("email_address"));
        Assert.IsTrue(usdDepositExchange.Fields.ContainsKey("amount"));
        Assert.IsTrue(usdDepositExchange.Fields.ContainsKey("country_code"));
        Assert.IsTrue(usdDepositExchange.Fields.ContainsKey("type"));

        Assert.IsNotNull(response.WithdrawAssets);
        Assert.IsTrue(response.WithdrawAssets.ContainsKey("USD"));
        var usdWithdraw = response.WithdrawAssets["USD"];
        Assert.IsTrue(usdWithdraw.Enabled);
        Assert.IsTrue(usdWithdraw.AuthenticationRequired);
        Assert.AreEqual(0.5m, usdWithdraw.FeeFixed);
        Assert.AreEqual(1.5m, usdWithdraw.FeePercent);
        Assert.AreEqual(0.1m, usdWithdraw.MinAmount);
        Assert.AreEqual(1000.0m, usdWithdraw.MaxAmount);
        Assert.IsNotNull(usdWithdraw.FundingMethods);
        Assert.AreEqual(2, usdWithdraw.FundingMethods.Count);
        Assert.IsTrue(usdWithdraw.FundingMethods.Contains("bank_account"));
        Assert.IsTrue(usdWithdraw.FundingMethods.Contains("cash"));
        Assert.IsNotNull(usdWithdraw.Types);
        Assert.AreEqual(2, usdWithdraw.Types.Count);
        Assert.IsTrue(usdWithdraw.Types.ContainsKey("bank_account"));
        Assert.IsTrue(usdWithdraw.Types.ContainsKey("cash"));

        Assert.IsFalse(response.WithdrawAssets["ETH"].Enabled);

        Assert.IsNotNull(response.WithdrawExchangeAssets);
        Assert.IsTrue(response.WithdrawExchangeAssets.ContainsKey("USD"));
        var usdWithdrawExchange = response.WithdrawExchangeAssets["USD"];
        Assert.IsFalse(usdWithdrawExchange.Enabled);
        Assert.IsTrue(usdWithdrawExchange.AuthenticationRequired);
        Assert.AreEqual(0.1m, usdWithdrawExchange.MinAmount);
        Assert.AreEqual(1000.0m, usdWithdrawExchange.MaxAmount);
        Assert.IsNotNull(usdWithdrawExchange.FundingMethods);
        Assert.AreEqual(2, usdWithdrawExchange.FundingMethods.Count);
        Assert.IsTrue(usdWithdrawExchange.FundingMethods.Contains("bank_account"));
        Assert.IsTrue(usdWithdrawExchange.FundingMethods.Contains("cash"));
        Assert.IsNotNull(usdWithdrawExchange.Types);
        Assert.IsTrue(usdWithdrawExchange.Types.ContainsKey("bank_account"));
        Assert.IsTrue(usdWithdrawExchange.Types.ContainsKey("cash"));

        Assert.IsNotNull(response.FeeInfo);
        Assert.IsFalse(response.FeeInfo.Enabled);
        Assert.IsTrue(response.FeeInfo.AuthenticationRequired);
        Assert.IsNotNull(response.FeeInfo.Description);
        Assert.IsTrue(response.FeeInfo.Description.Contains("Fees vary from 3 to 7 percent"));

        Assert.IsNotNull(response.TransactionsInfo);
        Assert.IsTrue(response.TransactionsInfo.Enabled);
        Assert.IsTrue(response.TransactionsInfo.AuthenticationRequired);

        Assert.IsNotNull(response.TransactionInfo);
        Assert.IsFalse(response.TransactionInfo.Enabled);
        Assert.IsTrue(response.TransactionInfo.AuthenticationRequired);

        Assert.IsNotNull(response.FeatureFlags);
        Assert.IsTrue(response.FeatureFlags.AccountCreation);
        Assert.IsTrue(response.FeatureFlags.ClaimableBalances);
    }

    /// <summary>
    ///     Verifies that FeeAsync retrieves fee information correctly.
    /// </summary>
    [TestMethod]
    public async Task FeeAsync_WithValidRequest_ReturnsFeeResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("fee.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new FeeRequest
        {
            Operation = "deposit",
            AssetCode = "ETH",
            Amount = 2034.09m,
            Type = "SEPA",
            Jwt = JwtToken,
        };

        // Act
        var response = await service.FeeAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(0.013m, response.Fee);
    }

    /// <summary>
    ///     Verifies that DepositAsync with bank payment returns correct response.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_BankPayment_ReturnsDepositResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-bank.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Amount = 123.123m,
            Jwt = JwtToken,
        };

        // Act
        var response = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("Make a payment to Bank: 121122676 Account: 13719713158835300", response.How);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
        Assert.IsNull(response.FeeFixed);
        Assert.AreEqual(10.0m, response.MinAmount);
        Assert.AreEqual(5000.0m, response.MaxAmount);
        Assert.IsNotNull(response.Instructions);
        Assert.IsTrue(response.Instructions.ContainsKey("organization.bank_number"));
        Assert.AreEqual("121122676", response.Instructions["organization.bank_number"].Value);
        Assert.AreEqual("US bank routing number", response.Instructions["organization.bank_number"].Description);
    }

    /// <summary>
    ///     Verifies that DepositAsync with BTC returns correct response.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_BTC_ReturnsDepositResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-btc.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "BTC",
            Account = AccountId,
            Amount = 3.123m,
            Jwt = JwtToken,
        };

        // Act
        var response = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("Make a payment to Bitcoin address 1Nh7uHdvY6fNwtQtM1G5EZAFPLC33B59rB", response.How);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
        Assert.AreEqual(0.0002m, response.FeeFixed);
        Assert.IsNotNull(response.Instructions);
        Assert.IsTrue(response.Instructions.ContainsKey("organization.crypto_address"));
        Assert.AreEqual("1Nh7uHdvY6fNwtQtM1G5EZAFPLC33B59rB",
            response.Instructions["organization.crypto_address"].Value);
    }

    /// <summary>
    ///     Verifies that DepositAsync with Ripple returns correct response.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_Ripple_ReturnsDepositResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-ripple.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "XRP",
            Account = AccountId,
            Amount = 300.0m,
            Jwt = JwtToken,
        };

        // Act
        var response = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("Make a payment to Ripple address rNXEkKCxvfLcM1h4HJkaj2FtmYuAWrHGbf with tag 88",
            response.How);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
        Assert.AreEqual(60, response.Eta);
        Assert.AreEqual(0.1m, response.FeePercent);
        Assert.IsNotNull(response.ExtraInfo);
        Assert.IsNotNull(response.ExtraInfo.Message);
        Assert.IsTrue(response.ExtraInfo.Message.Contains("You must include the tag"));
    }

    /// <summary>
    ///     Verifies that DepositAsync with MXN returns correct response.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_MXN_ReturnsDepositResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-mxn.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "MXN",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        var response = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("Make a payment to Bank: STP Account: 646180111803859359", response.How);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
        Assert.AreEqual(1800, response.Eta);
    }

    /// <summary>
    ///     Verifies that WithdrawAsync returns correct response.
    /// </summary>
    [TestMethod]
    public async Task WithdrawAsync_WithValidRequest_ReturnsWithdrawResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("withdraw.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new WithdrawRequest
        {
            AssetCode = "XLM",
            Type = "crypto",
            Dest = "GCTTGO5ABSTHABXWL2FMHPZ2XFOZDXJYJN5CKFRKXMPAAWZW3Y3JZ3JK",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        var response = await service.WithdrawAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GCIBUCGPOHWMMMFPFTDWBSVHQRT4DIBJ7AD6BZJYDITBK2LCVBYW7HUQ", response.AccountId);
        Assert.AreEqual("id", response.MemoType);
        Assert.AreEqual("123", response.Memo);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
        Assert.AreEqual(3600, response.Eta);
        Assert.AreEqual(0.1m, response.MinAmount);
        Assert.AreEqual(10000.0m, response.MaxAmount);
        Assert.AreEqual(0.5m, response.FeeFixed);
        Assert.AreEqual(1.5m, response.FeePercent);
        Assert.IsNotNull(response.ExtraInfo);
        Assert.IsNotNull(response.ExtraInfo.Message);
        Assert.IsTrue(response.ExtraInfo.Message.Contains("bank account details"));
    }

    /// <summary>
    ///     Verifies that DepositExchangeAsync returns correct response.
    /// </summary>
    [TestMethod]
    public async Task DepositExchangeAsync_WithValidRequest_ReturnsDepositResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-bank.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositExchangeRequest
        {
            DestinationAsset = "XYZ",
            SourceAsset = "iso4217:USD",
            QuoteId = "282837",
            Amount = 100m,
            Account = "GCIBUCGPOHWMMMFPFTDWBSVHQRT4DIBJ7AD6BZJYDITBK2LCVBYW7HUQ",
            LocationId = "999",
            Jwt = JwtToken,
        };

        // Act
        var response = await service.DepositExchangeAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("Make a payment to Bank: 121122676 Account: 13719713158835300", response.How);
        Assert.AreEqual("9421871e-0623-4356-b7b5-5996da122f3e", response.Id);
    }

    /// <summary>
    ///     Verifies that WithdrawExchangeAsync returns correct response.
    /// </summary>
    [TestMethod]
    public async Task WithdrawExchangeAsync_WithValidRequest_ReturnsWithdrawResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("withdraw.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new WithdrawExchangeRequest
        {
            SourceAsset = "XYZ",
            DestinationAsset = "iso4217:USD",
            QuoteId = "282837",
            Amount = 700m,
            Type = "bank_account",
            LocationId = "999",
            Jwt = JwtToken,
        };

        // Act
        var response = await service.WithdrawExchangeAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual("GCIBUCGPOHWMMMFPFTDWBSVHQRT4DIBJ7AD6BZJYDITBK2LCVBYW7HUQ", response.AccountId);
        Assert.AreEqual("id", response.MemoType);
        Assert.AreEqual("123", response.Memo);
    }

    /// <summary>
    ///     Verifies that DepositAsync throws CustomerInformationNeededException when customer info is needed.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(CustomerInformationNeededException))]
    public async Task DepositAsync_CustomerInformationNeeded_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("customer-information-needed.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "MXN",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        try
        {
            await service.DepositAsync(request);
        }
        catch (CustomerInformationNeededException ex)
        {
            // Assert
            Assert.IsNotNull(ex.Response);
            Assert.IsNotNull(ex.Response.Fields);
            Assert.IsTrue(ex.Response.Fields.Contains("tax_id"));
            throw;
        }
    }

    /// <summary>
    ///     Verifies that WithdrawAsync throws CustomerInformationNeededException when customer info is needed.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(CustomerInformationNeededException))]
    public async Task WithdrawAsync_CustomerInformationNeeded_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("customer-information-needed.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new WithdrawRequest
        {
            AssetCode = "XLM",
            Type = "crypto",
            Dest = "GCTTGO5ABSTHABXWL2FMHPZ2XFOZDXJYJN5CKFRKXMPAAWZW3Y3JZ3JK",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        try
        {
            await service.WithdrawAsync(request);
        }
        catch (CustomerInformationNeededException ex)
        {
            // Assert
            Assert.IsNotNull(ex.Response);
            Assert.IsNotNull(ex.Response.Fields);
            Assert.IsTrue(ex.Response.Fields.Contains("tax_id"));
            throw;
        }
    }

    /// <summary>
    ///     Verifies that DepositAsync throws CustomerInformationStatusException when customer info status is pending/denied.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(CustomerInformationStatusException))]
    public async Task DepositAsync_CustomerInformationStatus_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("customer-information-status.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "MXN",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        try
        {
            await service.DepositAsync(request);
        }
        catch (CustomerInformationStatusException ex)
        {
            // Assert
            Assert.IsNotNull(ex.Response);
            Assert.AreEqual("denied", ex.Response.Status);
            Assert.IsNotNull(ex.Response.MoreInfoUrl);
            Assert.AreEqual(3600, ex.Response.Eta);
            throw;
        }
    }

    /// <summary>
    ///     Verifies that TransactionsAsync returns transaction history correctly.
    /// </summary>
    [TestMethod]
    public async Task TransactionsAsync_WithValidRequest_ReturnsTransactionsResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("transactions.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new AnchorTransactionsRequest
        {
            AssetCode = "XLM",
            Account = "GCTTGO5ABSTHABXWL2FMHPZ2XFOZDXJYJN5CKFRKXMPAAWZW3Y3JZ3JK",
            Jwt = JwtToken,
        };

        // Act
        var response = await service.TransactionsAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Transactions);
        Assert.AreEqual(6, response.Transactions.Count);

        var firstTx = response.Transactions[0];
        Assert.AreEqual("82fhs729f63dh0v4", firstTx.Id);
        Assert.AreEqual("deposit", firstTx.Kind);
        Assert.AreEqual("pending_external", firstTx.Status);
        Assert.AreEqual(3600, firstTx.StatusEta);
        Assert.AreEqual(18.34m, firstTx.AmountIn);
        Assert.AreEqual(18.24m, firstTx.AmountOut);
        Assert.AreEqual(0.1m, firstTx.AmountFee);
        Assert.IsNotNull(firstTx.UpdatedAt);
        Assert.IsNotNull(firstTx.MoreInfoUrl);
        Assert.IsNotNull(firstTx.DepositMemo);
        Assert.AreEqual("memo123", firstTx.DepositMemo);
        Assert.AreEqual("text", firstTx.DepositMemoType);
        Assert.IsNotNull(firstTx.ClaimableBalanceId);
        Assert.IsNotNull(firstTx.ExternalExtra);
        Assert.AreEqual("SWIFT123", firstTx.ExternalExtra);
        Assert.IsNotNull(firstTx.ExternalExtraText);
        Assert.AreEqual("Bank of America", firstTx.ExternalExtraText);
        Assert.IsNotNull(firstTx.ExternalTransactionId);
        Assert.AreEqual("2dd16cb409513026fbe7defc0c6f826c2d2c65c3da993f747d09bf7dafd31093",
            firstTx.ExternalTransactionId);
        Assert.IsNotNull(firstTx.StartedAt);
        Assert.AreEqual("2017-03-20T17:05:32Z", firstTx.StartedAt);
        Assert.IsNotNull(firstTx.Instructions);
        Assert.AreEqual(2, firstTx.Instructions.Count);
        Assert.IsTrue(firstTx.Instructions.ContainsKey("organization.bank_number"));
        Assert.AreEqual("121122676", firstTx.Instructions["organization.bank_number"].Value);
        Assert.AreEqual("US bank routing number", firstTx.Instructions["organization.bank_number"].Description);
        Assert.IsTrue(firstTx.Instructions.ContainsKey("organization.bank_account_number"));
        Assert.AreEqual("13719713158835300", firstTx.Instructions["organization.bank_account_number"].Value);
        Assert.AreEqual("US bank account number", firstTx.Instructions["organization.bank_account_number"].Description);

        var secondTx = response.Transactions[1];
        Assert.AreEqual("52fys79f63dh3v2", secondTx.Id);
        Assert.AreEqual("deposit-exchange", secondTx.Kind);
        Assert.AreEqual("iso4217:BRL", secondTx.AmountInAsset);
        Assert.AreEqual("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN",
            secondTx.AmountOutAsset);
        Assert.AreEqual("quote-12345", secondTx.QuoteId);
        Assert.IsNotNull(secondTx.Message);
        Assert.IsTrue(secondTx.Message.Contains("Exchange transaction"));
        Assert.IsNotNull(secondTx.AmountFeeAsset);
        Assert.AreEqual("iso4217:BRL", secondTx.AmountFeeAsset);
        Assert.IsNotNull(secondTx.ExternalTransactionId);
        Assert.AreEqual("2dd16cb409513026fbe7defc0c6f826c2d2c65c3da993f747d09bf7dafd31093",
            secondTx.ExternalTransactionId);
        Assert.IsNotNull(secondTx.StartedAt);
        Assert.AreEqual("2021-06-11T17:05:32Z", secondTx.StartedAt);

        var thirdTx = response.Transactions[2];
        Assert.AreEqual("withdrawal", thirdTx.Kind);
        Assert.AreEqual("completed", thirdTx.Status);
        Assert.IsNotNull(thirdTx.Refunds);
        Assert.AreEqual(10m, thirdTx.Refunds.AmountRefunded);
        Assert.AreEqual(5m, thirdTx.Refunds.AmountFee);
        Assert.AreEqual(1, thirdTx.Refunds.Payments.Count);
        Assert.IsNotNull(thirdTx.Refunds.Payments[0].Id);
        Assert.AreEqual("b9d0b2292c4e09e8eb22d036171491e87b8d2086bf8b265874c8d182cb9c9020",
            thirdTx.Refunds.Payments[0].Id);
        Assert.AreEqual("stellar", thirdTx.Refunds.Payments[0].IdType);
        Assert.AreEqual(10m, thirdTx.Refunds.Payments[0].Amount);
        Assert.AreEqual(5m, thirdTx.Refunds.Payments[0].Fee);
        Assert.IsNotNull(thirdTx.WithdrawAnchorAccount);
        Assert.AreEqual("GBANAGOAXH5ONSBI2I6I5LHP2TCRHWMZIAMGUQH2TNKQNCOGJ7GC3ZOL", thirdTx.WithdrawAnchorAccount);
        Assert.IsNotNull(thirdTx.WithdrawMemo);
        Assert.AreEqual("186384", thirdTx.WithdrawMemo);
        Assert.IsNotNull(thirdTx.WithdrawMemoType);
        Assert.AreEqual("id", thirdTx.WithdrawMemoType);
        Assert.IsNotNull(thirdTx.StartedAt);
        Assert.AreEqual("2017-03-20T17:00:02Z", thirdTx.StartedAt);
        Assert.IsNotNull(thirdTx.CompletedAt);
        Assert.AreEqual("2017-03-20T17:09:58Z", thirdTx.CompletedAt);
        Assert.IsNotNull(thirdTx.StellarTransactionId);
        Assert.AreEqual("17a670bc424ff5ce3b386dbfaae9990b66a2a37b4fbe51547e8794962a3f9e6a",
            thirdTx.StellarTransactionId);
        Assert.IsNotNull(thirdTx.ExternalTransactionId);
        Assert.AreEqual("1238234", thirdTx.ExternalTransactionId);

        var fourthTx = response.Transactions[3];
        Assert.AreEqual("deposit", fourthTx.Kind);
        Assert.IsNotNull(fourthTx.Refunds);
        Assert.IsNotNull(fourthTx.Refunds.Payments[0].Id);
        Assert.AreEqual("104201", fourthTx.Refunds.Payments[0].Id);
        Assert.AreEqual("external", fourthTx.Refunds.Payments[0].IdType);
        Assert.AreEqual(10m, fourthTx.Refunds.Payments[0].Amount);
        Assert.AreEqual(5m, fourthTx.Refunds.Payments[0].Fee);
        Assert.IsNotNull(fourthTx.UpdatedAt);
        Assert.IsFalse(fourthTx.Refunded.Value);
        Assert.IsNotNull(fourthTx.From);
        Assert.AreEqual("AJ3845SAD", fourthTx.From);
        Assert.IsNotNull(fourthTx.To);
        Assert.AreEqual("GBITQ4YAFKD2372TNAMNHQ4JV5VS3BYKRK4QQR6FOLAR7XAHC3RVGVVJ", fourthTx.To);
        Assert.IsNotNull(fourthTx.StartedAt);
        Assert.AreEqual("2017-03-20T17:00:02Z", fourthTx.StartedAt);
        Assert.IsNotNull(fourthTx.CompletedAt);
        Assert.AreEqual("2017-03-20T17:09:58Z", fourthTx.CompletedAt);
        Assert.IsNotNull(fourthTx.StellarTransactionId);
        Assert.AreEqual("17a670bc424ff5ce3b386dbfaae9990b66a2a37b4fbe51547e8794962a3f9e6a",
            fourthTx.StellarTransactionId);
        Assert.IsNotNull(fourthTx.ExternalTransactionId);
        Assert.AreEqual("1238234", fourthTx.ExternalTransactionId);

        var fifthTx = response.Transactions[4];
        Assert.AreEqual("pending_transaction_info_update", fifthTx.Status);
        Assert.IsNotNull(fifthTx.RequiredInfoMessage);
        Assert.IsNotNull(fifthTx.RequiredInfoUpdates);
        Assert.IsTrue(fifthTx.RequiredInfoUpdates.Transaction?.ContainsKey("dest"));
        Assert.AreEqual("your bank account number", fifthTx.RequiredInfoUpdates.Transaction?["dest"].Description);
        Assert.IsNotNull(fifthTx.UserActionRequiredBy);
        Assert.IsNotNull(fifthTx.StartedAt);
        Assert.AreEqual("2017-03-20T17:00:02Z", fifthTx.StartedAt);

        var sixthTx = response.Transactions[5];
        Assert.AreEqual("withdrawal-exchange", sixthTx.Kind);
        Assert.AreEqual("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN", sixthTx.AmountInAsset);
        Assert.IsNotNull(sixthTx.AmountFeeAsset);
        Assert.AreEqual("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN",
            sixthTx.AmountFeeAsset);
        Assert.IsNotNull(sixthTx.StellarTransactionId);
        Assert.AreEqual("17a670bc424ff5ce3b386dbfaae9990b66a2a37b4fbe51547e8794962a3f9e6a",
            sixthTx.StellarTransactionId);
        Assert.IsNotNull(sixthTx.StartedAt);
        Assert.AreEqual("2021-06-11T17:05:32Z", sixthTx.StartedAt);
    }

    /// <summary>
    ///     Verifies that TransactionAsync returns single transaction correctly.
    /// </summary>
    [TestMethod]
    public async Task TransactionAsync_WithValidRequest_ReturnsTransactionResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("transaction.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new AnchorTransactionRequest
        {
            Id = "82fhs729f63dh0v4",
            StellarTransactionId = "17a670bc424ff5ce3b386dbfaae9990b66a2a37b4fbe51547e8794962a3f9e6a",
            Jwt = JwtToken,
        };

        // Act
        var response = await service.TransactionAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Transaction);
        Assert.AreEqual("82fhs729f63dh0v4", response.Transaction.Id);
        Assert.AreEqual("deposit", response.Transaction.Kind);
        Assert.AreEqual("pending_external", response.Transaction.Status);
        Assert.AreEqual(3600, response.Transaction.StatusEta);
        Assert.AreEqual(18.34m, response.Transaction.AmountIn);
        Assert.AreEqual(18.24m, response.Transaction.AmountOut);
        Assert.AreEqual(0.1m, response.Transaction.AmountFee);
        Assert.IsNotNull(response.Transaction.FeeDetails);
        Assert.AreEqual(0.1m, response.Transaction.FeeDetails.Total);
        Assert.AreEqual("iso4217:USD", response.Transaction.FeeDetails.Asset);
        Assert.IsNotNull(response.Transaction.FeeDetails.Details);
        Assert.AreEqual(2, response.Transaction.FeeDetails.Details.Count);
        Assert.AreEqual("Service fee", response.Transaction.FeeDetails.Details[0].Name);
        Assert.AreEqual(0.05m, response.Transaction.FeeDetails.Details[0].Amount);
        Assert.AreEqual("Standard processing fee", response.Transaction.FeeDetails.Details[0].Description);
        Assert.AreEqual("Network fee", response.Transaction.FeeDetails.Details[1].Name);
        Assert.AreEqual(0.05m, response.Transaction.FeeDetails.Details[1].Amount);
        Assert.AreEqual("Blockchain network transaction fee", response.Transaction.FeeDetails.Details[1].Description);
        Assert.IsNotNull(response.Transaction.ExternalTransactionId);
        Assert.AreEqual("2dd16cb409513026fbe7defc0c6f826c2d2c65c3da993f747d09bf7dafd31093",
            response.Transaction.ExternalTransactionId);
        Assert.IsNotNull(response.Transaction.StartedAt);
        Assert.AreEqual("2017-03-20T17:05:32Z", response.Transaction.StartedAt);
    }

    /// <summary>
    ///     Verifies that TransactionAsync includes ExternalTransactionId and Lang query parameters in the request URL.
    /// </summary>
    [TestMethod]
    public async Task TransactionAsync_WithExternalTransactionIdAndLang_IncludesQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("transaction.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new AnchorTransactionRequest
        {
            ExternalTransactionId = "ext123456",
            Lang = "es",
            Jwt = JwtToken,
        };

        // Act
        await service.TransactionAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("external_transaction_id=ext123456"));
        Assert.IsTrue(uriString.Contains("lang=es"));
    }

    /// <summary>
    ///     Verifies that PatchTransactionAsync updates transaction correctly.
    /// </summary>
    [TestMethod]
    public async Task PatchTransactionAsync_WithValidRequest_ReturnsTransactionResponse()
    {
        // Arrange
        var content = await LoadTestDataAsync("transaction.json");
        using var httpClient = CreateMockHttpClient(content);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new PatchTransactionRequest
        {
            Id = "82fhs729f63dh0v4",
            Fields = new Dictionary<string, object>
            {
                { "dest", "12345678901234" },
                { "dest_extra", "021000021" },
            },
            Jwt = JwtToken,
        };

        // Act
        var response = await service.PatchTransactionAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Transaction);
        Assert.AreEqual("82fhs729f63dh0v4", response.Transaction.Id);
        Assert.AreEqual("deposit", response.Transaction.Kind);
        Assert.AreEqual("pending_external", response.Transaction.Status);
        Assert.AreEqual(3600, response.Transaction.StatusEta);
        Assert.AreEqual(18.34m, response.Transaction.AmountIn);
        Assert.AreEqual(18.24m, response.Transaction.AmountOut);
        Assert.AreEqual(0.1m, response.Transaction.AmountFee);
        Assert.IsNotNull(response.Transaction.FeeDetails);
        Assert.AreEqual(0.1m, response.Transaction.FeeDetails.Total);
        Assert.AreEqual("iso4217:USD", response.Transaction.FeeDetails.Asset);
    }

    /// <summary>
    ///     Verifies that PatchTransactionAsync throws ArgumentException when Fields is null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task PatchTransactionAsync_WithNullFields_ThrowsArgumentException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("");
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new PatchTransactionRequest
        {
            Id = "82fhs729f63dh0v4",
            Fields = null,
            Jwt = JwtToken,
        };

        // Act
        await service.PatchTransactionAsync(request);
    }

    /// <summary>
    ///     Verifies that FromDomainAsync creates service from domain correctly.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithValidDomain_CreatesService()
    {
        // Arrange
        var tomlContent =
            await File.ReadAllTextAsync(Utils.GetTestDataAbsolutePath("Sep/Sep0001/stellar-toml-sample.toml"));
        using var httpClient = CreateMockHttpClient(tomlContent);
        var service = await TransferServerService.FromDomainAsync("example.com", httpClient);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws TransferServerException when TRANSFER_SERVER is not found.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(TransferServerException))]
    public async Task FromDomainAsync_WithoutTransferServer_ThrowsException()
    {
        // Arrange
        var tomlContent = @"VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""";
        using var httpClient = CreateMockHttpClient(tomlContent);

        // Act
        await TransferServerService.FromDomainAsync("example.com", httpClient);
    }

    /// <summary>
    ///     Verifies that constructor throws ArgumentException when transfer service address is null or empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithEmptyAddress_ThrowsArgumentException()
    {
        // Act
        _ = new TransferServerService("");
    }

    /// <summary>
    ///     Verifies that InfoAsync with language parameter includes it in request.
    /// </summary>
    [TestMethod]
    public async Task InfoAsync_WithLanguage_IncludesLanguageParameter()
    {
        // Arrange
        var content = await LoadTestDataAsync("info.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);

        // Act
        await service.InfoAsync("es");

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        Assert.IsTrue(capture.Request.RequestUri?.ToString().Contains("lang=es"));
    }

    /// <summary>
    ///     Verifies that DepositAsync throws AuthenticationRequiredException when authentication is required.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AuthenticationRequiredException))]
    public async Task DepositAsync_AuthenticationRequired_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("authentication-required.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Amount = 100.0m,
        };

        // Act
        await service.DepositAsync(request);
    }

    /// <summary>
    ///     Verifies that TransactionsAsync includes all optional query parameters in the request URL.
    /// </summary>
    [TestMethod]
    public async Task TransactionsAsync_WithAllOptionalParams_IncludesAllQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("transactions.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new AnchorTransactionsRequest
        {
            AssetCode = "XLM",
            Account = AccountId,
            NoOlderThan = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Limit = 10,
            Kind = "deposit",
            PagingId = "paging123",
            Lang = "en",
            Jwt = JwtToken,
        };

        // Act
        await service.TransactionsAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("no_older_than="));
        Assert.IsTrue(uriString.Contains("limit=10"));
        Assert.IsTrue(uriString.Contains("kind=deposit"));
        Assert.IsTrue(uriString.Contains("paging_id=paging123"));
        Assert.IsTrue(uriString.Contains("lang=en"));
    }

    /// <summary>
    ///     Verifies that DepositAsync includes all optional query parameters in the request URL.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_WithAllOptionalFields_IncludesAllQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-bank.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Amount = 123.123m,
            FundingMethod = "WIRE",
            MemoType = "text",
            Memo = "memo123",
            EmailAddress = "test@example.com",
            Type = "SEPA",
            WalletName = "TestWallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en",
            OnChangeCallback = "https://callback.example.com",
            CountryCode = "USA",
            ClaimableBalanceSupported = "true",
            CustomerId = "customer123",
            LocationId = "location456",
            ExtraFields = new Dictionary<string, string>
            {
                { "custom_field", "custom_value" },
            },
            Jwt = JwtToken,
        };

        // Act
        await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("funding_method=WIRE"));
        Assert.IsTrue(uriString.Contains("memo_type=text"));
        Assert.IsTrue(uriString.Contains("memo=memo123"));
        Assert.IsTrue(uriString.Contains("email_address=test%40example.com"));
        Assert.IsTrue(uriString.Contains("type=SEPA"));
        Assert.IsTrue(uriString.Contains("wallet_name=TestWallet"));
        Assert.IsTrue(uriString.Contains("wallet_url="));
        Assert.IsTrue(uriString.Contains("lang=en"));
        Assert.IsTrue(uriString.Contains("on_change_callback="));
        Assert.IsTrue(uriString.Contains("country_code=USA"));
        Assert.IsTrue(uriString.Contains("claimable_balance_supported=true"));
        Assert.IsTrue(uriString.Contains("customer_id=customer123"));
        Assert.IsTrue(uriString.Contains("location_id=location456"));
        Assert.IsTrue(uriString.Contains("custom_field=custom_value"));
    }

    /// <summary>
    ///     Verifies that WithdrawAsync includes all optional query parameters in the request URL.
    /// </summary>
    [TestMethod]
    public async Task WithdrawAsync_WithAllOptionalFields_IncludesAllQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("withdraw.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new WithdrawRequest
        {
            AssetCode = "XLM",
            FundingMethod = "bank_account",
            Type = "crypto",
            Dest = "GCTTGO5ABSTHABXWL2FMHPZ2XFOZDXJYJN5CKFRKXMPAAWZW3Y3JZ3JK",
            DestExtra = "extra123",
            Account = AccountId,
            Memo = "memo123",
            MemoType = "text",
            WalletName = "TestWallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en",
            OnChangeCallback = "https://callback.example.com",
            Amount = 120.0m,
            CountryCode = "USA",
            RefundMemo = "refund123",
            RefundMemoType = "text",
            CustomerId = "customer123",
            LocationId = "location456",
            ExtraFields = new Dictionary<string, string>
            {
                { "custom_field", "custom_value" },
            },
            Jwt = JwtToken,
        };

        // Act
        await service.WithdrawAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("funding_method=bank_account"));
        Assert.IsTrue(uriString.Contains("dest="));
        Assert.IsTrue(uriString.Contains("dest_extra=extra123"));
        Assert.IsTrue(uriString.Contains("memo=memo123"));
        Assert.IsTrue(uriString.Contains("memo_type=text"));
        Assert.IsTrue(uriString.Contains("wallet_name=TestWallet"));
        Assert.IsTrue(uriString.Contains("wallet_url="));
        Assert.IsTrue(uriString.Contains("lang=en"));
        Assert.IsTrue(uriString.Contains("on_change_callback="));
        Assert.IsTrue(uriString.Contains("country_code=USA"));
        Assert.IsTrue(uriString.Contains("refund_memo=refund123"));
        Assert.IsTrue(uriString.Contains("refund_memo_type=text"));
        Assert.IsTrue(uriString.Contains("customer_id=customer123"));
        Assert.IsTrue(uriString.Contains("location_id=location456"));
        Assert.IsTrue(uriString.Contains("custom_field=custom_value"));
    }

    /// <summary>
    ///     Verifies that DepositExchangeAsync includes all optional query parameters in the request URL.
    /// </summary>
    [TestMethod]
    public async Task DepositExchangeAsync_WithAllOptionalFields_IncludesAllQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("deposit-bank.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositExchangeRequest
        {
            DestinationAsset = "XYZ",
            SourceAsset = "iso4217:USD",
            Amount = 100m,
            Account = AccountId,
            FundingMethod = "WIRE",
            QuoteId = "quote123",
            MemoType = "text",
            Memo = "memo123",
            EmailAddress = "test@example.com",
            Type = "SEPA",
            WalletName = "TestWallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en",
            OnChangeCallback = "https://callback.example.com",
            CountryCode = "USA",
            ClaimableBalanceSupported = "true",
            CustomerId = "customer123",
            LocationId = "location456",
            ExtraFields = new Dictionary<string, string>
            {
                { "custom_field", "custom_value" },
            },
            Jwt = JwtToken,
        };

        // Act
        await service.DepositExchangeAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("destination_asset=XYZ"));
        Assert.IsTrue(uriString.Contains("source_asset=iso4217%3aUSD"));
        Assert.IsTrue(uriString.Contains("amount=100"));
        Assert.IsTrue(uriString.Contains("funding_method=WIRE"));
        Assert.IsTrue(uriString.Contains("quote_id=quote123"));
        Assert.IsTrue(uriString.Contains("memo_type=text"));
        Assert.IsTrue(uriString.Contains("memo=memo123"));
        Assert.IsTrue(uriString.Contains("email_address=test%40example.com"));
        Assert.IsTrue(uriString.Contains("type=SEPA"));
        Assert.IsTrue(uriString.Contains("wallet_name=TestWallet"));
        Assert.IsTrue(uriString.Contains("wallet_url="));
        Assert.IsTrue(uriString.Contains("lang=en"));
        Assert.IsTrue(uriString.Contains("on_change_callback="));
        Assert.IsTrue(uriString.Contains("country_code=USA"));
        Assert.IsTrue(uriString.Contains("claimable_balance_supported=true"));
        Assert.IsTrue(uriString.Contains("customer_id=customer123"));
        Assert.IsTrue(uriString.Contains("location_id=location456"));
        Assert.IsTrue(uriString.Contains("custom_field=custom_value"));
    }

    /// <summary>
    ///     Verifies that WithdrawExchangeAsync includes all optional query parameters in the request URL.
    ///     Note: claimable_balance_supported is NOT included as it's not part of SEP-6 withdraw-exchange spec.
    /// </summary>
    [TestMethod]
    public async Task WithdrawExchangeAsync_WithAllOptionalFields_IncludesAllQueryParams()
    {
        // Arrange
        var content = await LoadTestDataAsync("withdraw.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new WithdrawExchangeRequest
        {
            SourceAsset = "XYZ",
            DestinationAsset = "iso4217:USD",
            Amount = 700m,
            FundingMethod = "bank_account",
            Type = "bank_account",
            Dest = "GCTTGO5ABSTHABXWL2FMHPZ2XFOZDXJYJN5CKFRKXMPAAWZW3Y3JZ3JK",
            DestExtra = "extra123",
            QuoteId = "quote123",
            Account = AccountId,
            Memo = "memo123",
            MemoType = "text",
            WalletName = "TestWallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en",
            OnChangeCallback = "https://callback.example.com",
            CountryCode = "USA",
            RefundMemo = "refund123",
            RefundMemoType = "text",
            CustomerId = "customer123",
            LocationId = "location456",
            ExtraFields = new Dictionary<string, string>
            {
                { "custom_field", "custom_value" },
            },
            Jwt = JwtToken,
        };

        // Act
        await service.WithdrawExchangeAsync(request);

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsNotNull(capture.Request.RequestUri);
        var uriString = capture.Request.RequestUri.ToString();
        Assert.IsTrue(uriString.Contains("source_asset=XYZ"));
        Assert.IsTrue(uriString.Contains("destination_asset=iso4217%3aUSD"));
        Assert.IsTrue(uriString.Contains("amount=700"));
        Assert.IsTrue(uriString.Contains("funding_method=bank_account"));
        Assert.IsTrue(uriString.Contains("type=bank_account"));
        Assert.IsTrue(uriString.Contains("dest="));
        Assert.IsTrue(uriString.Contains("dest_extra=extra123"));
        Assert.IsTrue(uriString.Contains("quote_id=quote123"));
        Assert.IsTrue(uriString.Contains("memo=memo123"));
        Assert.IsTrue(uriString.Contains("memo_type=text"));
        Assert.IsTrue(uriString.Contains("wallet_name=TestWallet"));
        Assert.IsTrue(uriString.Contains("wallet_url="));
        Assert.IsTrue(uriString.Contains("lang=en"));
        Assert.IsTrue(uriString.Contains("on_change_callback="));
        Assert.IsTrue(uriString.Contains("country_code=USA"));
        // Note: claimable_balance_supported is NOT part of withdraw-exchange per SEP-6 spec
        Assert.IsFalse(uriString.Contains("claimable_balance_supported"));
        Assert.IsTrue(uriString.Contains("refund_memo=refund123"));
        Assert.IsTrue(uriString.Contains("refund_memo_type=text"));
        Assert.IsTrue(uriString.Contains("customer_id=customer123"));
        Assert.IsTrue(uriString.Contains("location_id=location456"));
        Assert.IsTrue(uriString.Contains("custom_field=custom_value"));
    }

    /// <summary>
    ///     Verifies that CustomerInformationNeededException handles empty/null Fields correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(CustomerInformationNeededException))]
    public async Task DepositAsync_CustomerInformationNeededEmptyFields_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("customer-information-needed-empty.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "MXN",
            Account = AccountId,
            Amount = 120.0m,
            Jwt = JwtToken,
        };

        // Act
        try
        {
            await service.DepositAsync(request);
        }
        catch (CustomerInformationNeededException ex)
        {
            // Assert
            Assert.IsNotNull(ex.Response);
            Assert.IsTrue(ex.Message.Contains("The anchor needs more information about the customer"));
            Assert.IsTrue(ex.Message.Contains("SEP-12"));
            Assert.IsFalse(ex.Message.Contains("Fields:"));
            throw;
        }
    }

    /// <summary>
    ///     Verifies that TransferServerException with inner exception works correctly.
    /// </summary>
    [TestMethod]
    public void TransferServerException_WithInnerException_WorksCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Transfer server error";

        // Act
        var exception = new TransferServerException(message, innerException);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.AreEqual(innerException, exception.InnerException);
    }

    /// <summary>
    ///     Verifies that constructor with HttpResilienceOptions works correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithResilienceOptions_CreatesService()
    {
        // Arrange & Act
        var service = new TransferServerService(
            ServiceAddress,
            null,
            "test-token",
            new Dictionary<string, string> { { "X-Custom", "value" } });

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    ///     Verifies that constructor with HttpResilienceOptions throws when address is empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithResilienceOptionsEmptyAddress_ThrowsArgumentException()
    {
        // Act
        _ = new TransferServerService("", resilienceOptions: null);
    }

    /// <summary>
    ///     Verifies that Dispose properly disposes internal HttpClient.
    /// </summary>
    [TestMethod]
    public void Dispose_WithoutInternalHttpClient_Succeeds()
    {
        // Arrange - Create service without providing HttpClient
        var service = new TransferServerService(ServiceAddress);

        // Act - Call Dispose twice to test idempotency (internal client not yet created)
        service.Dispose();
        service.Dispose();

        // Assert - No exception means success
        Assert.IsTrue(true);
    }

    /// <summary>
    ///     Verifies that Dispose properly disposes internal HttpClient when it was created.
    /// </summary>
    [TestMethod]
    public async Task Dispose_WithCreatedInternalHttpClient_DisposesClient()
    {
        // Arrange - Create service without providing HttpClient to trigger internal client creation
        // Use a port that will fail quickly
        var service = new TransferServerService(
            "http://127.0.0.1:1",
            null,
            "test-token");

        try
        {
            // Act - This will trigger GetOrCreateHttpClient() and create internal client
            // The request will fail because the port is invalid
            await service.InfoAsync();
        }
        catch
        {
            // Expected - the request will fail, but internal client was created
        }

        // Act - Dispose should now dispose the internal client
        service.Dispose();
        // Call again to test idempotency
        service.Dispose();

        // Assert - No exception means success
        Assert.IsTrue(true);
    }

    /// <summary>
    ///     Verifies that AddHeaders includes custom headers in request.
    /// </summary>
    [TestMethod]
    public async Task InfoAsync_WithCustomHeaders_IncludesHeadersInRequest()
    {
        // Arrange
        var content = await LoadTestDataAsync("info.json");
        var mockHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var customHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "X-Another-Header", "another-value" },
        };
        var service = new TransferServerService(ServiceAddress, httpClient, customHeaders);

        // Act
        await service.InfoAsync();

        // Assert
        Assert.IsNotNull(capture.Request);
        Assert.IsTrue(capture.Request.Headers.Contains("X-Custom-Header"));
        Assert.AreEqual("custom-value", capture.Request.Headers.GetValues("X-Custom-Header").First());
        Assert.IsTrue(capture.Request.Headers.Contains("X-Another-Header"));
    }

    /// <summary>
    ///     Verifies HandleForbiddenResponse handles JSON without type property gracefully.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_ForbiddenNoType_ThrowsHttpResponseException()
    {
        // Arrange
        var content = await LoadTestDataAsync("forbidden-no-type.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Jwt = JwtToken,
        };

        // Act & Assert - Should fall through to generic HTTP error handling
        await Assert.ThrowsExceptionAsync<HttpResponseException>(async () =>
            await service.DepositAsync(request));
    }

    /// <summary>
    ///     Verifies HandleForbiddenResponse handles malformed JSON gracefully.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_ForbiddenMalformedJson_ThrowsHttpResponseException()
    {
        // Arrange - Invalid JSON that will throw JsonException
        const string malformedJson = "{ invalid json }";
        using var httpClient = CreateMockHttpClient(malformedJson, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Jwt = JwtToken,
        };

        // Act & Assert - Should fall through to generic HTTP error handling
        await Assert.ThrowsExceptionAsync<HttpResponseException>(async () =>
            await service.DepositAsync(request));
    }

    /// <summary>
    ///     Verifies HandleForbiddenResponse handles empty type property gracefully.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_ForbiddenEmptyType_ThrowsHttpResponseException()
    {
        // Arrange
        var content = await LoadTestDataAsync("forbidden-empty-type.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Jwt = JwtToken,
        };

        // Act & Assert - Should fall through to generic HTTP error handling
        await Assert.ThrowsExceptionAsync<HttpResponseException>(async () =>
            await service.DepositAsync(request));
    }

    /// <summary>
    ///     Verifies HandleForbiddenResponse handles unknown type property gracefully.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_ForbiddenUnknownType_ThrowsHttpResponseException()
    {
        // Arrange
        var content = await LoadTestDataAsync("forbidden-unknown-type.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Jwt = JwtToken,
        };

        // Act & Assert - Should fall through to generic HTTP error handling
        await Assert.ThrowsExceptionAsync<HttpResponseException>(async () =>
            await service.DepositAsync(request));
    }

    /// <summary>
    ///     Verifies HandleForbiddenResponse handles empty response gracefully.
    /// </summary>
    [TestMethod]
    public async Task DepositAsync_ForbiddenEmptyResponse_ThrowsHttpResponseException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("", HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new DepositRequest
        {
            AssetCode = "USD",
            Account = AccountId,
            Jwt = JwtToken,
        };

        // Act & Assert - Should fall through to generic HTTP error handling
        await Assert.ThrowsExceptionAsync<HttpResponseException>(async () =>
            await service.DepositAsync(request));
    }

    /// <summary>
    ///     Verifies PatchTransactionAsync handles Forbidden response correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AuthenticationRequiredException))]
    public async Task PatchTransactionAsync_AuthenticationRequired_ThrowsException()
    {
        // Arrange
        var content = await LoadTestDataAsync("authentication-required.json");
        using var httpClient = CreateMockHttpClient(content, HttpStatusCode.Forbidden);
        var service = new TransferServerService(ServiceAddress, httpClient);
        var request = new PatchTransactionRequest
        {
            Id = "82fhs729f63dh0v4",
            Fields = new Dictionary<string, object>
            {
                { "dest", "1234567890" },
            },
            Jwt = JwtToken,
        };

        // Act
        await service.PatchTransactionAsync(request);
    }

    private class RequestCapture
    {
        public HttpRequestMessage? Request { get; set; }
    }
}