using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ACMESharp.Enrollment
{
    public interface ICertificateRequest
    {
        byte[] GenerateCertificateRequest(string commonName, IEnumerable<string> dnsNames);
        X509Certificate2Collection Complete(byte[] certificate);
    }

    public interface ICsrHandler
    {
        byte[] CSR { get; }
        X509Certificate2Collection Complete(byte[] certificate);
    }
}
