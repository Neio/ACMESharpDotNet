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

    class AcmeRevokeService
    {
        private readonly IStorage _storage;
        private readonly IOptions<AcmeOptions> _options;
        private readonly ILogger<AcmeRevokeService> _logger;
        public AcmeRevokeService(IStorage storage, ILogger<AcmeRevokeService> logger, IOptions<AcmeOptions> options)
        {
            _storage = storage;
            _logger = logger;
            _options = options;
        }

        public async Task RevokeCertificate()
        {
            Contract.Assert(!string.IsNullOrEmpty(_options.Value.CaUrl), "CA URL must be specified in the configuration");
            var url = new Uri(_options.Value.CaUrl);
            var hostname = _options.Value.DnsNames.First();
            var cert = _storage.Load<byte[]>($".{hostname}.pfx");
            if (cert == null)
            {
                _logger.LogWarning($"Certificate for {hostname} not found");
                return;
            }
            _logger.LogInformation($"Revoking certificate for {hostname} on {url}");
            using (var certObj = new X509Certificate2(cert, default(string), X509KeyStorageFlags.Exportable))
            {
                _logger.LogInformation($"Certificate thumpbrint: {certObj.Thumbprint}");
                var certRevocation = new CertificateRevocation();


                certRevocation.BeforeAcmeSign = (url, obj) => 
                {
                    var objStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    _logger.LogInformation($"Before Acme Sign {objStr}");
                };
                certRevocation.BeforeHttpSend = (url, obj) => 
                {
                    
                    _logger.LogInformation($"Before Http Send {obj.Content?.ReadAsStringAsync().Result}");
                };
                await certRevocation.Revoke(certObj, url);
            }

            
        }
    }
}
