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

        // Explicit V2 upgrades the legacy entry to the address-bound credential.
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
    public void AuthorizeEntry_WithSourceAccountCredentials_ReturnsEntryUnchanged()
    {
        // Source-account credentials are authorized by the transaction source-account signature, so
        // AuthorizeEntry is a no-op and returns the entry unchanged (matching the JS/Java SDKs). This
        // lets callers authorize every entry from a simulation result without special-casing the mix.
        var entry = new SorobanAuthorizationEntry(new SorobanSourceAccountCredentials(), SampleInvocation());

        var result = SorobanAuthorization.AuthorizeEntry(entry, KeyPair.Random(), ValidUntil, Network.Public());

        Assert.AreSame(entry, result);
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
    public void AuthorizeEntry_DefaultVersion_PreservesInputVariant()
    {
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var address = new ScAccountId(keyPair.AccountId);

        // No version argument: the default preserves the entry's existing variant, matching the JS
        // reference SDK whose authorizeEntry never changes the credential type. A V1 entry stays V1...
        var v1Entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(address, Nonce, 0, new SCString("")), SampleInvocation());
        var signedV1 = SorobanAuthorization.AuthorizeEntry(v1Entry, keyPair, ValidUntil, network);
        Assert.IsInstanceOfType(signedV1.Credentials, typeof(SorobanAddressCredentials));
        Assert.IsNotInstanceOfType(signedV1.Credentials, typeof(SorobanAddressCredentialsV2));

        // ...and a V2 entry stays V2.
        var v2Entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsV2(address, Nonce, 0, new SCString("")), SampleInvocation());
        var signedV2 = SorobanAuthorization.AuthorizeEntry(v2Entry, keyPair, ValidUntil, network);
        Assert.IsInstanceOfType(signedV2.Credentials, typeof(SorobanAddressCredentialsV2));
        // Preserve must also sign the V2 address-bound payload, not merely keep the credential type:
        // guards against a regression that preserved the type but signed the legacy (V1) payload.
        var v2Hash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, address, Nonce, ValidUntil, v2Entry.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(keyPair).Sign(v2Hash).ToXdrBase64(),
            ((SorobanAddressCredentialsV2)signedV2.Credentials).Signature.ToXdrBase64());
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

        // Variant-agnostic: the default preserves the entry's V1 credential; assert on the shared base.
        var cred = (SorobanAddressCredentialsBase)signed.Credentials;
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
        var descending = SorobanAuthTestHelpers.CompareAddressXdr(dA.SignerAddress, dB.SignerAddress) < 0
            ? new ISorobanEntrySigner[] { dB, dA }
            : new ISorobanEntrySigner[] { dA, dB };

        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), descending, ValidUntil, network);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(2, cred.Delegates.Length);
        Assert.IsTrue(
            SorobanAuthTestHelpers.CompareAddressXdr(cred.Delegates[0].Address, cred.Delegates[1].Address) < 0,
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
    public void AuthorizeEntryWithDelegates_NullSignerAddress_ThrowsArgumentExceptionWithoutSigning()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        // A custom signer that violates the ISorobanEntrySigner contract by returning a null SignerAddress
        // must be rejected with a clear ArgumentException naming the bad input (not an opaque
        // NullReferenceException), and before any (possibly interactive) signer is invoked.
        var badSigner = new FixedAddressSigner(null!, new SCString("sig"));
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntryWithDelegates(
                unsigned, new KeyPairEntrySigner(rootKp), new ISorobanEntrySigner[] { badSigner }, ValidUntil,
                network));
        Assert.AreEqual("delegateSigners", ex.ParamName);
        Assert.AreEqual(0, badSigner.SignCallCount);
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
                [new SorobanDelegateSignature(delegateAddress, new SCVoid(), [])]),
            SampleInvocation());

        // No forAddress: the signature goes to the root node; the unsigned (SCVoid) delegate placeholder
        // is preserved and the credential type stays WITH_DELEGATES (the version parameter is ignored).
        var signed = SorobanAuthorization.AuthorizeEntry(unsigned, rootKp, ValidUntil, network);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(rootHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());
        Assert.IsInstanceOfType(cred.Delegates[0].Signature, typeof(SCVoid));
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
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCVoid()),
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
        // Root (still an unsigned placeholder) and the outer-delegate signature are preserved; the root
        // expiration is still bumped.
        Assert.IsInstanceOfType(cred.AddressCredentials.Signature, typeof(SCVoid));
        Assert.AreEqual("outer-placeholder", ((SCString)cred.Delegates[0].Signature).InnerValue);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void AuthorizeEntry_ForUnknownAddress_Throws()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        // Root is an unsigned (SCVoid) placeholder so the expiration guard does not pre-empt the
        // unknown-address check: with no delegate nodes and a forAddress matching neither the root nor
        // any delegate, the "no credential node" path is what must fire.
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCVoid()),
                []),
            SampleInvocation());

        // forAddress is an argument that doesn't match this entry → ArgumentException(nameof(forAddress)).
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntry(unsigned, rootKp, ValidUntil, network,
                forAddress: new ScAccountId(KeyPair.Random().AccountId)));
        Assert.AreEqual("forAddress", ex.ParamName);
    }

    [TestMethod]
    public void AuthorizeEntry_ForUnknownAddressOnPlainEntry_ThrowsArgumentException()
    {
        // The address-credentials path (not the delegated path): a forAddress that matches the single
        // credential node's address is required; anything else is an invalid argument.
        var network = Network.Public();
        var keyPair = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(keyPair.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntry(unsigned, keyPair, ValidUntil, network,
                forAddress: new ScAccountId(KeyPair.Random().AccountId)));
        Assert.AreEqual("forAddress", ex.ParamName);
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_OnAlreadyDelegatedEntry_ThrowsArgumentException()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegated = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
                []),
            SampleInvocation());

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntryWithDelegates(
                delegated, new KeyPairEntrySigner(rootKp), [], ValidUntil, network));
        Assert.AreEqual("entry", ex.ParamName);
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_OnSourceAccountEntry_ThrowsArgumentException()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var entry = new SorobanAuthorizationEntry(new SorobanSourceAccountCredentials(), SampleInvocation());

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntryWithDelegates(
                entry, new KeyPairEntrySigner(rootKp), [], ValidUntil, network));
        Assert.AreEqual("entry", ex.ParamName);
    }

    [TestMethod]
    public void BuildWithDelegatesEntry_OnAlreadyDelegatedEntry_ThrowsArgumentException()
    {
        var rootKp = KeyPair.Random();
        var delegated = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
                []),
            SampleInvocation());

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.BuildWithDelegatesEntry(
                delegated, [new ScAccountId(KeyPair.Random().AccountId)], ValidUntil));
        Assert.AreEqual("entry", ex.ParamName);
    }

    [TestMethod]
    public void BuildWithDelegatesEntry_OnSourceAccountEntry_ThrowsArgumentException()
    {
        var entry = new SorobanAuthorizationEntry(new SorobanSourceAccountCredentials(), SampleInvocation());

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.BuildWithDelegatesEntry(
                entry, [new ScAccountId(KeyPair.Random().AccountId)], ValidUntil));
        Assert.AreEqual("entry", ex.ParamName);
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

        // Default version preserves the entry's V1 variant.
        Assert.IsInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentials));
        Assert.IsNotInstanceOfType(signed.Credentials, typeof(SorobanAddressCredentialsV2));
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
        var (first, second) = SorobanAuthTestHelpers.CompareAddressXdr(outerA, outerB) < 0
            ? (outerA, outerB)
            : (outerB, outerA);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCVoid()),
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
        Assert.IsInstanceOfType(cred.AddressCredentials.Signature, typeof(SCVoid));
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
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(entry, ValidUntil, Network.Public()));
        Assert.AreEqual("entry", ex.ParamName);
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
        var descending = SorobanAuthTestHelpers.CompareAddressXdr(a, b) < 0
            ? new ScAddress[] { b, a }
            : new ScAddress[] { a, b };

        var built = SorobanAuthorization.BuildWithDelegatesEntry(unsigned, descending, ValidUntil);

        var cred = (SorobanAddressCredentialsWithDelegates)built.Credentials;
        Assert.AreEqual(rootAddress.InnerValue, ((ScAccountId)cred.AddressCredentials.Address).InnerValue);
        Assert.AreEqual(Nonce, cred.AddressCredentials.Nonce);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        Assert.IsInstanceOfType(cred.AddressCredentials.Signature, typeof(SCVoid));
        Assert.AreEqual(2, cred.Delegates.Length);
        Assert.IsTrue(
            SorobanAuthTestHelpers.CompareAddressXdr(cred.Delegates[0].Address, cred.Delegates[1].Address) < 0,
            "Delegates must be sorted by increasing address.");
        foreach (var d in cred.Delegates)
        {
            Assert.IsInstanceOfType(d.Signature, typeof(SCVoid));
            Assert.AreEqual(0, d.NestedDelegates.Length);
        }

        // The built structure must serialize AND decode back to the same sorted, void-signed shape
        // (a real XDR round-trip, not just a non-null check).
        var decoded = (SorobanAddressCredentialsWithDelegates)
            SorobanAuthorizationEntry.FromXdr(built.ToXdr()).Credentials;
        Assert.AreEqual(2, decoded.Delegates.Length);
        Assert.IsTrue(
            SorobanAuthTestHelpers.CompareAddressXdr(decoded.Delegates[0].Address, decoded.Delegates[1].Address) < 0,
            "Delegates must remain sorted by increasing address after a serialize/deserialize round-trip.");
        foreach (var d in decoded.Delegates)
        {
            Assert.IsInstanceOfType(d.Signature, typeof(SCVoid));
        }
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
                    new ScAccountId(KeyPair.Random().AccountId), Nonce, 0, new SCVoid()),
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
    public void AuthorizeEntry_DelegateSignWithExpirationDifferingFromSignedRoot_Throws()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var delegateAddress = new ScAccountId(delegateKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        // Root + delegate are signed committing to expiration 1000.
        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), [new KeyPairEntrySigner(delegateKp)], 1000, network);

        // Re-signing only the delegate with a DIFFERENT expiration (2000) would rewrite the root
        // expiration while keeping the root signature taken over 1000, silently producing an entry the
        // network rejects. The guard fails fast instead.
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(signed, delegateKp, 2000, network, forAddress: delegateAddress));
        Assert.IsTrue(ex.Message.Contains("expiration", StringComparison.OrdinalIgnoreCase));

        // Re-signing the delegate with the SAME expiration (1000) is the correct flow and must succeed.
        var reSigned = SorobanAuthorization.AuthorizeEntry(
            signed, delegateKp, 1000, network, forAddress: delegateAddress);
        Assert.IsInstanceOfType(reSigned.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntry_RootSignWithExpirationDifferingFromSignedDelegates_Throws()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());

        // Root + delegate are signed committing to expiration 1000.
        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), [new KeyPairEntrySigner(delegateKp)], 1000, network);

        // Re-signing only the ROOT (no forAddress) with a DIFFERENT expiration (2000) bumps the root
        // expiration while preserving the delegate signature taken over 1000, silently producing an
        // entry the network rejects. The guard fails fast — symmetric to the delegate-side guard above.
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(signed, rootKp, 2000, network));
        Assert.IsTrue(ex.Message.Contains("expiration", StringComparison.OrdinalIgnoreCase));

        // Re-signing the root with the SAME expiration (1000) is the correct flow and must succeed.
        var reSigned = SorobanAuthorization.AuthorizeEntry(signed, rootKp, 1000, network);
        Assert.IsInstanceOfType(reSigned.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntry_RootSignViaExplicitForAddress_WithExpirationDifferingFromSignedDelegates_Throws()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        // Root + delegate are signed committing to expiration 1000.
        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), [new KeyPairEntrySigner(delegateKp)], 1000, network);

        // Targeting the root by its OWN address (forAddress: rootAddress) re-signs the root exactly like
        // the forAddress-omitted form does. The expiration guard must therefore fire here too: re-signing
        // the root over 2000 bumps the root expiration while preserving the delegate signature taken over
        // 1000, silently producing an entry the network rejects. (Regression: the guard previously keyed on
        // "forAddress is null" and was bypassed by passing the root address explicitly.)
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(signed, rootKp, 2000, network, forAddress: rootAddress));
        Assert.IsTrue(ex.Message.Contains("expiration", StringComparison.OrdinalIgnoreCase));

        // Re-signing the root via explicit forAddress with the SAME expiration (1000) remains valid.
        var reSigned = SorobanAuthorization.AuthorizeEntry(
            signed, rootKp, 1000, network, forAddress: rootAddress);
        Assert.IsInstanceOfType(reSigned.Credentials, typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_EmptyDelegateList_SignsRootAndProducesNoDelegates()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        // Documented: delegateSigners "may be empty, producing delegated credentials with no delegates".
        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp), [], ValidUntil, network);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(0, cred.Delegates.Length);
        Assert.AreEqual(ValidUntil, cred.AddressCredentials.SignatureExpirationLedger);
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(new KeyPairEntrySigner(rootKp).Sign(rootHash).ToXdrBase64(),
            cred.AddressCredentials.Signature.ToXdrBase64());
        // The empty-delegates structure must round-trip through XDR.
        Assert.IsInstanceOfType(
            SorobanAuthorizationEntry.FromXdr(signed.ToXdr()).Credentials,
            typeof(SorobanAddressCredentialsWithDelegates));
    }

    [TestMethod]
    public void AuthorizeEntryWithDelegates_ContractAddressDelegate_SignsRoutesAndRoundTrips()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCString("")),
            SampleInvocation());

        // A custom (smart-contract) account signer: the address is a contract and the signature is a
        // custom SCVal — the non-Ed25519 path KeyPairEntrySigner cannot reach. Pairing it with an account
        // delegate also exercises cross-address-type sorting (account type sorts before contract type).
        var contractAddress = new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        var contractSig = new SCString("contract-delegate-signature");
        var contractSigner = new FixedAddressSigner(contractAddress, contractSig);
        var accountKp = KeyPair.Random();

        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            unsigned, new KeyPairEntrySigner(rootKp),
            [contractSigner, new KeyPairEntrySigner(accountKp)],
            ValidUntil, network);

        // The custom signer must receive the CAP-71 root-bound payload (not a self-address-bound or
        // legacy payload). FixedAddressSigner returns a constant, so without this assertion the
        // custom-signer path could pass the wrong hash undetected.
        var expectedRootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        Assert.AreEqual(1, contractSigner.SignCallCount);
        CollectionAssert.AreEqual(expectedRootHash, contractSigner.LastPayloadHash,
            "The contract delegate signer must sign the root-bound preimage hash.");

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.AreEqual(2, cred.Delegates.Length);
        Assert.IsTrue(
            SorobanAuthTestHelpers.CompareAddressXdr(cred.Delegates[0].Address, cred.Delegates[1].Address) < 0,
            "Delegates (account + contract) must be emitted sorted by increasing address.");

        // The contract-address delegate carries its custom signature, addressed by the contract id.
        var contractNode = cred.Delegates[0].Address is ScContractId ? cred.Delegates[0] : cred.Delegates[1];
        Assert.AreEqual(contractAddress.InnerValue, ((ScContractId)contractNode.Address).InnerValue);
        Assert.AreEqual("contract-delegate-signature", ((SCString)contractNode.Signature).InnerValue);

        // The mixed-type delegated entry round-trips through XDR with the contract address intact.
        var decoded = (SorobanAddressCredentialsWithDelegates)
            SorobanAuthorizationEntry.FromXdr(signed.ToXdr()).Credentials;
        var decodedContract =
            decoded.Delegates[0].Address is ScContractId ? decoded.Delegates[0] : decoded.Delegates[1];
        Assert.AreEqual(contractAddress.InnerValue, ((ScContractId)decodedContract.Address).InnerValue);
    }

    [TestMethod]
    public void AuthorizeEntry_ForDeeplyNestedDelegate_RoutesSignatureToDepth3Node()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var level1 = new ScAccountId(KeyPair.Random().AccountId);
        var level2 = new ScAccountId(KeyPair.Random().AccountId);
        var leafKp = KeyPair.Random();
        var leafAddress = new ScAccountId(leafKp.AccountId);

        // root -> level1 -> level2 -> leaf (three levels of nesting).
        var unsigned = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, 0, new SCVoid()),
                [
                    new SorobanDelegateSignature(level1, new SCString("l1-placeholder"),
                    [
                        new SorobanDelegateSignature(level2, new SCString("l2-placeholder"),
                        [
                            new SorobanDelegateSignature(leafAddress, new SCString("leaf-placeholder"), []),
                        ]),
                    ]),
                ]),
            SampleInvocation());

        var signed = SorobanAuthorization.AuthorizeEntry(
            unsigned, leafKp, ValidUntil, network, forAddress: leafAddress);

        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        var rootHash = SorobanAuthorization.BuildAddressAuthPreimageHash(
            network, rootAddress, Nonce, ValidUntil, unsigned.RootInvocation);
        // The depth-3 leaf signed the shared root-bound payload; the routing recursion reached it.
        var leafNode = cred.Delegates[0].NestedDelegates[0].NestedDelegates[0];
        Assert.AreEqual(new KeyPairEntrySigner(leafKp).Sign(rootHash).ToXdrBase64(),
            leafNode.Signature.ToXdrBase64());
        // Every ancestor placeholder is preserved.
        Assert.AreEqual("l1-placeholder", ((SCString)cred.Delegates[0].Signature).InnerValue);
        Assert.AreEqual("l2-placeholder",
            ((SCString)cred.Delegates[0].NestedDelegates[0].Signature).InnerValue);
    }

    [TestMethod]
    public void AuthorizeEntry_BogusForAddressOnDelegatedEntry_DoesNotInvokeSignerAndThrows()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateAddress = new ScAccountId(KeyPair.Random().AccountId);
        var plain = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());
        var delegated = SorobanAuthorization.BuildWithDelegatesEntry(plain, [delegateAddress], ValidUntil);

        // A forAddress matching neither root nor any delegate must be rejected WITHOUT invoking the
        // (interactive) signer — a hardware wallet / remote co-signer must not be prompted for a request
        // guaranteed to fail.
        var signer = new FixedAddressSigner(new ScAccountId(rootKp.AccountId), new SCString("sig"));
        var bogus = new ScAccountId(KeyPair.Random().AccountId);
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntry(delegated, signer, ValidUntil, network, forAddress: bogus));
        Assert.AreEqual("forAddress", ex.ParamName);
        Assert.AreEqual(0, signer.SignCallCount);
    }

    [TestMethod]
    public void AuthorizeEntry_BogusForAddress_WithSignedRootAndMismatchedExpiration_ThrowsArgumentException()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var delegateKp = KeyPair.Random();
        var plain = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(rootKp.AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());
        // Root + delegate signed over expiration 1000 (real, non-placeholder signatures).
        var signed = SorobanAuthorization.AuthorizeEntryWithDelegates(
            plain, new KeyPairEntrySigner(rootKp), [new KeyPairEntrySigner(delegateKp)], 1000, network);

        // A bogus forAddress must report the argument error (no matching node), NOT the expiration guard:
        // the forAddress existence check now runs before the expiration guards.
        var bogus = new ScAccountId(KeyPair.Random().AccountId);
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            SorobanAuthorization.AuthorizeEntry(signed, rootKp, 2000, network, forAddress: bogus));
        Assert.AreEqual("forAddress", ex.ParamName);
    }

    [TestMethod]
    public void AuthorizeEntry_ForAddressMatchingRootAndDelegate_SignsBoth()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        // A delegate node that shares the root's address (an unusual but constructible topology).
        var entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, ValidUntil, new SCVoid()),
                [new SorobanDelegateSignature(rootAddress, new SCVoid(), [])]),
            SampleInvocation());

        // Targeting the shared address signs BOTH the root and the same-address delegate with the same
        // signature (the documented "route to every matching node" plus "sign root" semantics).
        var signed = SorobanAuthorization.AuthorizeEntry(entry, rootKp, ValidUntil, network, forAddress: rootAddress);
        var cred = (SorobanAddressCredentialsWithDelegates)signed.Credentials;
        Assert.IsFalse(cred.AddressCredentials.Signature is SCVoid, "the root must be signed");
        Assert.IsFalse(cred.Delegates[0].Signature is SCVoid, "the same-address delegate must also be signed");
        Assert.AreEqual(
            cred.AddressCredentials.Signature.ToXdrBase64(), cred.Delegates[0].Signature.ToXdrBase64());
    }

    [TestMethod]
    public void AuthorizeEntry_DelegateNestingBeyondMaxDepth_ThrowsInsteadOfStackOverflow()
    {
        var network = Network.Public();
        var rootKp = KeyPair.Random();
        var rootAddress = new ScAccountId(rootKp.AccountId);
        var targetKp = KeyPair.Random();

        // A delegate chain deeper than the supported nesting cap (XdrDataInputStream.DefaultMaxDepth =
        // 200). A pathologically deep caller-built tree must surface a clear, catchable exception rather
        // than an uncatchable StackOverflowException.
        SorobanDelegateSignature[] chain = [];
        for (var i = 0; i < 250; i++)
        {
            chain = [new SorobanDelegateSignature(new ScAccountId(KeyPair.Random().AccountId), new SCVoid(), chain)];
        }

        var deep = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, Nonce, ValidUntil, new SCVoid()), chain),
            SampleInvocation());

        Assert.ThrowsException<InvalidOperationException>(() =>
            SorobanAuthorization.AuthorizeEntry(
                deep, targetKp, ValidUntil, network, forAddress: new ScAccountId(targetKp.AccountId)));
    }

    [TestMethod]
    public void BuildAuthorizationEntryPreimageHash_NullArguments_Throw()
    {
        var entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentials(new ScAccountId(KeyPair.Random().AccountId), Nonce, 0, new SCString("")),
            SampleInvocation());
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(null!, ValidUntil, Network.Public()));
        Assert.ThrowsException<ArgumentNullException>(() =>
            SorobanAuthorization.BuildAuthorizationEntryPreimageHash(entry, ValidUntil, null!));
    }

    [TestMethod]
    [Ignore(
        "Enable after Protocol 27 Testnet upgrade (2026-06-18): submit a V2-signed InvokeHostFunction and assert success. Tracked by issue #186 AC4.")]
    public void AuthorizeEntry_AgainstP27Testnet_SubmitsSuccessfully()
    {
        // Not yet implemented: until the P27 testnet is live (2026-06-18) there is no network to submit
        // to. Fail loudly if enabled before the body exists, so it cannot silently pass and be mistaken
        // for verified coverage of issue #186 AC4.
        Assert.Inconclusive("Pending Protocol 27 Testnet upgrade (2026-06-18); submission test not yet implemented.");
    }
}