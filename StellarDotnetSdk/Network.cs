using System;
using System.Text;

namespace StellarDotnetSdk;

/// <summary>
///     Represents a Stellar network identified by its unique passphrase.
///     The passphrase is hashed to produce the network ID, which is used to distinguish
///     transactions on different networks (e.g., public mainnet vs. testnet).
/// </summary>
public class Network
{
    /// <summary>The passphrase for the Stellar public (mainnet) network.</summary>
    public const string PublicPassphrase = "Public Global Stellar Network ; September 2015";

    /// <summary>The passphrase for the Stellar test network.</summary>
    public const string TestnetPassphrase = "Test SDF Network ; September 2015";

    /// <summary>
    ///     Initializes a new <see cref="Network" /> instance with the given passphrase.
    /// </summary>
    /// <param name="networkPassphrase">The network passphrase used to generate the network ID.</param>
    public Network(string networkPassphrase)
    {
        NetworkPassphrase = networkPassphrase ??
                            throw new ArgumentNullException(nameof(networkPassphrase),
                                "networkPassphrase cannot be null");
    }

    /// <summary>Gets the passphrase that identifies this network.</summary>
    public string NetworkPassphrase { get; }

    /// <summary>Gets the SHA-256 hash of the network passphrase, used as the network ID in transaction signing.</summary>
    public byte[] NetworkId => Util.Hash(Encoding.UTF8.GetBytes(NetworkPassphrase));

    /// <summary>Gets or sets the currently active network. Must be set before signing transactions.</summary>
    public static Network? Current { get; private set; }

    /// <summary>Creates a new <see cref="Network" /> instance for the Stellar public (mainnet) network.</summary>
    /// <returns>A <see cref="Network" /> configured with the public network passphrase.</returns>
    public static Network Public()
    {
        return new Network(PublicPassphrase);
    }

    /// <summary>Creates a new <see cref="Network" /> instance for the Stellar test network.</summary>
    /// <returns>A <see cref="Network" /> configured with the testnet passphrase.</returns>
    public static Network Test()
    {
        return new Network(TestnetPassphrase);
    }

    /// <summary>Sets the current active network used for signing transactions.</summary>
    /// <param name="network">The network to use, or <c>null</c> to clear the current network.</param>
    public static void Use(Network? network)
    {
        Current = network;
    }

    /// <summary>Checks whether the given network is the Stellar public (mainnet) network.</summary>
    /// <param name="network">The network to check.</param>
    /// <returns><c>true</c> if the network uses the public passphrase; otherwise, <c>false</c>.</returns>
    public static bool IsPublicNetwork(Network network)
    {
        return network.NetworkPassphrase == PublicPassphrase;
    }

    /// <summary>Sets the current active network to the Stellar public (mainnet) network.</summary>
    public static void UsePublicNetwork()
    {
        Use(Public());
    }

    /// <summary>Sets the current active network to the Stellar test network.</summary>
    public static void UseTestNetwork()
    {
        Use(Test());
    }
}