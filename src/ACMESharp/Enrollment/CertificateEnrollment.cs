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
using System.Net.Http;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACMESharp.Enrollment
{
    public partial class CertificateEnrollment
    {
        // This is ACME Certificate enrollment

        private readonly IStorage _storage;
        private readonly IChallengeProvider _challengeProvider;

        public TimeSpan AuthorizationTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan FinalizeOrderTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public Action<string, object> BeforeAcmeSign { get; set; }
        public Action<EnrollmentProgress, string> ProgressUpdate { get; set; }

        public Action<string, HttpRequestMessage> BeforeHttpSend { get; set; }

        public Action<string, HttpResponseMessage> AfterHttpSend { get; set; }

        public CertificateEnrollment(IStorage storage, IChallengeProvider challengeProvider)
        {
            _storage = storage;
            _challengeProvider = challengeProvider;
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

            var serverShortName = server?.Host.Replace(".", "_");

            var directory = _storage.Load<ServiceDirectory>("directory");

            if (directory != null && server == null)
            {
                server = new Uri(directory.Directory);
            }

            if (server == null)
                throw new InvalidOperationException("Must specify the ACME server or have saved service directory to use");

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
                usePostAsGet: true))
            {
                client.BeforeAcmeSign = BeforeAcmeSign;
                client.BeforeHttpSend = BeforeHttpSend;
                client.AfterHttpSend = AfterHttpSend;

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

                ProgressUpdate?.Invoke(EnrollmentProgress.AccountResolved, account.Kid);
            }
            else
            {

                state.Client.Account = state.AccountDetails;
                ProgressUpdate?.Invoke(EnrollmentProgress.AccountResolved, state.AccountDetails.Kid);
            }

            return true;
        }
        private async Task<bool> CreateOrRefreshOrderAsync(EnrollmentState state)
        {
            if (state.OrderDetails == null)
            {
                var order = await state.Client.CreateOrderAsync(state.DnsNames, cancel: state.CancellationToken);
                state.OrderDetails = order;
                ProgressUpdate?.Invoke(EnrollmentProgress.OrderCreated, order.OrderUrl);
            }
            else
            {
                var newDetails = await state.Client.GetOrderDetailsAsync(state.OrderDetails.OrderUrl, state.OrderDetails, cancel: state.CancellationToken);
                newDetails.OrderUrl = state.OrderDetails.OrderUrl;
                state.OrderDetails = newDetails;
                ProgressUpdate?.Invoke(EnrollmentProgress.OrderRefreshed, newDetails.OrderUrl);
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
                        
                        ProgressUpdate?.Invoke(EnrollmentProgress.PendingAuthorization, "Challenge Handler has handled challenge:" +
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

                ProgressUpdate?.Invoke(EnrollmentProgress.PendingAuthorization, "Waiting for Authorization to be validated. Would stop retry at " + authTimeout);
                authz = await state.Client.GetAuthorizationDetailsAsync(authUrl, state.CancellationToken);
            }
            if (authz.Status != AuthroizationStates.Valid)
            {
                ProgressUpdate?.Invoke(EnrollmentProgress.AuthorizationFailure, "Authorization is not valid: " + authz.Status);
                throw new InvalidOperationException("Failed to validate authorization");
            }
            ProgressUpdate?.Invoke(EnrollmentProgress.AuthorizationComplete, "Authorization is valid: " + authUrl);
        }
        

        private async Task<bool> FinalizeOrderAsync(EnrollmentState state)
        {

            await CreateOrRefreshOrderAsync(state);


            var order = state.OrderDetails;
            var csrBytes = state.Csr.GenerateCertificateRequest("CN=" + state.DnsNames.First(), state.DnsNames);
            var finalizedOrder = await state.Client.FinalizeOrderAsync(order.Payload.Finalize, csrBytes, state.CancellationToken);
            finalizedOrder.OrderUrl = state.OrderDetails.OrderUrl;
            state.OrderDetails = finalizedOrder;
            ProgressUpdate?.Invoke(EnrollmentProgress.OrderFinalized, order.Payload.Finalize);
            return true;
        }

        private async Task<X509Certificate2Collection> DownloadCertificate(EnrollmentState state)
        {

            DateTime start = DateTime.UtcNow;

            while (string.IsNullOrEmpty(state.OrderDetails?.Payload?.Certificate))
            {
                await CreateOrRefreshOrderAsync(state);

                if (state.OrderDetails.Payload.Status == OrderStates.Invalid)
                    throw new InvalidOperationException("Order is invalid");

                if (DateTime.UtcNow - start > FinalizeOrderTimeout)
                    throw new InvalidOperationException("Timed out waiting for certificate to be issued");

                if (!string.IsNullOrEmpty(state.OrderDetails.Payload.Certificate))
                {
                    break;
                }

                ProgressUpdate?.Invoke(EnrollmentProgress.CertificatePending, "Waiting for certificate to be issued");
                await Task.Delay(5000, state.CancellationToken);


                if (state.CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();

            }

            ProgressUpdate?.Invoke(EnrollmentProgress.CertificateIssued, state.OrderDetails.Payload.Certificate);
            var certBytes = await state.Client.GetOrderCertificateAsync(state.OrderDetails, state.CancellationToken);

            return state.Csr.Complete(certBytes);
        }
    }

    public enum EnrollmentProgress
    {
        AccountResolved,
        OrderCreated,
        OrderRefreshed,
        PendingAuthorization,
        AuthorizationFailure,
        AuthorizationComplete,
        OrderFinalized,
        CertificatePending,
        CertificateIssued,
    }
}
