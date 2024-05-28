
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ACMESharp.Enrollment;

namespace ACMEWeb
{
    public class RsaCsrProvider : ICertificateRequest
    {
        private readonly RSA _rsa;
        private readonly HashAlgorithmName _hashAlgorithm;
        public RsaCsrProvider(HashAlgorithmName hashAlgorithm, int keyLength = 2048)
        {
            _hashAlgorithm = hashAlgorithm;
            _rsa = RSA.Create(keyLength);
        }
        public X509Certificate2Collection Complete(byte[] certificate)
        {
            var certCollection = new X509Certificate2Collection();
            certCollection.Import(certificate, null, X509KeyStorageFlags.Exportable);
            certCollection[0] = certCollection[0].CopyWithPrivateKey(_rsa);
            return certCollection;
        }

        public byte[] GenerateCertificateRequest(string commonName, IEnumerable<string> dnsNames)
        {
            var csr = new CertificateRequest(commonName, _rsa, _hashAlgorithm, RSASignaturePadding.Pkcs1);
            return csr.CreateSigningRequest();
        }
    }
}
