using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

        Assert.IsNotNull(response.WithdrawAssets);
        Assert.IsTrue(response.WithdrawAssets.ContainsKey("USD"));
        var usdWithdraw = response.WithdrawAssets["USD"];
        Assert.IsTrue(usdWithdraw.Enabled);
        Assert.IsTrue(usdWithdraw.AuthenticationRequired);
        Assert.IsNotNull(usdWithdraw.Types);
        Assert.AreEqual(2, usdWithdraw.Types.Count);
        Assert.IsTrue(usdWithdraw.Types.ContainsKey("bank_account"));
        Assert.IsTrue(usdWithdraw.Types.ContainsKey("cash"));

        Assert.IsFalse(response.WithdrawAssets["ETH"].Enabled);

        Assert.IsNotNull(response.WithdrawExchangeAssets);
        Assert.IsTrue(response.WithdrawExchangeAssets.ContainsKey("USD"));

        Assert.IsNotNull(response.FeeInfo);
        Assert.IsFalse(response.FeeInfo.Enabled);

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

        var secondTx = response.Transactions[1];
        Assert.AreEqual("52fys79f63dh3v2", secondTx.Id);
        Assert.AreEqual("deposit-exchange", secondTx.Kind);
        Assert.AreEqual("iso4217:BRL", secondTx.AmountInAsset);
        Assert.AreEqual("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN",
            secondTx.AmountOutAsset);

        var thirdTx = response.Transactions[2];
        Assert.AreEqual("withdrawal", thirdTx.Kind);
        Assert.AreEqual("completed", thirdTx.Status);
        Assert.IsNotNull(thirdTx.Refunds);
        Assert.AreEqual(10m, thirdTx.Refunds.AmountRefunded);
        Assert.AreEqual(5m, thirdTx.Refunds.AmountFee);
        Assert.AreEqual(1, thirdTx.Refunds.Payments.Count);
        Assert.AreEqual("stellar", thirdTx.Refunds.Payments[0].IdType);

        var fourthTx = response.Transactions[3];
        Assert.AreEqual("deposit", fourthTx.Kind);
        Assert.IsNotNull(fourthTx.Refunds);
        Assert.AreEqual("external", fourthTx.Refunds.Payments[0].IdType);

        var fifthTx = response.Transactions[4];
        Assert.AreEqual("pending_transaction_info_update", fifthTx.Status);
        Assert.IsNotNull(fifthTx.RequiredInfoMessage);
        Assert.IsNotNull(fifthTx.RequiredInfoUpdates);
        Assert.IsTrue(fifthTx.RequiredInfoUpdates.ContainsKey("dest"));
        Assert.AreEqual("your bank account number", fifthTx.RequiredInfoUpdates["dest"].Description);

        var sixthTx = response.Transactions[5];
        Assert.AreEqual("withdrawal-exchange", sixthTx.Kind);
        Assert.AreEqual("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN", sixthTx.AmountInAsset);
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

    private class RequestCapture
    {
        public HttpRequestMessage? Request { get; set; }
    }
}