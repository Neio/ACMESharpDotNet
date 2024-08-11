namespace ACMESharp.Enrollment
{
    class AuthroizationStates
    {
        public const string Pending = "pending";
        public const string Valid = "valid";
        public const string Invalid = "invalid";
        public const string Deactivated = "deactivated";
        public const string Revoked = "revoked";
        public const string Expired = "expired";
        /*
         *                        pending --------------------+
                                     |                        |
                   Challenge failure |                        |
                          or         |                        |
                         Error       |  Challenge valid       |
                           +---------+---------+              |
                           |                   |              |
                           V                   V              |
                        invalid              valid            |
                                               |              |
                                               |              |
                                               |              |
                                +--------------+--------------+
                                |              |              |
                                |              |              |
                         Server |       Client |   Time after |
                         revoke |   deactivate |    "expires" |
                                V              V              V
                             revoked      deactivated      expired

        State Transitions for Authorization Objects
         */
    }
}
