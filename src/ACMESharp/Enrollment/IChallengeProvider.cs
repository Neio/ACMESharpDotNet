using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Authorizations;

namespace ACMESharp.Enrollment
{
    public interface IChallengeProvider
    {
        /// <summary>
        /// Notify the provider that a challenge is ready to be started.
        /// The provider must be validating the challenge is ready to be validated
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<bool> CompleteChallenge(IChallengeValidationDetails challenge, CancellationToken cancel = default);

        Task<bool> CleanUpChallenge(IChallengeValidationDetails challenge, CancellationToken cancel = default);

    }
}
