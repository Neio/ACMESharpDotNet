namespace ACMESharp.Enrollment
{
    class ChallengeStates
    {
        public const string Pending = "pending";
        public const string Processing = "processing";
        public const string Valid = "valid";
        public const string Invalid = "invalid";


        /*  https://datatracker.ietf.org/doc/html/rfc8555#section-7.1.6
         *             pending
                           |
                           | Receive
                           | response
                           V
                       processing <-+
                           |   |    | Server retry or
                           |   |    | client retry request
                           |   +----+
                           |
                           |
               Successful  |   Failed
               validation  |   validation
                 +---------+---------+
                 |                   |
                 V                   V
               valid              invalid

         * */
    }
}
