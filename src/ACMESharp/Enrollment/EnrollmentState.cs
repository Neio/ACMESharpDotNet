using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;

namespace ACMESharp.Enrollment
{
    class EnrollmentState
    {
        public string ServerNameShort { get; set; }
        public ServiceDirectory Directory { get; set; }
        public AcmeProtocolClient Client { get; set; }
        public AccountDetails AccountDetails { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public IEnumerable<string> Contacts { get; set; }
        public Uri AcmeServer { get; set; }
        public ExternalAccountBindingInfo ExternalAccountBinding { get; set; }
        public string[] DnsNames { get; set; }
        public OrderDetails OrderDetails { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public ICertificateRequest Csr { get; set; }
    }
}
