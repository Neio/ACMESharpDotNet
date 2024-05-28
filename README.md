# ACMESharp Core <img align="right" width="100" src="https://raw.githubusercontent.com/Neio/ACMESharpCore/master/docs/acmesharp-logo-color.png">

An ACME client library for .NET


![CI](https://github.com/Neio/ACMESharpDotNet/workflows/CI/badge.svg)

<!--
[![AV Build status](https://ci.appveyor.com/api/projects/status/v3ch5gu85i05ehd9?svg=true)](https://ci.appveyor.com/project/Neio/acmesharpcore)

-->

## Overview

This library originated as a port of the [ACMESharp](https://github.com/ebekker/ACMESharp) client library from .NET Framework to .NET Muti-targets

However, this rewrite is now actually more complete than the [original](https://github.com/ebekker/ACMESharp),
including operations from the ACME specification that were left out of the original and supporting the latest
versions of the specification.  Check out the [library-specific README](/src/ACMESharp) for details as they develop.

A couple of useful examples have been [put together](https://github.com/Neio/ACMESharpDotNet/tree/master/src/examples) to demonstrate how to use the client library to implement a [CLI tool](https://github.com/Neio/ACMESharpDotNet/tree/master/src/examples/ACMECLI) and automated certificate installation for [ASP.NET Core](https://github.com/Neio/ACMESharpDotNet/tree/master/src/examples/ACMEKestrel) applications.  More are coming...

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
