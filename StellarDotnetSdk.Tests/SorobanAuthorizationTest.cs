﻿using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using SorobanAddressCredentials = StellarDotnetSdk.Operations.SorobanAddressCredentials;
using SorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using SorobanAuthorizedInvocation = StellarDotnetSdk.Operations.SorobanAuthorizedInvocation;
using SorobanCredentials = StellarDotnetSdk.Operations.SorobanCredentials;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SorobanAuthorizationTest
{
    private const long Nonce = -9223372036854775807;
    private const uint SignatureExpirationLedger = 1319013123;

    private const string WasmHash = "6416281094F721A3CC324DC5A119A71101E80F17B03D92FE528AFEC56238B882";

    private readonly ScAccountId _accountAddress = new("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

    private readonly ContractExecutableWasm _contractExecutableWasm = new(WasmHash);

    private readonly SCString _signature = new("Signature");

    private SorobanAddressCredentials InitSorobanAddressCredentials()
    {
        return new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithValidCredentials()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithMissingAddress()
    {
        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            new SorobanAddressCredentials(null, Nonce, SignatureExpirationLedger, _signature));
        Assert.IsTrue(ex.Message.Contains("Address cannot be null."));
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithAddressBeingAContractAddress()
    {
        var contractAddress = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var sorobanCredentials =
            new SorobanAddressCredentials(contractAddress, Nonce, SignatureExpirationLedger, _signature);

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(contractAddress.InnerValue, ((ScContractId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithMissingSignature()
    {
        var ex = Assert.ThrowsException<ArgumentNullException>(()
            => new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, null));
        Assert.IsTrue(ex.Message.Contains("Signature cannot be null."));
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithZeroSignatureExpirationLedger()
    {
        var sorobanCredentials = new SorobanAddressCredentials(_accountAddress, Nonce, 0, _signature);

        // Act 
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithZeroNonce()
    {
        var sorobanCredentials =
            new SorobanAddressCredentials(_accountAddress, 0, SignatureExpirationLedger, _signature);
        // Act 
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAuthorizationEntryWithEmptySubInvocations()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);

        var preimage = new ContractIdAddressPreimage(_accountAddress.InnerValue, salt);
        var rootInvocation = new SorobanAuthorizedInvocation(
            new SorobanAuthorizedCreateContractFunction(
                new CreateContractHostFunction(preimage, _contractExecutableWasm)),
            []);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractFunction));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(0, decodedSubInvocations.Length);
    }

    [TestMethod]
    public void TestSorobanAuthorizationEntryContainingAuthorizedCreateContractFunction()
    {
        var hostFunction = new CreateContractHostFunction(WasmHash, _accountAddress.InnerValue);
        var authorizedCreateContractFn = new SorobanAuthorizedCreateContractFunction(hostFunction);

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedCreateContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedCreateContractFn, Array.Empty<SorobanAuthorizedInvocation>()),
            ]);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractFunction));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        var salt = ((ContractIdAddressPreimage)hostFunction.ContractIdPreimage).Salt;
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.IsInstanceOfType(decodedSubFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        decodedPreimage = (ContractIdAddressPreimage)decodedSubFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedSubFunction.Executable, typeof(ContractExecutableWasm));
        decodedExecutable = (ContractExecutableWasm)decodedSubFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
    }

    [TestMethod]
    public void TestSorobanAuthorizationEntryContainingAuthorizedCreateContractV2Function()
    {
        var arguments = new SCVal[] { new SCString("Test") };
        var hostFunction =
            new CreateContractV2HostFunction(WasmHash, _accountAddress.InnerValue, arguments);
        var authorizedCreateContractFn = new SorobanAuthorizedCreateContractV2Function(hostFunction);

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedCreateContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedCreateContractFn, []),
            ]
        );

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractV2Function));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractV2Function)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        var salt = ((ContractIdAddressPreimage)hostFunction.ContractIdPreimage).Salt;
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction =
            ((SorobanAuthorizedCreateContractV2Function)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.IsInstanceOfType(decodedSubFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        decodedPreimage = (ContractIdAddressPreimage)decodedSubFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedSubFunction.Executable, typeof(ContractExecutableWasm));
        decodedExecutable = (ContractExecutableWasm)decodedSubFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
        var decodedArguments = decodedRootFunction.Arguments;
        Assert.AreEqual(arguments.Length, decodedArguments.Length);
        Assert.AreEqual(((SCString)arguments[0]).InnerValue, ((SCString)decodedArguments[0]).InnerValue);
    }

    [TestMethod]
    public void TestSorobanAuthorizationEntryContainingAuthorizedContractFunction()
    {
        var contractAddress = new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        var functionName = "hello";
        var argName = "world";
        var args = new SCVal[] { new SCBool(false), new SCString(argName) };
        var authorizedContractFn = new SorobanAuthorizedContractFunction(
            new InvokeContractHostFunction(contractAddress, new SCSymbol(functionName), args));

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedContractFn, []),
            ]);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedContractFunction));
        var decodedRootFunction = ((SorobanAuthorizedContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.AreEqual(functionName, decodedRootFunction.FunctionName.InnerValue);
        Assert.AreEqual(args.Length, decodedRootFunction.Args.Length);
        Assert.AreEqual(((SCBool)args[0]).InnerValue, ((SCBool)decodedRootFunction.Args[0]).InnerValue);
        Assert.AreEqual(((SCString)args[1]).InnerValue, ((SCString)decodedRootFunction.Args[1]).InnerValue);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction = ((SorobanAuthorizedContractFunction)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.AreEqual(functionName, decodedSubFunction.FunctionName.InnerValue);
        Assert.AreEqual(args.Length, decodedSubFunction.Args.Length);
        Assert.AreEqual(((SCBool)args[0]).InnerValue, ((SCBool)decodedSubFunction.Args[0]).InnerValue);
        Assert.AreEqual(((SCString)args[1]).InnerValue, ((SCString)decodedSubFunction.Args[1]).InnerValue);
    }
}