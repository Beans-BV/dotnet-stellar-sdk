# dotnetcore-stellar-sdk
Stellar API SDK for .NET Core 2.x

[![Build Status](https://travis-ci.org/elucidsoft/dotnetcore-stellar-sdk.svg?branch=master)](https://travis-ci.org/elucidsoft/dotnetcore-stellar-sdk)  [![NuGet](https://img.shields.io/nuget/v/1.0.svg)](https://www.nuget.org/packages/stellar-dotnetcore-sdk/1.0.0)

The .NET Core Stellar Sdk library provides APIs to build transactions and connect to [Horizon](https://github.com/stellar/horizon).

Read more about [Stellar](https://www.stellar.org/)

This project is a full port of the official [Java SDK API](https://github.com/stellar/java-stellar-sdk).  It is fully functional and all of the original Java Unit Tests were also ported and are passing.  

## Quick Start
To install using Nuget run `Install-Package stellar-dotnetcore-sdk -Version 1.0.0` or install the Nuget package from Visual Studio.

## Documentation
Read the API [Reference Documentation](https://elucidsoft.github.io/dotnetcore-stellar-sdk/) for more information about the API.  For more guidance Stellar.org has documentation that is specific to their [Javascript API](https://www.stellar.org/developers/js-stellar-sdk/reference/) but usage is very similar.

## Basic Usage
For some examples on how to use this library, take a look at the [Get Started docs in the developers site](https://www.stellar.org/developers/guides/get-started/create-account.html).

## Demo Application
In the root of the solution there is a console application called TestConsole, it connects to the Horizon TestNet and pulls down some data. The TestNet can be cleared at any moment so the keys it uses may not be valid.  You can use the [Stellar Laboratory](https://www.stellar.org/laboratory/) to setup an account on TestNet and to play around with data between the TestNet and the API.  You can also use the API to create an account, and Laboratory to validate the results, vice versa!  

## License
dotnetcore-stellar-sdk is licensed under an Apache-2.0 license. See the [LICENSE](https://github.com/elucidsoft/dotnetcore-stellar-sdk/blob/master/LICENSE.txt) file for details.
