using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using Newtonsoft.Json;

namespace ACMESharp.Crypto.JOSE.Impl
{
    /// <summary>
    /// JWS Signing tool implements RS-family of algorithms as per
    /// http://self-issued.info/docs/draft-ietf-jose-json-web-algorithms-00.html#SigAlgTable
    /// </summary>
    public class RSJwsTool : IJwsTool
    {
        private HashAlgorithmName _sha;
        private RSA _rsa;
        private RSJwk _jwk;

        /// <summary>
        /// Specifies the size in bits of the SHA-2 hash function to use.
        /// Supported values are 256, 384 and 512.
        /// </summary>
        public int HashSize { get; set; } = 256;

        /// <summary>
        /// Specifies the size in bits of the RSA key to use.
        /// Supports values in the range 2048 - 4096 inclusive.
        /// </summary>
        /// <returns></returns>
        public int KeySize { get; set; } = 2048;

        public string JwsAlg => $"RS{HashSize}";
        private bool _shouldDispose;

        public void Init()
        {
            InitHash();

            if (KeySize < 2048 || KeySize > 4096)
                throw new InvalidOperationException("illegal RSA key bit length");

            _shouldDispose = true;
            _rsa = new RSACryptoServiceProvider(KeySize);
        }

        private void InitHash()
        {
            switch (HashSize)
            {
                case 256:
                    _sha = HashAlgorithmName.SHA256;
                    break;
                case 384:
                    _sha = HashAlgorithmName.SHA384;
                    break;
                case 512:
                    _sha = HashAlgorithmName.SHA512;
                    break;
                default:
                    throw new System.InvalidOperationException("illegal SHA2 hash size");
            }
        }

        public void Dispose()
        {
            if (_shouldDispose)
            {
                _rsa?.Dispose();
                _rsa = null;
            }
        }

        public string Export()
        {
            try
            {
                return _rsa.ToXmlString(true);
            }
            catch (PlatformNotSupportedException)
            {
                return ToXmlString(_rsa, true);
            }
        }

        public void Import(string exported)
        {
            try
            {
                _rsa.FromXmlString(exported);
            }
            catch (PlatformNotSupportedException)
            {
                FromXmlString(_rsa, exported);
            }
        }

        public object ExportJwk(bool canonical = false)
        {
            // Note, we only produce a canonical form of the JWK
            // for export therefore we ignore the canonical param

            if (_jwk == null)
            {
                var keyParams = _rsa.ExportParameters(false);
                _jwk = new RSJwk
                {
                    e = CryptoHelper.Base64.UrlEncode(keyParams.Exponent),
                    n = CryptoHelper.Base64.UrlEncode(keyParams.Modulus),
                };
            }

            return _jwk;
        }

        public void ImportJwk(string jwkJson)
        {
            Init();
            var jwk = JsonConvert.DeserializeObject<RSJwk>(jwkJson);
            var keyParams = new RSAParameters
            {
                Exponent = CryptoHelper.Base64.UrlDecode(jwk.e),
                Modulus = CryptoHelper.Base64.UrlDecode(jwk.n),
            };
            _rsa.ImportParameters(keyParams);
        }

        internal void Import(RSA rsa)
        {
            KeySize = rsa.KeySize;
            if (KeySize == 2048)
                HashSize = 256;
            else if (KeySize > 2048 && KeySize < 4096)
                HashSize = 384;
            else
                HashSize = 512;
            /*
                General guide for RSA key lengths and their typical hash function pairings:

                RSA 2048-bit: Commonly used with SHA-256 (256-bit hash) or SHA-384 (384-bit hash).
                RSA 3072-bit: Used with SHA-384 (384-bit hash) or SHA-512 (512-bit hash) for higher security.
                RSA 4096-bit: Used with SHA-512 (512-bit hash) for very high security.
            */

            InitHash();
            _shouldDispose = false;
            _rsa = rsa;
        }

        public byte[] Sign(byte[] raw)
        {
            return _rsa.SignData(raw, _sha, RSASignaturePadding.Pkcs1);
        }

        public bool Verify(byte[] raw, byte[] sig)
        {
            return _rsa.VerifyData(raw, sig, _sha, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        }

        // As per RFC 7638 Section 3, these are the *required* elements of the
        // JWK and are sorted in lexicographic order to produce a canonical form
        class RSJwk
        {
            [JsonProperty(Order = 1)]
            public string e;

            [JsonProperty(Order = 2)]
            public string kty = "RSA";

            [JsonProperty(Order = 3)]
            public string n;
        }


        #region MyRsaXmlExportImport

        // These are lifted and adapted from:
        //  https://github.com/dotnet/corefx/issues/23686#issuecomment-383245291

        private const string RSAExportDocRootElement = "RSAKeyValue";
        private static void FromXmlString(RSA rsa, string xmlString)
        {
            RSAParameters parameters = new RSAParameters();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals(RSAExportDocRootElement))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case nameof(parameters.Modulus):
                            parameters.Modulus = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.Exponent):
                            parameters.Exponent = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.P):
                            parameters.P = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.Q):
                            parameters.Q = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.DP):
                            parameters.DP = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.DQ):
                            parameters.DQ = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.InverseQ):
                            parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                        case nameof(parameters.D):
                            parameters.D = (string.IsNullOrEmpty(node.InnerText)
                                ? null : Convert.FromBase64String(node.InnerText));
                            break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }

        private static string ToXmlString(RSA rsa, bool includePrivateParameters)
        {
            RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);

            return string.Format("<{0}>{1}{2}{3}{4}{5}{6}{7}{8}</{0}>",
                RSAExportDocRootElement,
                parameters.Modulus == null ? null
                    : $"<{nameof(parameters.Modulus)}>{Convert.ToBase64String(parameters.Modulus)}</{nameof(parameters.Modulus)}>",
                parameters.Exponent == null ? null
                    : $"<{nameof(parameters.Exponent)}>{Convert.ToBase64String(parameters.Exponent)}</{nameof(parameters.Exponent)}>",
                parameters.P == null ? null
                    : $"<{nameof(parameters.P)}>{Convert.ToBase64String(parameters.P)}</{nameof(parameters.P)}>",
                parameters.Q == null ? null
                    : $"<{nameof(parameters.Q)}>{Convert.ToBase64String(parameters.Q)}</{nameof(parameters.Q)}>",
                parameters.DP == null ? null
                    : $"<{nameof(parameters.DP)}>{Convert.ToBase64String(parameters.DP)}</{nameof(parameters.DP)}>",
                parameters.DQ == null ? null
                    : $"<{nameof(parameters.DQ)}>{Convert.ToBase64String(parameters.DQ)}</{nameof(parameters.DQ)}>",
                parameters.InverseQ == null ? null
                    : $"<{nameof(parameters.InverseQ)}>{Convert.ToBase64String(parameters.InverseQ)}</{nameof(parameters.InverseQ)}>",
                parameters.D == null ? null
                    : $"<{nameof(parameters.D)}>{Convert.ToBase64String(parameters.D)}</{nameof(parameters.D)}>"
            );
        }

        #endregion // MyRsaXmlExportImport
    }
}
