---
name: verify
description: Verify changes to the SDK library (StellarDotnetSdk) at its runtime surface - the packed NuGet package used from a scratch app, its requests to Stellar RPC and Horizon on Testnet, and the XML docs it ships.
---

# Verify the SDK library

The SDK is a library, so its surface is the package boundary and the HTTP requests it sends. Use the package the
way a NuGet user does, not through a project reference.

## Pack the branch

Use a throwaway version. The SDK package depends on `stellar-dotnet-sdk-xdr` at the same version, so pack both.

```bash
dotnet pack StellarDotnetSdk/StellarDotnetSdk.csproj -c Release -o <feed> -p:Version=0.0.0-verify1
dotnet pack StellarDotnetSdk.Xdr/StellarDotnetSdk.Xdr.csproj -c Release -o <feed> -p:Version=0.0.0-verify1
```

Packing with `-p:Version` rebuilds `bin/Release` with that version. The next normal build overwrites it.

## Use it from a scratch app

- Create a `net10.0` console app with `<PackageReference Include="stellar-dotnet-sdk" Version="0.0.0-verify1" />`.
- Restore into an isolated cache, so the throwaway version never lands in `~/.nuget/packages`:
  `NUGET_PACKAGES=<scratch>/nuget-cache dotnet restore --source <feed> --source ~/.nuget/packages`.
  After one normal build of this repository, the global folder holds every dependency, so this works offline.
- Run the app with the same `NUGET_PACKAGES` value.

## Drive Testnet and capture the wire

- Stellar RPC: `https://soroban-testnet.stellar.org`. Friendbot: `https://friendbot.stellar.org/?addr=<G...>`.
- Call `Network.UseTestNetwork()` before `tx.Sign(...)`.
- Capture request bodies: pass `new HttpClient(handler)` to `new StellarRpcServer(url, httpClient)`. Make `handler`
  a `DelegatingHandler` that reads `request.Content`, with `InnerHandler = new HttpClientHandler()`.
- Address (non-source) auth entries need a deploy that another account authorizes:
  `CreateContractOperation.FromAddress(wasmHash, otherAccountId)`. Upload
  `StellarDotnetSdk.IntegrationTests/TestData/Wasm/soroban_hello_world_contract.wasm` first. Uploads are
  content-addressed, so a repeat upload is harmless.
- The simulate, sign, send and poll flow is in
  `StellarDotnetSdk.IntegrationTests/Infrastructure/SorobanIntegrationTestBase.cs`.
- To see what the server does without the SDK, post the same JSON-RPC request with `HttpClient.PostAsync`.

## XML docs

- The docs ship as `lib/<tfm>/StellarDotnetSdk.xml` in the package, for net10.0, net8.0 and netstandard2.1.
- To see the text docfx renders for `<inheritdoc>`, call Roslyn's internal
  `Microsoft.CodeAnalysis.Shared.Extensions.ISymbolExtensions.GetDocumentationComment(symbol, compilation, culture,
  expandIncludes: true, expandInheritdoc: true, cancellationToken)` by reflection, from the
  `Microsoft.CodeAnalysis.CSharp.Workspaces` package. Build the compilation on the packaged DLL, with
  `XmlDocumentationProvider.CreateFromFile(<xml>)` for its docs. docfx calls the same method.

## Gotchas

- Stellar RPC `GetAccount` trails Friendbot by a few seconds, and Friendbot can rate-limit. Retry both.
- `GetTransactionResponse.WasmHash` is uppercase hex. Compare it case-insensitively.
- `AuthMode.ENFORCE` with address auth and no supplied entries fails with `Error(Auth, InvalidAction)`. That is
  expected.

## Not reachable at a surface

- Stellar RPC versions other than the one Testnet runs.
- IDE tooltips (Visual Studio, Rider): they need the IDE.
