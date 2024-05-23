using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Security.Cryptography;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Crypto.JOSE.Impl;


namespace ACMESharp.Protocol.Resources
{
    public class ExternalAccountBindingInfo
    {
        public string Kid { get; set; }

        public string Hmac {get;set;}

        public JwsSignedPayload ToPayload(IJwsTool accountSigner, ServiceDirectory serviceDirectory)
        {
            if (accountSigner == null)
                throw new ArgumentNullException(nameof(accountSigner));

            if (serviceDirectory == null)
                throw new ArgumentNullException(nameof(serviceDirectory));

            if (string.IsNullOrEmpty(Kid))
                throw new ArgumentNullException(nameof(Kid));
                
            if (string.IsNullOrEmpty(Hmac))
                throw new ArgumentNullException(nameof(Hmac));
            
            var eabHmacKey = ACMESharp.Crypto.CryptoHelper.Base64.UrlDecode(Hmac);
            var signAlg = new HMACSHA256(eabHmacKey);
            var eabProtectedHeader = new Dictionary<string, string>
                {
                    {"alg", "HS256" },
                    {"kid", Kid },
                    {"url", serviceDirectory.NewAccount }
                };
            var ebaJwkPayload = JsonConvert.SerializeObject(accountSigner.ExportJwk());
            var eab = JwsHelper.SignFlatJsonAsObject(bts => signAlg.ComputeHash(bts), ebaJwkPayload, eabProtectedHeader);
            return eab;
        }
    }
}