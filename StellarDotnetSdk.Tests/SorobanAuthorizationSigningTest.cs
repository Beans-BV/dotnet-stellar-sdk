using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCMap = StellarDotnetSdk.Soroban.SCMap;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using SorobanAddressCredentials = StellarDotnetSdk.Operations.SorobanAddressCredentials;
using SorobanAddressCredentialsWithDelegates = StellarDotnetSdk.Operations.SorobanAddressCredentialsWithDelegates;
using SorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using SorobanAuthorizedInvocation = StellarDotnetSdk.Operations.SorobanAuthorizedInvocation;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SorobanAuthorizationSigningTest
{
    private const long Nonce = 1234567890L;
    private const uint ValidUntil = 2000;

    private static SorobanAuthorizedInvocation SampleInvocation()
    {
        var contract = new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        var fn = new SorobanAuthorizedContractFunction(
            new InvokeContractHostFunction(contract, new SCSymbol("hello"), [new SCString("world")]));
        return new SorobanAuthorizedInvocation(fn, []);
    }

    [TestMethod]
    public void PreimageHash_V2BindsAddress_DiffersFromV1AndIs32Bytes()
    {
        var network = Network.Public();
        var address = new ScAccountId(KeyPair.Random().AccountId);
        var invocation = SampleInvocation();

        var v2 = SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, invocation);
        var v1 = SorobanAuthorization.BuildAuthPreimageHash(network, Nonce, ValidUntil, invocation);

        Assert.AreEqual(32, v2.Length);
        Assert.AreEqual(32, v1.Length);
        CollectionAssert.AreNotEqual(v1, v2);
    }

    [TestMethod]
    public void KeyPairEntrySigner_ProducesVerifiableStandardSignatureScVal()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var invocation = SampleInvocation();
        var payloadHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, address, Nonce, ValidUntil, invocation);

        var signer = new KeyPairEntrySigner(keyPair);
        var sigVal = (SCVec)signer.Sign(payloadHash);

        Assert.AreEqual(1, sigVal.InnerValue.Length);
        var map = (SCMap)sigVal.InnerValue[0];
        Assert.AreEqual(2, map.Entries.Length);
        Assert.AreEqual("public_key", ((SCSymbol)map.Entries[0].Key).InnerValue);
        Assert.AreEqual("signature", ((SCSymbol)map.Entries[1].Key).InnerValue);
        var publicKey = ((SCBytes)map.Entries[0].Value).InnerValue;
        var signature = ((SCBytes)map.Entries[1].Value).InnerValue;
        Assert.AreEqual(32, publicKey.Length);
        Assert.AreEqual(64, signature.Length);
        Assert.IsTrue(keyPair.Verify(payloadHash, signature));
    }

    [TestMethod]
    public void AuthorizeEntry_WithKeyPair_ProducesV2CredentialThatRoundTrips()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, keyPair, ValidUntil, network, SorobanCredentialsVersion.V2);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
        var cred = (SorobanAddressCredentialsV2)signed.Credentials;
        Assert.AreEqual(ValidUntil, cred.SignatureExpirationLedger);
        Assert.AreEqual(Nonce, cred.Nonce);

        var expectedHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, address, Nonce, ValidUntil, unsigned.RootInvocation);
        var expectedSig = new KeyPairEntrySigner(keyPair).Sign(expectedHash);
        Assert.AreEqual(expectedSig.ToXdrBase64(), cred.Signature.ToXdrBase64());

        var decoded = SorobanAuthorizationEntry.FromXdr(signed.ToXdr());
        Assert.IsInstanceOfType(decoded.Credentials, typeof(SorobanAddressCredentialsV2));
    }

    [TestMethod]
    public void AuthorizeEntry_WithV1Version_ProducesV1Credential()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, keyPair, ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentials));
        Assert.IsNotInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
        var expectedHash = SorobanAuthorization.BuildAuthPreimageHash(
            network, Nonce, ValidUntil, unsigned.RootInvocation);
        var expectedSig = new KeyPairEntrySigner(keyPair).Sign(expectedHash);
        Assert.AreEqual(expectedSig.ToXdrBase64(),
            ((SorobanAddressCredentials)signed.Credentials).Signature.ToXdrBase64());
    }

    [TestMethod]
    public void AuthorizeEntry_WithSourceAccountCredentials_Throws()
    {
        var entry = new SorobanAuthorizationEntry(new SorobanSourceAccountCredentials(), SampleInvocation());
        Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(entry, KeyPair.Random(), ValidUntil, Network.Public()));
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_ProducesDelegatedCredentialThatRoundTrips()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), [new KeyPairEntrySigner(delegateKp)],
            ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.AreEqual(1, cred.Delegates.Length);
        Assert.AreEqual(delegateKp.AccountId, ((ScAccountId)cred.Delegates[0].Address).InnerValue);
        Assert.AreEqual(0, cred.Delegates[0].NestedDelegates.Length);

        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(rootHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());

        var decoded = SorobanAuthorizationEntry.FromXdr(signed.ToXdr());
        Assert.IsInstanceOfType(decoded.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntry_DefaultVersion_ProducesV1Credential()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")),
            SampleInvocation());

        // No version argument: the default must be the universally-accepted V1 credential, because
        // V2 is rejected by pre-Protocol-27 networks (V2 is opt-in until P27 is live everywhere).
        var signed = SorobanAuthorization.AuthorizeEntry(unsigned, keyPair, ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentials));
        Assert.IsNotInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
    }

    [TestMethod]
    public void AuthorizeEntry_TakesCredentialAddressFromEntry_NotSigner()
    {
        var network = Network.Public();
        var entryKeyPair = KeyPair.Random();
        var signerKeyPair = KeyPair.Random();
        var entryAddress = new ScAccountId(entryKeyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(entryAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        // Matching the Java and JS SDKs: the credential address is taken from the entry, not the
        // signer. Signing with a mismatched key does not throw; the on-chain __check_auth rejects it.
        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, signerKeyPair, ValidUntil, network, SorobanCredentialsVersion.V2);

        var cred = (SorobanAddressCredentialsV2)signed.Credentials;
        Assert.AreEqual(entryKeyPair.AccountId, ((ScAccountId)cred.Address).InnerValue);
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_EmitsDelegatesSortedByAddress()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        var dA = new KeyPairEntrySigner(KeyPair.Random());
        var dB = new KeyPairEntrySigner(KeyPair.Random());
        // Deliberately pass the delegates in descending address order so the helper has to reorder them.
        var descending = CompareAddress(dA.SignerAddress, dB.SignerAddress) < 0
            ? new ISorobanEntrySigner[] { dB, dA }
            : new ISorobanEntrySigner[] { dA, dB };

        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), descending, ValidUntil, network);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(2, cred.Delegates.Length);
        Assert.IsTrue(
            CompareAddress(cred.Delegates[0].Address, cred.Delegates[1].Address) < 0,
            "Delegates must be emitted sorted by increasing address.");
    }

    [TestMethod]
    public void BuildAddressAuthPreimageHash_MatchesIndependentlyAssembledV2Preimage()
    {
        var network = Network.Public();
        var address = new ScAccountId("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");
        var invocation = SampleInvocation();

        var actual =
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, invocation);

        // Re-assemble the ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS preimage byte-for-byte using
        // primitive XDR writes, independent of the generated HashIDPreimage encoder. This locks the
        // discriminant value and field order (networkID, nonce, expiration, address, invocation) — the
        // parts a schema regeneration or refactor could silently change.
        var stream = new XdrDataOutputStream();
        stream.WriteInt(10); // ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS
        stream.Write(network.NetworkId); // Hash networkID (opaque[32])
        stream.WriteLong(Nonce); // int64 nonce
        stream.WriteUInt(ValidUntil); // uint32 signatureExpirationLedger
        SCAddress.Encode(stream, address.ToXdr()); // SCAddress address
        StellarDotnetSdk.Xdr.SorobanAuthorizedInvocation.Encode(stream, invocation.ToXdr());
        var expected = SHA256.HashData(stream.ToArray());

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void BuildAuthPreimageHash_MatchesIndependentlyAssembledV1Preimage()
    {
        var network = Network.Public();
        var invocation = SampleInvocation();

        var actual = SorobanAuthorization.BuildAuthPreimageHash(network, Nonce, ValidUntil, invocation);

        // Independent re-assembly of the legacy ENVELOPE_TYPE_SOROBAN_AUTHORIZATION preimage (no address).
        var stream = new XdrDataOutputStream();
        stream.WriteInt(9); // ENVELOPE_TYPE_SOROBAN_AUTHORIZATION
        stream.Write(network.NetworkId);
        stream.WriteLong(Nonce);
        stream.WriteUInt(ValidUntil);
        StellarDotnetSdk.Xdr.SorobanAuthorizedInvocation.Encode(stream, invocation.ToXdr());
        var expected = SHA256.HashData(stream.ToArray());

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    [Ignore(
        "Add a cross-SDK known-answer vector (expected SHA-256 from JS/Rust) for the V2 WITH_ADDRESS preimage once a Protocol 27 reference SDK publishes one. Tracked by issue #186.")]
    public void BuildAddressAuthPreimageHash_MatchesCrossSdkKnownAnswerVector()
    {
        // Intentionally empty until an external P27 reference vector is available.
    }

    [TestMethod]
    [Ignore(
        "Enable after Protocol 27 Testnet upgrade (2026-06-18): submit a V2-signed InvokeHostFunction and assert success. Tracked by issue #186 AC4.")]
    public void AuthorizeEntry_AgainstP27Testnet_SubmitsSuccessfully()
    {
        // Intentionally empty until P27 testnet is live (2026-06-18).
    }

    private static byte[] AddressXdrBytes(ScAddress address)
    {
        var stream = new XdrDataOutputStream();
        SCAddress.Encode(stream, address.ToXdr());
        return stream.ToArray();
    }

    private static int CompareAddress(ScAddress a, ScAddress b)
    {
        var x = AddressXdrBytes(a);
        var y = AddressXdrBytes(b);
        var min = Math.Min(x.Length, y.Length);
        for (var i = 0; i < min; i++)
        {
            if (x[i] != y[i])
            {
                return x[i].CompareTo(y[i]);
            }
        }

        return x.Length.CompareTo(y.Length);
    }
}