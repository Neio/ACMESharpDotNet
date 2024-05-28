using System;
using System.Collections.Generic;
using System.Text;

namespace ACMESharp.Enrollment
{
    class OrderStates
    {
        public const string Pending = "pending";
        public const string Ready = "ready";
        public const string Processing = "processing";
        public const string Valid = "valid";
        public const string Invalid = "invalid";

        /*
         *  pending --------------+
               |                  |
               | All authz        |
               | "valid"          |
               V                  |
             ready ---------------+
               |                  |
               | Receive          |
               | finalize         |
               | request          |
               V                  |
           processing ------------+
               |                  |
               | Certificate      | Error or
               | issued           | Authorization failure
               V                  V
             valid             invalid

                State Transitions for Order Objects
         * */
    }
}
