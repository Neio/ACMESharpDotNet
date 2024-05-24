# ACMESharp Core <img align="right" width="100" src="https://raw.githubusercontent.com/Neio/ACMESharpCore/master/docs/acmesharp-logo-color.png">

An ACME client library for .NET


![CI](https://github.com/Neio/ACMESharpCore/workflows/CI/badge.svg)

<!--
[![AV Build status](https://ci.appveyor.com/api/projects/status/v3ch5gu85i05ehd9?svg=true)](https://ci.appveyor.com/project/Neio/acmesharpcore)

-->

## Tests

| Component/Test Type | Linux | Windows |
|-|-:|-:|
| Base Unit Tests |[![Test](https://gist.githubusercontent.com/Neio/4c682a860b32f3c20d5a1578f2ea392e/raw/46b5bae7121d94da028b4ca088bb5000db6c4f05/acmesharpcore-unit_tests-ubuntu.md_badge.svg)](https://gist.github.com/Neio/4c682a860b32f3c20d5a1578f2ea392e) | [![Test](https://gist.githubusercontent.com/Neio/3c24d49ccab356883fc7c8aa7ddb8b69/raw/518599fbd6b119012ffd647d7f596d8b72f5e575/acmesharpcore-unit_tests-windows.md_badge.svg)](https://gist.github.com/Neio/3c24d49ccab356883fc7c8aa7ddb8b69) |
| SimplePKI Unit Tests |[![Test](https://gist.githubusercontent.com/Neio/87212718f0e466a4ca1acd2cfe5864e0/raw/bb1333b4ba995894ea5708076f311a8163c9d24b/acmesharpcore-simplepki_unit_tests-ubuntu.md_badge.svg)](https://gist.github.com/Neio/87212718f0e466a4ca1acd2cfe5864e0) | [![Test](https://gist.githubusercontent.com/Neio/8565f122b30277c19e0672b8e50881f4/raw/c109510f360835bf07870fbd62a6aa6427392065/acmesharpcore-simplepki_unit_tests-windows.md_badge.svg)](https://gist.github.com/Neio/8565f122b30277c19e0672b8e50881f4) |
| MockServer Unit Tests |[![Test](https://gist.githubusercontent.com/Neio/82db02b925701f956e4a34d23d669523/raw/bdbb1e08e82394598ba9d7a2bae4577d41127045/acmesharpcore-mockserver_unit_tests-ubuntu.md_badge.svg)](https://gist.github.com/Neio/82db02b925701f956e4a34d23d669523) | [![Test](https://gist.githubusercontent.com/Neio/4e63ee72e708b50af9db071dc4beae10/raw/cf1427cc41d536e7944ad974dfaa7a8f0efec63f/acmesharpcore-mockserver_unit_tests-windows.md_badge.svg)](https://gist.github.com/Neio/4e63ee72e708b50af9db071dc4beae10) |
| Integration Tests | [![Test](https://gist.githubusercontent.com/Neio/e597698df42796e4f47385b61f5dab55/raw/c8dd7bb1fb887ac5b37bfca03d7c27a1cecebebf/acmesharpcore-integration_tests-ubuntu.md_badge.svg)](https://gist.github.com/Neio/e597698df42796e4f47385b61f5dab55) | [![Test](https://gist.githubusercontent.com/Neio/a08fafc6b77d8bdab0dd0ae463f78cd9/raw/74b23356363a8a6397eb3165bfeaeef4b30f484c/acmesharpcore-integration_tests-windows.md_badge.svg)](https://gist.github.com/Neio/a08fafc6b77d8bdab0dd0ae463f78cd9)

## Packages

| Component | Stable Release | Early Access |
|-|-|-|
| | Hosted on the [NuGet Gallery](https://www.nuget.org/packages?q=Tags%3A%22acmesharp%22) | Hosted on [MyGet Gallery](https://www.myget.org/gallery/acmesharp)
| ACMESharpDotNet client library | [![NuGet](https://img.shields.io/nuget/v/ACMESharpCore.svg)](https://www.nuget.org/packages/ACMESharpCore) | [![MyGet](https://img.shields.io/myget/acmesharp/vpre/ACMESharpCore.svg)](https://www.myget.org/feed/acmesharp/package/nuget/ACMESharpCore)
| Crypto Support library | [![NuGet](https://img.shields.io/nuget/v/ACMESharpCore.Crypto.svg)](https://www.nuget.org/packages/ACMESharpCore.Crypto) | [![MyGet](https://img.shields.io/myget/acmesharp/vpre/ACMESharpCore.Crypto.svg)](https://www.myget.org/feed/acmesharp/package/nuget/ACMESharpCore.Crypto)
| SimplePKI library | [![NuGet](https://img.shields.io/nuget/v/PKISharp.SimplePKI.svg)](https://www.nuget.org/packages/PKISharp.SimplePKI) | [![MyGet](https://img.shields.io/myget/acmesharp/vpre/PKISharp.SimplePKI.svg)](https://www.myget.org/feed/acmesharp/package/nuget/PKISharp.SimplePKI)

## Overview

This library originated as a port of the [ACMESharp](https://github.com/ebekker/ACMESharp) client library from .NET Framework to .NET Muti-targets

However, this rewrite is now actually more complete than the [original](https://github.com/ebekker/ACMESharp),
including operations from the ACME specification that were left out of the original and supporting the latest
versions of the specification.  Check out the [library-specific README](/src/ACMESharp) for details as they develop.

A couple of useful examples have been [put together](https://github.com/Neio/ACMESharpCore/tree/master/src/examples) to demonstrate how to use the client library to implement a [CLI tool](https://github.com/Neio/ACMESharpCore/tree/master/src/examples/ACMECLI) and automated certificate installation for [ASP.NET Core](https://github.com/Neio/ACMESharpCore/tree/master/src/examples/ACMEKestrel) applications.  More are coming...

----

 Please Note: |
--------------|
If you are interested in using Let's Encrypt, or any other ACME-compliant CA in a .NET context, please see the [ACMESharp project](https://github.com/ebekker/ACMESharp) for a working implementation of an ACME client library for .NET Framework and complementary PowerShell module for Windows PowerShell.

----

The goals for this project:

* [x] Migrate the ACMESharp client library to .NET Standard 2.0
* [x] Remove legacy cruft
* [x] Clean up the namespace structure and code org
* [x] Adjust coding standards to better conform with industry standards
* [x] Complete any missing features from the ACME spec
* [x] Prepare for, and implement move to ACME 2.0 spec
* [ ] Clearly separate and maintain independently the client library and the PS module
