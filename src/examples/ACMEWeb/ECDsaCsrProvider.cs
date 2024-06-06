using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ACMESharp.Enrollment;

namespace ACMEWeb
{
    public class ECDsaCsrProvider : ICertificateRequest
    {
        private readonly ECDsa _ecdsa;
        private readonly HashAlgorithmName _hashAlgorithm;
        public ECDsaCsrProvider(HashAlgorithmName hashAlgorithm, int keySize)
        {
            switch (keySize)
            {
                case 256:
                    _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                    break;
                case 384:
                    _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                    break;
                case 521:
                    _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP521);
                    break;
                default:
                    _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                    break;
            }
            _hashAlgorithm = hashAlgorithm;

        }
        public X509Certificate2Collection Complete(byte[] certificate)
        {
            var certCollection = new X509Certificate2Collection();
            certCollection.Import(certificate, null, X509KeyStorageFlags.Exportable);
            certCollection = new X509Certificate2Collection(certCollection.OrderByDescending(x => x.NotBefore).ToArray());
            certCollection[0] = certCollection[0].CopyWithPrivateKey(_ecdsa);
            return certCollection;
        }

        public byte[] GenerateCertificateRequest(string commonName, IEnumerable<string> dnsNames)
        {
            var csr = new CertificateRequest(commonName, _ecdsa, _hashAlgorithm);
            return csr.CreateSigningRequest();
        }

    }

    

}
