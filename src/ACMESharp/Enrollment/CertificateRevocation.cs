using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;

namespace ACMESharp.Enrollment
{
    public class CertificateRevocation
    {

        public Action<string, object> BeforeAcmeSign { get; set; }

        public Action<string, HttpRequestMessage> BeforeHttpSend { get; set; }

        public Action<string, HttpResponseMessage> AfterHttpSend { get; set; }

        public async Task Revoke(X509Certificate2 certificate,
            Uri acmeServer,
            RevokeReason revokeReason = RevokeReason.Unspecified,
            HttpClient httpClient = null,
            CancellationToken token = default)
        {

            if (acmeServer == null)
                throw new InvalidOperationException("Must specify the ACME server or have saved service directory to use");
            /*
            https://datatracker.ietf.org/doc/html/rfc8555/#section-7.6
            "jwk": certificate's public key 

            The server MUST also consider a revocation request valid if it is
            signed with the private key corresponding to the public key in the
            certificate.

            
            */
            IJwsTool jwsTool = new Crypto.JOSE.Impl.CertJwsTool(certificate);
            var acmeServerUriAuthority = acmeServer.GetLeftPart(UriPartial.Authority);
            if (httpClient != null)
            {
                if (httpClient.BaseAddress != null && httpClient.BaseAddress.GetLeftPart(UriPartial.Authority) != acmeServerUriAuthority)
                {
                    throw new InvalidOperationException($"HttpClient base address {httpClient.BaseAddress.GetLeftPart(UriPartial.Authority)} must match the ACME server URI {acmeServerUriAuthority}");
                }
                else if (httpClient.BaseAddress == null)
                {
                    httpClient.BaseAddress = new Uri(acmeServerUriAuthority);
                }
            }

            using (var client = httpClient != null ?
                new AcmeProtocolClient(httpClient, signer: jwsTool, usePostAsGet: true) :
                new AcmeProtocolClient(acmeServer, signer: jwsTool, usePostAsGet: true))
            {
                client.BeforeAcmeSign = BeforeAcmeSign;
                client.AfterHttpSend = AfterHttpSend;
                client.BeforeHttpSend = BeforeHttpSend;

                client.Directory.Directory = acmeServer.PathAndQuery;
                client.Directory = await client.GetDirectoryAsync(token);

                await client.GetNonceAsync();

                var certificateBytes = certificate.Export(X509ContentType.Cert);

                await client.RevokeCertificateAsync(certificateBytes, revokeReason, jwsTool, token);
            }
        }

        

    }
}
