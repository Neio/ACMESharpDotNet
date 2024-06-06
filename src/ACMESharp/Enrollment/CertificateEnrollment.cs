using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Authorizations;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACMESharp.Enrollment
{
    public partial class CertificateEnrollment
    {
        // This is ACME Certificate enrollment

        private readonly IStorage _storage;
        private readonly IChallengeProvider _challengeProvider;
        private readonly ILogger _logger;

        public TimeSpan AuthorizationTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan FinalizeOrderTimeout { get; set; } = TimeSpan.FromMinutes(5);

        public CertificateEnrollment(IStorage storage, IChallengeProvider challengeProvider, ILogger logger)
        {
            _storage = storage;
            _challengeProvider = challengeProvider;
            _logger = logger;
        }

        public async Task<X509Certificate2Collection> Enroll(string[] dnsNames,
            string[] emails, 
            Uri AcmeServer,
            ExternalAccountBindingInfo ExternalAccountBinding,
            ChallengeType challageType,
            ICertificateRequest certHandler,
            CancellationToken token)
        {
            var server = AcmeServer;

            if (server == null)
                throw new InvalidOperationException("Must specify the ACME server or have saved service directory to use");
            var serverShortName = server?.Host.Replace(".", "_");

            var directory = _storage.Load<ServiceDirectory>("directory");

            if (directory != null && server == null)
            {
                server = new Uri(directory.Directory);
            }


            if (dnsNames == null || dnsNames.Length == 0)
                throw new InvalidOperationException("Must specify at least one DNS name");

            if (emails == null || emails.Length == 0)
                throw new InvalidOperationException("Must specify at least one email address");

            var state = new EnrollmentState
            {
                ServerNameShort = serverShortName,
                Directory = directory,
                AcmeServer = server,
                ExternalAccountBinding = ExternalAccountBinding,
                CancellationToken = token,
                DnsNames = dnsNames,
                Contacts = emails.Select(email => "mailto:" + email).ToArray(),
                ChallengeType = challageType,
                Csr = certHandler
            };

            var signerInfo = _storage.Load<EnrollmentAccountKey>("accountkey");
            var signer = signerInfo?.GenerateTool();

            using (var client = new AcmeProtocolClient(server,
                signer: signer,
                logger: _logger,
                usePostAsGet: true))
            {
                state.Client = client;

                if (signer == null)
                {
                    signer = client.Signer;
                    _storage.Save("accountkey", new EnrollmentAccountKey(signer));
                }

                await ResolveServiceDirectory(state);

                await client.GetNonceAsync();

                await ResolveAccount(state);

                await CreateOrRefreshOrderAsync(state);

                await ResolveChallenge(state);

                await FinalizeOrderAsync(state);

                return await DownloadCertificate(state);
            }
        }

        private async Task<bool> ResolveServiceDirectory(EnrollmentState state)
        {
            var directory = state.Directory;
            var client = state.Client;
            if (directory == null)
            {
                directory = await client.GetDirectoryAsync(state.CancellationToken);
                _storage.Save("directory", directory);
            }

            state.Directory = directory;
            client.Directory = directory;

            return true;
        }   

        private async Task<bool> ResolveAccount(EnrollmentState state)
        {

            state.AccountDetails = _storage.Load<AccountDetails>("accountdetails");
            if (state.AccountDetails == null)
            {
                var account = await state.Client.CreateAccountAsync(state.Contacts, true, state.ExternalAccountBinding, cancel: state.CancellationToken);

                _storage.Save("accountdetails", account);
                state.Client.Account = account;
                state.AccountDetails = account;

            }
            else
            {

                state.Client.Account = state.AccountDetails;
            }

            return true;
        }
        private async Task<bool> CreateOrRefreshOrderAsync(EnrollmentState state)
        {
            if (state.OrderDetails == null)
            {
                _logger.LogInformation("Creating new Order");
                var order = await state.Client.CreateOrderAsync(state.DnsNames, cancel: state.CancellationToken);
                state.OrderDetails = order;
            }
            else
            {
                _logger.LogInformation("Refreshing Order status");
                var newDetails = await state.Client.GetOrderDetailsAsync(state.OrderDetails.OrderUrl, state.OrderDetails, cancel: state.CancellationToken);
                newDetails.OrderUrl = state.OrderDetails.OrderUrl;
                state.OrderDetails = newDetails;
            }

            return true;
        }

        private async Task<bool> ResolveChallenge(EnrollmentState state)
        {
            foreach (var auth in state.OrderDetails.Payload.Authorizations)
            {
                var authz = await state.Client.GetAuthorizationDetailsAsync(auth, state.CancellationToken);

                if (authz.Status == AuthroizationStates.Valid)
                    continue;

                if (authz.Status != AuthroizationStates.Pending)
                    throw new InvalidOperationException("Authorization is not in pending state: " + authz.Status);

                List<IChallengeValidationDetails> validationDetails = new List<IChallengeValidationDetails>();

                foreach (var chlng in authz.Challenges)
                {
                    if (chlng.Status == ChallengeStates.Valid)
                        continue;

                    var chlngValidation = AuthorizationDecoder.DecodeChallengeValidation(authz, chlng.Type, state.Client.Signer);

                    if (chlng.Status == ChallengeStates.Invalid)
                    {
                        await _challengeProvider.CleanUpChallenge(chlngValidation, state.CancellationToken);
                        throw new InvalidOperationException("Failed to answer challenge");
                    }

                    if (await _challengeProvider.CompleteChallenge(chlngValidation, state.CancellationToken))
                    {
                        _logger.LogInformation("Challenge Handler has handled challenge:" +
                            JsonConvert.SerializeObject(chlngValidation, Formatting.None));

                        validationDetails.Add(chlngValidation);

                        await state.Client.AnswerChallengeAsync(chlng.Url, state.CancellationToken);

                    }
                }


                try
                {
                    await WaitForAuthorizations(auth, state);
                }
                finally
                {
                    foreach (var challenge in validationDetails)
                    {
                        await _challengeProvider.CleanUpChallenge(challenge, state.CancellationToken);
                    }
                }

            }

            return true;
        }
        private async Task WaitForAuthorizations(string authUrl, EnrollmentState state)
        {
            var authz = await state.Client.GetAuthorizationDetailsAsync(authUrl, state.CancellationToken);

            DateTime authTimeout = DateTime.UtcNow.Add(AuthorizationTimeout);
            while (authz.Status == AuthroizationStates.Pending)
            {
                await Task.Delay(10 * 1000, state.CancellationToken);

                if (state.CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

                if (authTimeout < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Timeout waiting authorization validation");
                }

                _logger.LogInformation("Waiting for Authorization to be validated. Would stop retry at  " + authTimeout);
                authz = await state.Client.GetAuthorizationDetailsAsync(authUrl, state.CancellationToken);
            }
            if (authz.Status != AuthroizationStates.Valid)
            {
                _logger.LogInformation("Authorization is not valid: " + authz.Status);
                throw new InvalidOperationException("Failed to validate authorization");
            }
        }
        

        private async Task<bool> FinalizeOrderAsync(EnrollmentState state)
        {
            _logger.LogInformation("Finalizing Order");

            await CreateOrRefreshOrderAsync(state);


            var order = state.OrderDetails;
            var csrBytes = state.Csr.GenerateCertificateRequest("CN=" + state.DnsNames.First(), state.DnsNames);
            var finalizedOrder = await state.Client.FinalizeOrderAsync(order.Payload.Finalize, csrBytes, state.CancellationToken);
            finalizedOrder.OrderUrl = state.OrderDetails.OrderUrl;
            state.OrderDetails = finalizedOrder;

            return true;
        }

        private async Task<X509Certificate2Collection> DownloadCertificate(EnrollmentState state)
        {
            _logger.LogInformation("Downloading Certificate");


            await CreateOrRefreshOrderAsync(state);
            DateTime start = DateTime.UtcNow;

            while (string.IsNullOrEmpty(state.OrderDetails.Payload.Certificate))
            {
                await CreateOrRefreshOrderAsync(state);

                if (state.OrderDetails.Payload.Status == OrderStates.Invalid)
                    throw new InvalidOperationException("Order is invalid");

                if (DateTime.UtcNow - start > FinalizeOrderTimeout)
                    throw new InvalidOperationException("Timed out waiting for certificate to be issued");

                _logger.LogInformation("Waiting...");
                await Task.Delay(5000, state.CancellationToken);


                if (state.CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

            }

            var certBytes = await state.Client.GetOrderCertificateAsync(state.OrderDetails, state.CancellationToken);
            return state.Csr.Complete(certBytes);
        }
    }
}
