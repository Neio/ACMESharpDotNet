# README - ACMESharp Dotnet

## Changes

* Based on .NET Framework, .NET Standard 2.0, .NET 8.0, etc
  * Moved to HttpClient from WebRequest
* Using async code throughout
* Expanded support for RSA keys to include more SHA (256, 348, 512) and RSA (2048-4096) sizes
* Added support for EC keys for both account and cert keys, supporting standard curves
  * P-256
  * P-348
  * P-521
* Improved Integration Testing setup
* CICD in GitHub


## What's Implemented and Working

* ACME Resource Directory Lookup
* First Nonce Lookup
* Create Account
  * Create/Check Duplicate Account
* Update Account
* Change Account Key
* Deactivate Account
* Create Order
* Decode Challenge details for types:
  * `dns-01`
  * `http-01`
  * `tls-alpn-01`
* Answer Challenge
* Refresh Challenge
* Deactivate Authorization
* Finalize Authorization (Submit CSR)
* Revoke Certificate
  * Support Revoke with Certificate Key Pair
* Cross-platform support
  * Tested on [Windows]
  * Tested on [Linux]
* External Account Binding from [ACME 7.3.4](https://datatracker.ietf.org/doc/html/rfc8555#section-7.3.4)
  
## What's Not Implemented/Not Working

* Automatic detection/handling of Change of TOS from [ACME 7.3.4](https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.3.4) is not implemented
* Order Pre-Authorizations from [ACME 7.4.1](https://tools.ietf.org/html/draft-ietf-acme-acme-12#section-7.4.1) is not implemented

