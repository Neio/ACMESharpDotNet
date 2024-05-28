using System.Collections.Concurrent;
using ACMESharp.Authorizations;
using ACMESharp.Enrollment;

namespace ACMEWeb
{
    public class HttpChallaneProvider : IChallengeProvider
    {
        private ConcurrentDictionary<string, string> _challenges = new ConcurrentDictionary<string, string>();
        private readonly ILogger<HttpChallaneProvider> _logger;

        public HttpChallaneProvider(ILogger<HttpChallaneProvider> logger)
        {
            _logger = logger;
        }

        public Task<bool> CleanUpChallenge(IChallengeValidationDetails challenge, CancellationToken cancel = default)
        {
            if (challenge is Http01ChallengeValidationDetails httpChallenge)
            {
                _challenges.TryRemove(httpChallenge.HttpResourcePath, out _);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
            
        }

        public Task<bool> CompleteChallenge(IChallengeValidationDetails challenge, CancellationToken cancel = default)
        {
            if (challenge is Http01ChallengeValidationDetails httpChallenge)
            {
                var token = httpChallenge.HttpResourcePath.Split('/').Last();
                _logger.LogInformation($"HttpChallenge: {token} = {httpChallenge.HttpResourceValue}");
                _challenges[token] = httpChallenge.HttpResourceValue;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }


        public string ValidateChallenge(string token)
        {
            _logger.LogInformation($"Validating challenge for token: {token}");
            if (_challenges.TryGetValue(token, out var value))
            {
                return value;
            }
            else
            {
                return "unknown";
            }
        }
    }
}
