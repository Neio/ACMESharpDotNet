using System.IO;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace ACMESharp.IntegrationTests
{
    public class AcmeServerFixure : IDisposable
    {
        Process _acmeServer;
        public AcmeServerFixure()
        {
            var thisAsmLocation = Path.GetDirectoryName(typeof(AcmeServerFixure).Assembly.Location);
            /**
            var jsonPathBase = Path.Combine(thisAsmLocation, @"config/_IGNORE/");

            R53 = JsonConvert.DeserializeObject<R53Helper>(
                    File.ReadAllText(jsonPathBase + "R53Helper.json"));
            S3 = JsonConvert.DeserializeObject<S3Helper>(
                    File.ReadAllText(jsonPathBase + "S3Helper.json"));

            // For testing this makes it easier to repeat tests
            // that use the same DNS names and need to be refreshed
            R53.DnsRecordTtl = 60;**/
            // start in the asm location
            DNS = new DNSHelper();
            Http = new HttpHelper();
            Directory.SetCurrentDirectory(thisAsmLocation);
            
            //Task.Run(() => ACMESharp.MockServer.Program.Main(new string[0]));
            
            _acmeServer = Process.Start("./ACMESharp.MockServer");
            Thread.Sleep(10000);

            
        }

        public void Dispose()
        {
            _acmeServer.Kill();
        }

        public class DNSHelper
        {
            public async Task EditTxtRecord(string dnsName, IEnumerable<string> dnsValues, bool delete = false)
            {

            }
        }

        public class HttpHelper
        {
            public async Task EditFile(string filePath, string contentType, string content)
            {

            }
            
        }

        public DNSHelper DNS { get; }
        public HttpHelper Http { get; }

        //public R53Helper R53 { get; }

        //public S3Helper S3 { get; }
    }
}