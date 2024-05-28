using System;
using System.Collections.Generic;
using System.Text;
using ACMESharp.Crypto.JOSE;
using Newtonsoft.Json;

namespace ACMESharp.Enrollment
{
    public class EnrollmentAccountKey
    {
        public EnrollmentAccountKey(IJwsTool jwsTool)
        {
            JwsAlg = jwsTool.JwsAlg;
            Key = jwsTool.Export();
        }
        [JsonConstructor]
        public EnrollmentAccountKey(string JwsAlg, string Key)
        {
            this.JwsAlg = JwsAlg;
            this.Key = Key;
        }
        public string JwsAlg { get; private set; }
        public string Key { get; private set; }

        public IJwsTool GenerateTool()
        {
            if (JwsAlg.StartsWith("ES"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
                tool.HashSize = int.Parse(JwsAlg.Substring(2));
                tool.Init();
                tool.Import(Key);
                return tool;
            }

            if (JwsAlg.StartsWith("RS"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.RSJwsTool();
                tool.HashSize = int.Parse(JwsAlg.Substring(2));
                tool.Init();
                tool.Import(Key);
                return tool;
            }

            throw new Exception($"Unknown or unsupported KeyType [{JwsAlg}]");
        }
    }
}
