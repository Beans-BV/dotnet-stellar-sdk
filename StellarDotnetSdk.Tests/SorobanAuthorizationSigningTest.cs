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
using SorobanCredentials = StellarDotnetSdk.Operations.SorobanCredentials;
using SorobanDelegateSignature = StellarDotnetSdk.Operations.SorobanDelegateSignature;

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
            unsigned, keyPair, ValidUntil, network);

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
            unsigned, keyPair, ValidUntil, network, SorobanCredentialsVersion.V1);

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
        // CAP-0071-01: the delegate signs the same root-bound payload as the top-level account,
        // not a payload bound to its own address.
        Assert.AreEqual(new KeyPairEntrySigner(delegateKp).Sign(rootHash).ToXdrBase64(),
            cred.Delegates[0].Signature.ToXdrBase64());

        var decoded = SorobanAuthorizationEntry.FromXdr(signed.ToXdr());
        Assert.IsInstanceOfType(decoded.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntry_DefaultVersion_ProducesV2Credential()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")),
            SampleInvocation());

        // No version argument: the default is the address-bound V2 credential, matching the JS
        // reference SDK (v16). Pass V1 explicitly when targeting a network not yet on Protocol 27.
        var signed = SorobanAuthorization.AuthorizeEntry(unsigned, keyPair, ValidUntil, network);

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
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
            unsigned, signerKeyPair, ValidUntil, network);

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

        var kpA = KeyPair.Random();
        var kpB = KeyPair.Random();
        var dA = new KeyPairEntrySigner(kpA);
        var dB = new KeyPairEntrySigner(kpB);
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

        // Sorting must not mis-pair addresses and signatures, and every delegate signs the same
        // root-bound payload (CAP-0071-01).
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        foreach (var emitted in cred.Delegates)
        {
            var kp = ((ScAccountId)emitted.Address).InnerValue == kpA.AccountId ? kpA : kpB;
            Assert.AreEqual(new KeyPairEntrySigner(kp).Sign(rootHash).ToXdrBase64(),
                emitted.Signature.ToXdrBase64());
        }
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
    public void BuildAuthPreimageHash_NullArguments_ThrowArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAuthPreimageHash(null!, Nonce, ValidUntil, SampleInvocation()));
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAuthPreimageHash(Network.Public(), Nonce, ValidUntil, null!));
    }

    [TestMethod]
    public void BuildAddressAuthPreimageHash_NullArguments_ThrowArgumentNullException()
    {
        var network = Network.Public();
        var address = new ScAccountId(KeyPair.Random().AccountId);
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAddressAuthPreimageHash(null!, address, Nonce, ValidUntil, SampleInvocation()));
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, null!, Nonce, ValidUntil, SampleInvocation()));
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, null!));
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_NullDelegateElement_ThrowsArgumentException()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntryWithDelegates(
                unsigned, new KeyPairEntrySigner(rootKp), new ISorobanEntrySigner[] { null! }, ValidUntil, network));
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_DuplicateDelegateAddresses_ThrowsArgumentException()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var duplicated = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        // CAP-0071-01 forbids duplicate addresses in a delegates array; fail fast client-side
        // instead of letting the network reject the invocation.
        Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntryWithDelegates(
                unsigned, new KeyPairEntrySigner(rootKp),
                [new KeyPairEntrySigner(duplicated), new KeyPairEntrySigner(duplicated)],
                ValidUntil, network));
    }

    [TestMethod]
    public void BuildAddressAuthPreimageHash_MatchesCrossSdkKnownAnswerVector()
    {
        // Known-answer vector generated with js-stellar-sdk v16.0.0-rc.1 (dist-tag p27), the
        // Protocol 27 reference implementation. Identical inputs were fed both to raw
        // xdr.HashIdPreimage.envelopeTypeSorobanAuthorizationWithAddress and to the SDK's
        // buildAuthorizationEntryPreimage helper; both produced this hash. The vector covers the
        // V2 and the delegated signing payload alike — per CAP-0071-01 both use the same
        // WITH_ADDRESS preimage bound to the top-level address.
        // Regenerate both vectors with TestData/generate-p27-auth-kat.mjs.
        var network = Network.Public();
        var address = new ScAccountId("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

        var actual = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, address, Nonce, ValidUntil, SampleInvocation());

        Assert.AreEqual(
            "79b329f646bafd6ba1de82e2d470af9b9f324f4c3165d415456cc4f8847d5ea8",
            Convert.ToHexString(actual).ToLowerInvariant());
    }

    [TestMethod]
    public void BuildAuthPreimageHash_MatchesCrossSdkKnownAnswerVector()
    {
        // Legacy V1 vector from the same js-stellar-sdk v16.0.0-rc.1 generation run as above.
        var actual = SorobanAuthorization.BuildAuthPreimageHash(
            Network.Public(), Nonce, ValidUntil, SampleInvocation());

        Assert.AreEqual(
            "f9410dc7ac854c6f53e9f0ba1a905dcd2eaf368390231713e41145777562972d",
            Convert.ToHexString(actual).ToLowerInvariant());
    }

    [TestMethod]
    public void AuthorizeEntry_OnDelegatedEntry_SignsRootAndPreservesDelegates()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateAddress = new ScAccountId(KeyPair.Random().AccountId);
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
                [new SorobanDelegateSignature(delegateAddress, new SCString("placeholder"), [])]),
            SampleInvocation());

        // No forAddress: the signature goes to the root node; the delegate placeholder is preserved
        // and the credential type stays WITH_DELEGATES (the version parameter is ignored).
        var signed = SorobanAuthorization.AuthorizeEntry(unsigned, rootKp, ValidUntil, network);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(rootHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());
        Assert.AreEqual("placeholder", ((SCString)cred.Delegates[0].Signature).InnerValue);
    }

    [TestMethod]
    public void AuthorizeEntry_ForDelegateAddress_RoutesSignatureToNestedDelegateNode()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var outerAddress = new ScAccountId(KeyPair.Random().AccountId);
        var nestedKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var nestedAddress = new ScAccountId(nestedKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("root-placeholder")),
                [
                    new SorobanDelegateSignature(outerAddress, new SCString("outer-placeholder"),
                        [new SorobanDelegateSignature(nestedAddress, new SCString("nested-placeholder"), [])]),
                ]),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, nestedKp, ValidUntil, network, forAddress: nestedAddress);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        // The nested delegate signed the shared root-bound payload (CAP-0071-01).
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(nestedKp).Sign(rootHash).ToXdrBase64(),
            cred.Delegates[0].NestedDelegates[0].Signature.ToXdrBase64());
        // Root and outer-delegate signatures are preserved; root expiration is still bumped.
        Assert.AreEqual("root-placeholder", ((SCString)cred.AddressCredentials.Signature).InnerValue);
        Assert.AreEqual("outer-placeholder", ((SCString)cred.Delegates[0].Signature).InnerValue);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void AuthorizeEntry_ForUnknownAddress_Throws()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
                []),
            SampleInvocation());

        Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(unsigned, rootKp, ValidUntil, network,
                forAddress: new ScAccountId(KeyPair.Random().AccountId)));
    }

    [TestMethod]
    public void AuthorizeEntry_ForRootAddressOnPlainEntry_Signs()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")),
            SampleInvocation());

        // forAddress matching the (only) credential node of a plain address entry succeeds.
        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, keyPair, ValidUntil, network, forAddress: new ScAccountId(keyPair.AccountId));

        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
    }

    [TestMethod]
    public void AuthorizeEntry_ForAddressInMultipleDelegateArrays_SignsEveryMatchingNode()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var sharedKp = KeyPair.Random();
        var sharedAddress = new ScAccountId(sharedKp.AccountId);
        var outerA = new ScAccountId(KeyPair.Random().AccountId);
        var outerB = new ScAccountId(KeyPair.Random().AccountId);
        // Keep the top-level delegate array sorted so it serializes; the shared address appears as
        // a nested delegate under both outer delegates.
        var (first, second) = CompareAddress(outerA, outerB) < 0 ? (outerA, outerB) : (outerB, outerA);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("root-placeholder")),
                [
                    new SorobanDelegateSignature(first, new SCString("outer-placeholder"),
                        [new SorobanDelegateSignature(sharedAddress, new SCString("nested-placeholder"), [])]),
                    new SorobanDelegateSignature(second, new SCString("outer-placeholder"),
                        [new SorobanDelegateSignature(sharedAddress, new SCString("nested-placeholder"), [])]),
                ]),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, sharedKp, ValidUntil, network, forAddress: sharedAddress);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        var expected = new KeyPairEntrySigner(sharedKp).Sign(rootHash).ToXdrBase64();
        // Both matching nested nodes receive the signature; outer nodes and root are preserved.
        Assert.AreEqual(expected, cred.Delegates[0].NestedDelegates[0].Signature.ToXdrBase64());
        Assert.AreEqual(expected, cred.Delegates[1].NestedDelegates[0].Signature.ToXdrBase64());
        Assert.AreEqual("outer-placeholder", ((SCString)cred.Delegates[0].Signature).InnerValue);
        Assert.AreEqual("root-placeholder", ((SCString)cred.AddressCredentials.Signature).InnerValue);
    }

    [TestMethod]
    public void BuildAuthorizationEntryPreimageHash_PicksVariantByCredentialType()
    {
        var network = Network.Public();
        var address = new ScAccountId(KeyPair.Random().AccountId);
        var invocation = SampleInvocation();

        var v1Entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")), invocation);
        var v2Entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsV2(address, Nonce, 0, new SCString("")), invocation);
        var delegatedEntry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(address, Nonce, 0, new SCString("")), []),
            invocation);

        // V1 credentials select the legacy preimage; V2 and delegated credentials select the
        // address-bound preimage bound to the (root) credential address.
        CollectionAssert.AreEqual(
            SorobanAuthorization.BuildAuthPreimageHash(network, Nonce, ValidUntil, invocation),
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(v1Entry, ValidUntil, network));
        CollectionAssert.AreEqual(
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, invocation),
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(v2Entry, ValidUntil, network));
        CollectionAssert.AreEqual(
            SorobanAuthorization.BuildAddressAuthPreimageHash(network, address, Nonce, ValidUntil, invocation),
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(delegatedEntry, ValidUntil, network));
    }

    [TestMethod]
    public void BuildAuthorizationEntryPreimageHash_SourceAccountCredentials_Throws()
    {
        var entry = new SorobanAuthorizationEntry(new SorobanSourceAccountCredentials(), SampleInvocation());
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(entry, ValidUntil, Network.Public()));
        Assert.IsTrue(ex.Message.Contains("Source-account", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void BuildAuthorizationEntryPreimageHash_UnknownCredentialsSubtype_MessageIncludesTypeName()
    {
        var entry = new SorobanAuthorizationEntry(new UnknownCredentials(), SampleInvocation());

        // An unrecognized credential subtype must not be reported as source-account credentials;
        // the message must name the offending CLR type to aid diagnosis.
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(entry, ValidUntil, Network.Public()));
        Assert.IsTrue(ex.Message.Contains(nameof(UnknownCredentials)));
    }

    [TestMethod]
    public void AuthorizeEntry_UnknownCredentialsSubtype_MessageIncludesTypeName()
    {
        var entry = new SorobanAuthorizationEntry(new UnknownCredentials(), SampleInvocation());

        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(entry, KeyPair.Random(), ValidUntil, Network.Public()));
        Assert.IsTrue(ex.Message.Contains(nameof(UnknownCredentials)));
    }

    [TestMethod]
    public void BuildWithDelegatesEntry_BuildsSortedVoidSignedStructure()
    {
        var rootAddress = new ScAccountId(KeyPair.Random().AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("simulation-signature")),
            SampleInvocation());

        var a = new ScAccountId(KeyPair.Random().AccountId);
        var b = new ScAccountId(KeyPair.Random().AccountId);
        // Deliberately descending input order; the builder must sort ascending.
        var descending = CompareAddress(a, b) < 0 ? new ScAddress[] { b, a } : new ScAddress[] { a, b };

        var built = SorobanAuthorization.BuildWithDelegatesEntry(unsigned, descending, ValidUntil);

        var cred = (SorobanAddressCredentialsWithDelegates)built.Credentials;
        Assert.AreEqual(rootAddress.InnerValue, ((ScAccountId)cred.AddressCredentials.Address).InnerValue);
        Assert.AreEqual(Nonce, cred.AddressCredentials.Nonce);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.IsInstanceOfType(cred.AddressCredentials.Signature, typeof(SCVoid));
        Assert.AreEqual(2, cred.Delegates.Length);
        Assert.IsTrue(CompareAddress(cred.Delegates[0].Address, cred.Delegates[1].Address) < 0,
            "Delegates must be sorted by increasing address.");
        foreach (var d in cred.Delegates)
        {
            Assert.IsInstanceOfType(d.Signature, typeof(SCVoid));
            Assert.AreEqual(0, d.NestedDelegates.Length);
        }

        // The built structure must serialize (sorted, no duplicates).
        Assert.IsNotNull(built.ToXdr());
    }

    [TestMethod]
    public void BuildWithDelegatesEntry_DuplicateAddresses_Throws()
    {
        var rootAddress = new ScAccountId(KeyPair.Random().AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());
        var duplicated = new ScAccountId(KeyPair.Random().AccountId);

        Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.BuildWithDelegatesEntry(
                unsigned, new ScAddress[] { duplicated, duplicated }, ValidUntil));
    }

    [TestMethod]
    public void BuildWithDelegatesEntry_ThenIncrementalAuthorize_SignsAllParties()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var delegateAddress = new ScAccountId(delegateKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        // The full JS-parity incremental flow: build the unsigned delegated structure once with the
        // final expiration, then each party signs with the SAME validUntilLedgerSeq.
        var built = SorobanAuthorization.BuildWithDelegatesEntry(unsigned, [delegateAddress], ValidUntil);
        var rootSigned = SorobanAuthorization.AuthorizeEntry(built, rootKp, ValidUntil, network);
        var fullySigned = SorobanAuthorization.AuthorizeEntry(
            rootSigned, delegateKp, ValidUntil, network, forAddress: delegateAddress);

        var cred = (SorobanAddressCredentialsWithDelegates)fullySigned.Credentials;
        var sharedHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(sharedHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());
        Assert.AreEqual(new KeyPairEntrySigner(delegateKp).Sign(sharedHash).ToXdrBase64(),
            cred.Delegates[0].Signature.ToXdrBase64());
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void AuthorizeEntry_NullKeyPair_ThrowsWithSignerParamName()
    {
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(KeyPair.Random().AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        // The exception must reference the public API parameter ("signer"), not the parameter name
        // of the KeyPairEntrySigner constructor the key pair is forwarded to ("keyPair").
        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.AuthorizeEntry(unsigned, (KeyPair)null!, ValidUntil, Network.Public()));
        Assert.AreEqual("signer", ex.ParamName);
    }

    [TestMethod]
    public void AuthorizeEntry_ForAddressWithNullDelegateElement_ThrowsInvalidOperation()
    {
        var network = Network.Public();
        var signerKp = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(
                    new ScAccountId(KeyPair.Random().AccountId), Nonce, 0, new SCString("")),
                new SorobanDelegateSignature[] { null! }),
            SampleInvocation());

        // A null delegate element must surface the clear CAP-71 validation error during signature
        // routing, not a NullReferenceException.
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(unsigned, signerKp, ValidUntil, network,
                forAddress: new ScAccountId(signerKp.AccountId)));
        Assert.IsTrue(ex.Message.Contains("null"));
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

    private sealed class UnknownCredentials : SorobanCredentials
    {
    }
}