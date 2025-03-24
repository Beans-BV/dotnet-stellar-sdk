<p align="center">
    <img height="200" src="https://raw.githubusercontent.com/Beans-BV/dotnet-stellar-sdk/master/docfx/images/logo.svg">
    <h3 align="center">dotnet-stellar-sdk</h3>
    <p align="center">
        Stellar API SDK for .NET
        <br /> 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/actions/workflows/pack_and_test.yml">
            <img src="https://github.com/Beans-BV/dotnet-stellar-sdk/actions/workflows/pack_and_test.yml/badge.svg?branch=master">
        </a>
        <a href="https://www.nuget.org/packages/stellar-dotnet-sdk">
            <img src="https://img.shields.io/nuget/v/stellar-dotnet-sdk.svg" />
        </a>
        <a href="https://www.nuget.org/packages/stellar-dotnet-sdk">
            <img src="https://img.shields.io/nuget/dt/stellar-dotnet-sdk.svg" />
        </a>
        <br />
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/issues/new?template=Bug_report.md">Report Bug</a> · 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/issues/new?template=Feature_request.md">Request Feature</a> · 
        <a href="https://github.com/Beans-BV/dotnet-stellar-sdk/security/policy">Report Security Vulnerability</a> 
    </p>
</p>

## Table of Contents
-   [About the Project](#about-the-project)
-   [Installation](#installation)
    -   [Visual Studio](#visual-studio)
    -   [JetBrains Rider](#jetbrains-rider)
    -   [Other](#other)
-   [Examples](#examples)
-   [Documentation](#documentation)
-   [Community & Support](#community--support)
-   [Contributing](#contributing)
-   [License](#license)

## About The Project
`dotnet-stellar-sdk` is a .NET library for communicating with a [Stellar Horizon server](https://github.com/stellar/go/tree/master/services/horizon) or [Stellar RPC server](https://developers.stellar.org/docs/data/rpc).
<br />
It is used for building Stellar apps.
<br />
_This project originated as a full port of the official [Java SDK API](https://github.com/lightsail-network/java-stellar-sdk)._

## Installation
The `stellar-dotnet-sdk` library is bundled in a NuGet package.
-   [NuGet Package](https://www.nuget.org/packages/stellar-dotnet-sdk)

### Visual Studio
-   Using the [console](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell)
    -   Run `Install-Package stellar-dotnet-sdk` in the console.
-   Using the [NuGet Package Manager](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio)
    -   Search this package [NuGet Package](https://www.nuget.org/packages/stellar-dotnet-sdk) and install it.

### JetBrains Rider
-   [Using NuGet in Rider](https://www.jetbrains.com/help/rider/Using_NuGet.html)
### Other
-   [Ways to install a NuGet package](https://docs.microsoft.com/en-us/nuget/consume-packages/overview-and-workflow#ways-to-install-a-nuget-package)

### Examples
The SDK includes numerous example applications showcasing its features. Explore these standalone projects:
- [Horizon Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Horizon/Program.cs)
- [Soroban Examples](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/master/Examples/Soroban/Program.cs)

## Documentation
Documentation is available [here](https://beans-bv.github.io/dotnet-stellar-sdk/).

## Community & Support
-   [Stellar Stack Exchange](https://stellar.stackexchange.com/)
-   [Keybase Team](https://keybase.io/team/stellar_dotnet)
-   [Stellar Developers on Discord](https://discord.com/invite/stellardev)

## Contributing
For information on how to contribute, please refer to our [contribution guide](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/master/CONTRIBUTING.md).

## License
`dotnet-stellar-sdk` is licensed under an Apache-2.0 license. See the [LICENSE](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/master/LICENSE.txt) file for details.
