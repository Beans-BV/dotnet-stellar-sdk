---
name: verify
description: Verify a change to the SDK at its public surface - a consumer program on the packed NuGet package, against a real local HTTP server. Use for /verify in this repo.
---

# Verify the SDK at its package boundary

The SDK is a library: the surface is a separate consumer program that references the **packed**
`stellar-dotnet-sdk` package, not the project.

## Pack (new code, and the base for a before/after)

```bash
dotnet pack stellar-dotnet-sdk.sln -c Release -o <feed> -p:PackageVersion=12.0.0-verify.new
git worktree add --detach <base-dir> <base-sha>   # then pack there with -p:PackageVersion=12.0.0-verify.base
```

- The solution pack builds `stellar-dotnet-sdk` and `stellar-dotnet-sdk-xdr` with the same version, so the SDK
  depends on the matching XDR package. A unique prerelease version avoids a stale hit in `~/.nuget/packages`.

## Consumer project

- `PackageReference Include="stellar-dotnet-sdk" Version="$(SdkVersion)"`, built with `-p:SdkVersion=...`;
  one copy of the project per version (separate `obj/`).
- `RestorePackagesPath` in the project folder, `RestoreFallbackFolders` = `~/.nuget/packages` (dependencies
  resolve offline), and a `nuget.config` with `<clear />`, the local feed and nuget.org.
- Check `obj/project.assets.json` for the resolved version. `AssemblyInformationalVersion` ends in
  `+<commit sha>`: print it to prove which build ran.

## Drive it

- HTTP services (SEP clients, Horizon, RPC): a raw `TcpListener` HTTP/1.1 server on 127.0.0.1 that records the
  raw request (request line, headers, body bytes) and answers a scripted response. `QuoteService` accepts
  `http://127.0.0.1:<port>` in its constructor; `FromDomainAsync` forces `https://<domain>`, so drive it with a
  caller-owned `HttpClient` over a custom `HttpMessageHandler`.
- Time zones: run the consumer with `TZ=America/New_York` or `TZ=Asia/Tokyo` (honored on macOS and Linux).

## Gotchas

- On this Mac (`LANG` unset, region NL), .NET 10 picks `CurrentCulture` `en-NL` (comma decimals) and .NET 8 picks
  `en-US`. Print values with the invariant culture before diffing runs across runtimes.
- `dotnet pack` of the whole solution takes about a minute per tree. A clean build plus every CI step in
  `.github/workflows/pack_and_test.yml` (three unit-test targets, MAUI desktop build, MAUI unit tests) takes
  about 2.5 minutes per commit.
