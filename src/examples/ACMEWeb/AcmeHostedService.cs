
using System.Diagnostics.Contracts;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ACMESharp.Enrollment;
using ACMESharp.Protocol.Resources;
using Microsoft.Extensions.Options;

namespace ACMEWeb
{
    public class AcmeHostedService : BackgroundService, IHostedService
    {
        private readonly CertificateEnrollment _certificateEnrollment;
        private readonly AcmeOptions _acmeOptions;
        private readonly ILogger<AcmeHostedService> _logger;
        private readonly IStorage _storage;
        private readonly IHostApplicationLifetime _lifetime;
        public AcmeHostedService(IHostApplicationLifetime lifetime, IChallengeProvider challeneProvider, IStorage storage, CertificateEnrollment certificateEnrollment, ILogger<AcmeHostedService> logger, IOptions<AcmeOptions> options)
        {
            _certificateEnrollment = certificateEnrollment;
            _acmeOptions = options.Value;
            _logger = logger;
            _storage = storage;
            _lifetime = lifetime;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _certificateEnrollment.ProgressUpdate = new Action<EnrollmentProgress, string>((progress, message) =>
                {
                    _logger.LogInformation("Progress: {0} - {1}", progress, message);
                });
                // delay for 5 seconds to allow the web server to start
                await Task.Delay(5 * 1000, stoppingToken);

                ExternalAccountBindingInfo ebaInfo = null;
                if (!string.IsNullOrEmpty(_acmeOptions.EabKid) && !string.IsNullOrEmpty(_acmeOptions.EabHmacKey))
                {
                    _logger.LogInformation("Using External Account Binding with kid:" + _acmeOptions.EabKid);
                    ebaInfo = new ExternalAccountBindingInfo
                    {
                        Kid = _acmeOptions.EabKid,
                        Hmac = _acmeOptions.EabHmacKey,
                    };
                }

                Contract.Assert(!string.IsNullOrEmpty(_acmeOptions.CaUrl), "CA URL must be specified in the configuration");

                Contract.Assert(_acmeOptions.AccountContactEmails.Count() > 0, "At least one account contact email must be specified in the configuration");
                Contract.Assert(_acmeOptions.DnsNames.Count() > 0, "At least one DNS name must be specified in the configuration");

                _logger.LogInformation("Starting certificate enrollment in " + _acmeOptions.CaUrl);

                ICertificateRequest csr;
                if (_acmeOptions.CertificateKeyAlgor == "rsa")
                {
                    csr = new RsaCsrProvider(HashAlgorithmName.SHA256, _acmeOptions.CertificateKeySize ?? 2048);
                }
                else
                {
                    // ECDSA key size is related to hashing
                    csr = new ECDsaCsrProvider(HashAlgorithmName.SHA256, 256);
                }

                var certificates = await _certificateEnrollment.Enroll(_acmeOptions.DnsNames.ToArray(),
                    _acmeOptions.AccountContactEmails.ToArray(),
                    new Uri(_acmeOptions.CaUrl), ebaInfo, ChallengeType.Http01,
                    csr, stoppingToken);

                var certificate = certificates.FirstOrDefault(); // leaf certificate

                _logger.LogInformation("Certificate enrollment completed");

                _logger.LogInformation("First Certificate thumbprint: {0}", certificate.Thumbprint);

                var certPem = certificates.ExportCertificatePems();
                _logger.LogInformation("Certificate PEM: {0}", certPem);
                _storage.Save("." + _acmeOptions.DnsNames.First() + ".pem", certPem);

                AsymmetricAlgorithm key = (AsymmetricAlgorithm)certificate.GetRSAPrivateKey() ?? certificate.GetECDsaPrivateKey();
                string pubKeyPem = key.ExportSubjectPublicKeyInfoPem();
                _logger.LogInformation("Public key PEM: {0}", pubKeyPem);
                _storage.Save("." + _acmeOptions.DnsNames.First() + ".pub", pubKeyPem);
                string privKeyPem = key.ExportPkcs8PrivateKeyPem();
                _logger.LogInformation("Private key PEM: {0}", privKeyPem);
                _storage.Save("." + _acmeOptions.DnsNames.First() + ".key", privKeyPem);

                var pkcs12Bytes = certificates.Export(X509ContentType.Pkcs12);
                _storage.Save("." + _acmeOptions.DnsNames.First() + ".pfx", pkcs12Bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during certificate enrollment");
            }
        }
    }
}
