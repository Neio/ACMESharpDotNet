using System;

namespace ACMESharp.Enrollment
{
    [Flags]
    public enum ChallengeType : uint
    {
        Dns01 = 1,
        Http01 = 2,
        TlsAlpn01 = 4
    }
}
