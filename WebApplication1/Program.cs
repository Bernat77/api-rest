using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Globalization;

namespace WebApplication1
{
    public class Program
    {
        private const string clientId = "9c7bada2-e186-4e21-80e2-909f47c5cbd7";
        private const string aadInstance = "http://login.microsoftonline.com/{0}";
        private const string tenant = "bernattesthotmail.onmicrosoft.com";
        private const string resource = "https://graph.windows.net";
        private const string appKey = "igwsvBX27*)=ubTJSPI045+";
        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);



        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
