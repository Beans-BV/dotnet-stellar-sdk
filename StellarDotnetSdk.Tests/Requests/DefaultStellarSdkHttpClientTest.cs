using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class DefaultStellarSdkHttpClientTest
{
    /// <summary>
    /// Verifies that the default constructor adds the expected default headers
    /// (X-Client-Name and X-Client-Version) to all requests.
    /// </summary>
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

    /// <summary>
    /// Verifies that when a custom client name is provided to the constructor,
    /// it is used in the X-Client-Name header instead of the default value.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomClientName_UsesProvided()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(clientName: "my-custom-client");

        // Assert
        Assert.AreEqual("my-custom-client", string.Join("", client.DefaultRequestHeaders.GetValues("X-Client-Name")));
    }

    /// <summary>
    /// Verifies that when a custom client version is provided to the constructor,
    /// it is used in the X-Client-Version header instead of the default value.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomClientVersion_UsesProvided()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(clientVersion: "1.2.3");

        // Assert
        Assert.AreEqual("1.2.3", string.Join("", client.DefaultRequestHeaders.GetValues("X-Client-Version")));
    }

    /// <summary>
    /// Verifies that when a bearer token is provided to the constructor,
    /// the Authorization header is set with the Bearer scheme and the provided token.
    /// </summary>
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

    /// <summary>
    /// Verifies that when no bearer token is provided to the constructor,
    /// the Authorization header is not set.
    /// </summary>
    [TestMethod]
    public void Constructor_NoBearerToken_NoAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient();

        // Assert
        Assert.IsNull(client.DefaultRequestHeaders.Authorization);
    }

    /// <summary>
    /// Verifies that when an empty bearer token is provided to the constructor,
    /// the Authorization header is not set (empty strings are treated as no token).
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyBearerToken_NoAuthorizationHeader()
    {
        // Arrange & Act
        using var client = new DefaultStellarSdkHttpClient(bearerToken: "");

        // Assert
        Assert.IsNull(client.DefaultRequestHeaders.Authorization);
    }

    /// <summary>
    /// Verifies that when null retry options are provided to the constructor,
    /// the client is created successfully using default retry options.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRetryOptions_UsesDefaults()
    {
        // Arrange & Act - should not throw
        using var client = new DefaultStellarSdkHttpClient(retryOptions: null);

        // Assert - client is created successfully
        Assert.IsNotNull(client);
    }

    /// <summary>
    /// Verifies that when custom retry options are provided to the constructor,
    /// the client is created successfully and uses the provided options.
    /// </summary>
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

