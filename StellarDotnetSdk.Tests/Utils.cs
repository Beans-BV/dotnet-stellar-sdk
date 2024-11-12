using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using XdrTransactionResult = StellarDotnetSdk.Xdr.TransactionResult;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Tests;

public static class Utils
{
    /// <summary>
    ///     Gets the path to the JSON data file relative to the calling test.
    ///     By default, the JSON data files should have the same directory structure as their tests, but under /TestData.
    ///     For example, if the test is in Responses\Effects\LiquidityPoolEffectResponseTest, then the JSON file is located in
    ///     TestData\Responses\Effects\LiquidityPoolEffectResponseTest.
    /// </summary>
    /// <param name="jsonFilePath">
    ///     Path of the JSON file. If only the name of the file provided, then the file is located in
    ///     the default folder structure. Otherwise, provide the relative path of the file to the TestData directory.
    /// </param>
    /// <param name="testFilePath">
    ///     Leave it empty if the JSON file is located in the default location. Otherwise, it's should
    ///     be the relative path to the JSON file.
    /// </param>
    /// <returns>The relative path to the JSON data file.</returns>
    public static string GetTestDataPath(string jsonFilePath, [CallerFilePath] string testFilePath = "")
    {
        if (Path.GetDirectoryName(jsonFilePath) != "")
        {
            return Path.Combine("TestData", jsonFilePath);
        }
        // testFilePath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\Responses\Effects\LiquidityPoolEffectResponseTest.cs on Windows
        // testFileDirectoryPath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\Responses\Effects\ on Windows
        // AppContext.BaseDirectory would be /home/runner/work/dotnet-stellar-sdk/dotnet-stellar-sdk/StellarDotnetSdk.Tests/bin/Release/net8.0 on Linux
        // and C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\bin\Release\net8.0 on Windows
        // rootPath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\ on Windows
        var rootPath =
            Path.GetDirectoryName(
                Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppContext.BaseDirectory))));

        var testFileDirectoryPath = Path.GetDirectoryName(testFilePath);
        // Would be responses\Effects\
        var testRelativeDirectoryPath = Path.GetRelativePath(rootPath, testFileDirectoryPath);

        return Path.Combine("TestData", testRelativeDirectoryPath, jsonFilePath);
    }

    public static TransactionResult AssertResultOfType(string xdr, Type resultType, bool isSuccess)
    {
        var result = TransactionResult.FromXdrBase64(xdr);
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(1, failed.Results.Count);
        Assert.IsInstanceOfType(failed.Results[0], resultType);
        Assert.AreEqual(isSuccess, failed.Results[0].IsSuccess);
        return result;
    }

    public static string CreateTransactionResultXdr(OperationResult.OperationResultTr operationResultTr)
    {
        var transactionResult = new XdrTransactionResult
        {
            Ext = new XdrTransactionResult.TransactionResultExt(),
            FeeCharged = new Int64(100L),
            Result = new XdrTransactionResult.TransactionResultResult
            {
                Discriminant =
                {
                    InnerValue = TransactionResultCode.TransactionResultCodeEnum.txFAILED,
                },
                Results = new OperationResult[1],
            },
        };

        transactionResult.Result.Results[0] = new OperationResult
        {
            Tr = operationResultTr,
        };

        var outputStream = new XdrDataOutputStream();
        XdrTransactionResult.Encode(outputStream, transactionResult);
        return Convert.ToBase64String(outputStream.ToArray());
    }

    public static async Task CheckAndCreateAccountOnTestnet(string accountId)
    {
        using Server server = new("https://horizon-testnet.stellar.org");
        try
        {
            await server.Accounts.Account(accountId);
        }
        catch (HttpResponseException)
        {
            bool isSuccess;
            do
            {
                var fundResponse = await server.TestNetFriendBot.FundAccount(accountId).Execute();
                var result = TransactionResult.FromXdrBase64(fundResponse.ResultXdr);
                isSuccess = result.IsSuccess && result is TransactionResultSuccess;
            } while (!isSuccess);
        }
    }

    public static Server CreateTestServerWithContent(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient(content, statusCode);
        return new Server(uri, httpClient);
    }

    public static SorobanServer CreateTestSorobanServerWithContent(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://soroban-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient(content, statusCode);
        return new SorobanServer(uri, httpClient);
    }

    public static Server CreateTestServerWithHeaders(
        Dictionary<string, IEnumerable<string>> headers,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient("", statusCode, headers);
        return new Server(uri, httpClient);
    }

    public static async Task<Server> CreateTestServerWithJson(
        string pathToJson,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        var jsonPath = GetTestDataPath(pathToJson);
        var content = await File.ReadAllTextAsync(jsonPath);
        return CreateTestServerWithContent(content, statusCode, uri);
    }

    public static HttpClient CreateFakeHttpClient(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        IDictionary<string, IEnumerable<string>>? headers = null)
    {
        var mockFakeHttpMessageHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockFakeHttpMessageHandler.Object);

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = content != null ? new StringContent(content) : null,
        };

        httpResponseMessage.Headers.Add("X-Ratelimit-Limit", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Remaining", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Reset", "-1");
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        mockFakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponseMessage);

        return httpClient;
    }

    public abstract class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return await Task.FromResult(Send(request));
        }
    }
}