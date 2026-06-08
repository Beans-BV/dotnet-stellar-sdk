using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

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
            new InvokeContractHostFunction(contract, new SCSymbol("hello"), new SCVal[] { new SCString("world") }));
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
        var payloadHash =
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, invocation);

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

        var signed = SorobanAuthorization.AuthorizeEntry(unsigned, keyPair, ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
        var cred = (SorobanAddressCredentialsV2)signed.Credentials;
        Assert.AreEqual(ValidUntil, cred.SignatureExpirationLedger);
        Assert.AreEqual(Nonce, cred.Nonce);

        var expectedHash =
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil,
                unsigned.RootInvocation);
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

        var signed =
            SorobanAuthorization.AuthorizeEntry(unsigned, keyPair, ValidUntil, network, SorobanCredentialsVersion.V1);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentials));
        Assert.IsNotInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
        var expectedHash =
            SorobanAuthorization.BuildAuthPreimageHash(network, Nonce, ValidUntil, unsigned.RootInvocation);
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
            unsigned, new KeyPairEntrySigner(rootKp), new ISorobanEntrySigner[] { new KeyPairEntrySigner(delegateKp) },
            ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.AreEqual(1, cred.Delegates.Length);
        Assert.AreEqual(delegateKp.AccountId, ((ScAccountId)cred.Delegates[0].Address).InnerValue);
        Assert.AreEqual(0, cred.Delegates[0].NestedDelegates.Length);

        var rootHash =
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, rootAddress, Nonce, ValidUntil,
                unsigned.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(rootHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());

        var decoded = SorobanAuthorizationEntry.FromXdr(signed.ToXdr());
        Assert.IsInstanceOfType(decoded.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    [Ignore(
        "Enable after Protocol 27 Testnet upgrade (2026-06-18): submit a V2-signed InvokeHostFunction and assert success. Tracked by issue #186 AC4.")]
    public void AuthorizeEntry_AgainstP27Testnet_SubmitsSuccessfully()
    {
        // Intentionally empty until P27 testnet is live (2026-06-18).
    }
}