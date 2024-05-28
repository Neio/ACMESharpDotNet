using System.Diagnostics;
using System.IO;
using System;

namespace PKISharp.SimplePKI.UnitTests
{
    public static class OpenSsl
    {
        public const string OpenSslLightPath = @"C:\Program Files\OpenSSL\bin\openssl.exe";

        public static Process Start(string arguments)
        {
            if (File.Exists(OpenSslLightPath))
            {
                return Process.Start(OpenSslLightPath, arguments);
            }
            else
            {
                Console.WriteLine("openssl " + arguments);
                return Process.Start("openssl", arguments);
            }
        }       
    }
}