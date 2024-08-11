using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Crypto.JOSE.Impl;
using Newtonsoft.Json;

namespace ACMESharp.UnitTests
{
    [TestClass]
    public class CertJwsToolTest
    {
        [TestMethod]
        [DataRow(2048, "2.16.840.1.101.3.4.2.1", "RS256")]
        [DataRow(3072, "2.16.840.1.101.3.4.2.2", "RS384")]
        [DataRow(4096, "2.16.840.1.101.3.4.2.3", "RS512")] // Import from cert is fixed to RS256
        public void TestRsaCertJwsTool(int keyLength, string oid, string expectedAlg)
        {
            using (var rsa = RSA.Create(keyLength))
            {
                var certRequest = new CertificateRequest("CN=foo.example.com", rsa, HashAlgorithmName.FromOid(oid), RSASignaturePadding.Pkcs1);
                certRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
                
                var cert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
                // With Private key
                TestCommon(cert, expectedAlg);
               
                // Without Private key
                var certBytes = cert.Export(X509ContentType.Cert);
                TestCommon(new X509Certificate2(certBytes), expectedAlg);
            }

            
            
            
        }

        [TestMethod]
        [DataRow("2.16.840.1.101.3.4.2.1", "ES256")]
        [DataRow("2.16.840.1.101.3.4.2.2", "ES384")]
        [DataRow("2.16.840.1.101.3.4.2.3", "ES512")]
        public void TestEcdsaCertJwsTool(string oid,  string expectedAlg)
        {
            ECCurve curve;
            if (expectedAlg == "ES256")
            {
                curve = ECCurve.NamedCurves.nistP256;
            }
            else if (expectedAlg == "ES384")
            {
                curve = ECCurve.NamedCurves.nistP384;
            }
            else if (expectedAlg == "ES512")
            {
                curve = ECCurve.NamedCurves.nistP521;
            }
            else
            {
                throw new Exception("unknown or unsupported signature algorithm");
            }

            using (var ecdsa = ECDsa.Create(curve))
            {
                var certRequest = new CertificateRequest("CN=foo.example.com", ecdsa, HashAlgorithmName.FromOid(oid));
                certRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
                
                var cert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

                // With Private key
                TestCommon(cert, expectedAlg);
               
                // Without Private key
                var certBytes = cert.Export(X509ContentType.Cert);
                TestCommon(new X509Certificate2(certBytes), expectedAlg);
                
            }
        }

        private void TestCommon(X509Certificate2 cert, string expectedAlg)
        {
            using (var jwsTool = new CertJwsTool(cert))
            {
                var jwsAlg = jwsTool.JwsAlg;
                var jwsExportJwk = jwsTool.ExportJwk();
                Assert.IsNotNull(jwsExportJwk);
                Assert.AreEqual(expectedAlg, jwsAlg);

                var jwkStr = JsonConvert.SerializeObject(jwsExportJwk);

                var restoredJwsTool = GetJwsTool(expectedAlg, jwkStr);
                var reExporetedJwsStr = JsonConvert.SerializeObject(restoredJwsTool.ExportJwk());
                Assert.AreEqual(reExporetedJwsStr, jwkStr);
            }
        }

        private IJwsTool GetJwsTool(string alg, string jwk)
        {
            IJwsTool tool = null;
            switch (alg)
            {
                case "RS256":
                    tool = new RSJwsTool { HashSize = 256 };
                    ((RSJwsTool)tool).ImportJwk(jwk);
                    break;
                case "RS384":
                    tool = new RSJwsTool { HashSize = 384 };
                    ((RSJwsTool)tool).ImportJwk(jwk);
                    break;
                case "RS512":
                    tool = new RSJwsTool { HashSize = 512 };
                    ((RSJwsTool)tool).ImportJwk(jwk);
                    break;
                case "ES256":
                    tool = new ESJwsTool { HashSize = 256 };
                    ((ESJwsTool)tool).ImportJwk(jwk);
                    break;
                case "ES384":
                    tool = new ESJwsTool { HashSize = 384 };
                    ((ESJwsTool)tool).ImportJwk(jwk);
                    break;
                case "ES512":
                    tool = new ESJwsTool { HashSize = 512 };
                    ((ESJwsTool)tool).ImportJwk(jwk);
                    break;
                default:
                    throw new Exception("unknown or unsupported signature algorithm");
            }
            return tool;
        }
    }
    
}