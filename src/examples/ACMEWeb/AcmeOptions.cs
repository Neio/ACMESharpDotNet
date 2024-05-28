using System;
using System.Collections.Generic;
using ACMESharp.Authorizations;
using ACMESharp.Protocol.Resources;

namespace ACMEWeb
{
    public class AcmeOptions
    {
        public const string ZeroSslEndpoint = "https://acme.zerossl.com/v2/DV90";
        public const string LetsEncryptV2Endpoint = "https://acme-v02.api.letsencrypt.org/directory";

        public const int DefaultRsaKeySize = 2048;
        public const int DefaultEcKeySize = 256;

        public string CaUrl { get; set; } = LetsEncryptV2Endpoint;

        public IEnumerable<string> AccountContactEmails { get; set; } = new List<string>();

        public bool AcceptTermsOfService { get; set; }

        public IEnumerable<string> DnsNames { get; set; } = new List<string>();

        public string CertificateKeyAlgor { get; set; } = "ec";

        public int? CertificateKeySize { get; set; }

        public int WaitForAuthorizations { get; set; } = 60;

        public int WaitForCertificate { get; set; } = 60; 

        public string EabKid { get; set; }

        public string EabHmacKey { get; set; }
    }
}