using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace ACMESharp.Crypto.JOSE.Impl
{
    public class CertJwsTool: IJwsTool
    {
        private readonly IJwsTool _jwsTool = null;
        public CertJwsTool(X509Certificate2 certificate)
        {
            // Note the Tool might only be used for verification, then public key is enough

            var rsaPrivateKey = certificate.GetRSAPrivateKey();
            var rsaKey = rsaPrivateKey ?? certificate.GetRSAPublicKey();
            
            if (rsaKey != null)
            {
                // RSA
                var rsJwsTool = new RSJwsTool();
                rsJwsTool.Import(rsaKey);
                _jwsTool = rsJwsTool;
                return;
            }
            

            var ecdsa = certificate.GetECDsaPrivateKey() ?? certificate.GetECDsaPublicKey();
            if (ecdsa != null)
            {
                var ecdsaJwsTool = new ESJwsTool();
                ecdsaJwsTool.Import(ecdsa);
                _jwsTool = ecdsaJwsTool;
                return;
            }
            
            throw new NotSupportedException("Unsupported private key type");
                
            
        }

        public string JwsAlg => _jwsTool.JwsAlg;

        public void Dispose()
        {
            _jwsTool.Dispose();
        }

        public string Export()
        {
            return _jwsTool.Export();
        }

        public object ExportJwk(bool canonical = false)
        {
            return _jwsTool.ExportJwk(canonical);
        }

        public void Import(string exported)
        {
            _jwsTool.Import(exported);
        }

        public void Init()
        {
            _jwsTool.Init();
        }

        public byte[] Sign(byte[] raw)
        {
            return _jwsTool.Sign(raw);
        }

        public bool Verify(byte[] raw, byte[] sig)
        {
            return _jwsTool.Verify(raw, sig);
        }

    }
}