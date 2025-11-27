using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class DefaultStellarSdkHttpClientTest
{
    [TestMethod]
    public void Constructor_DefaultHeaders_Added()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient();

        // Assert
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Name"));
        Assert.IsTrue(client.DefaultRequestHeaders.Contains("X-Client-Version"));
        Assert.AreEqual("stellar-dotnet-sdk", string.Join("", client.DefaultRequestHeaders.GetValues("X-Client-Name")));
    }

    [TestMethod]
    public void Constructor_CustomClientName_UsesProvided()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(clientName: "my-custom-client");

        // Assert
        Assert.AreEqual("my-custom-client", string.Join("", client.DefaultRequestHeaders.GetValues("X-Client-Name")));
    }

    [TestMethod]
    public void Constructor_CustomClientVersion_UsesProvided()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(clientVersion: "1.2.3");

        // Assert
        Assert.AreEqual("1.2.3", string.Join("", client.DefaultRequestHeaders.GetValues("X-Client-Version")));
    }

    [TestMethod]
    public void Constructor_BearerToken_AuthorizationHeaderSet()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(bearerToken: "test-token-123");

        // Assert
        Assert.IsNotNull(client.DefaultRequestHeaders.Authorization);
        Assert.AreEqual("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
        Assert.AreEqual("test-token-123", client.DefaultRequestHeaders.Authorization.Parameter);
    }

    [TestMethod]
    public void Constructor_NoBearerToken_NoAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient();

        // Assert
        Assert.IsNull(client.DefaultRequestHeaders.Authorization);
    }

    [TestMethod]
    public void Constructor_EmptyBearerToken_NoAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(bearerToken: "");

        // Assert
        Assert.IsNull(client.DefaultRequestHeaders.Authorization);
    }

    [TestMethod]
    public void Constructor_NullRetryOptions_UsesDefaults()
    {
        // Arrange & Act - should not throw
        using var client = new DefaultStellarSdkHttpClient(retryOptions: null);

        // Assert - client is created successfully
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_CustomRetryOptions_Accepted()
    {
        // Arrange
        var options = new HttpRetryOptions { MaxRetryCount = 5 };

        // Act - should not throw
        using var client = new DefaultStellarSdkHttpClient(retryOptions: options);

        // Assert - client is created successfully
        Assert.IsNotNull(client);
    }
}

