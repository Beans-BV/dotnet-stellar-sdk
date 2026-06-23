using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     End-to-end resilience tests using a real <see cref="Server" /> over a scripted handler.
///     Verifies the full pipeline retries transient failures and surfaces the deserialized response.
/// </summary>
[TestClass]
public class ServerResilienceIntegrationTests
{
    /// <summary>
    ///     Verifies the full preset wiring end-to-end: a Server backed by a DefaultStellarSdkHttpClient
    ///     configured with the ForHorizon preset retries a 429 and returns the successful deserialized
    ///     root response on the follow-up call.
    /// </summary>
    [TestMethod]
    public async Task Server_ForHorizon_RetriesTooManyRequestsThenReturnsRoot()
    {
        var rootJson = File.ReadAllText(Utils.GetTestDataPath("Responses/root.json"));
        var scripted = new ScriptedHttpMessageHandler(
            ScriptedHttpMessageHandler.Status(HttpStatusCode.TooManyRequests, "0"),
            ScriptedHttpMessageHandler.Ok(rootJson));

        var resilience = HttpResilienceOptionsPresets.ForHorizon();
        resilience.BaseDelay = TimeSpan.FromMilliseconds(1);
        resilience.MaxDelay = TimeSpan.FromMilliseconds(10);
        resilience.UseJitter = false;

        // Go through DefaultStellarSdkHttpClient so the test exercises the real preset wiring:
        // HasAnyResilienceFeatureEnabled must detect the preset and wrap the scripted handler.
        using var httpClient = new DefaultStellarSdkHttpClient(resilienceOptions: resilience, innerHandler: scripted);
        using var server = new Server("https://horizon-testnet.stellar.org/", httpClient);

        var root = await server.RootAsync();

        Assert.IsNotNull(root);
        Assert.AreEqual(24, root.CurrentProtocolVersion);
        Assert.AreEqual(2, scripted.CallCount);
    }
}