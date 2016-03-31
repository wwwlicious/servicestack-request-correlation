// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace DemoService
{
    using System;
    using System.Diagnostics;
    using Funq;
    using ServiceStack;
    using ServiceStack.Request.Correlation;
    using ServiceStack.Request.Correlation.Interfaces;
    using ServiceStack.Text;

    class Program
    {
        static void Main(string[] args)
        {
            var serviceUrl = "http://127.0.0.1:8090/";
            new AppHost(serviceUrl).Init().Start("http://*:8090/");
            $"ServiceStack SelfHost listening at {serviceUrl} ".Print();
            Process.Start(serviceUrl);

            Console.ReadLine();
        }
    }

    public class AppHost : AppSelfHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl) : base("DemoService", typeof (DemoService).Assembly)
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                WebHostUrl = serviceUrl,
                ApiVersion = "2.0"
            });

            // Default plugin with x-mac-requestId headername and Rustflakes generator
            // Plugins.Add(new RequestCorrelationFeature());

            // Customised plugin
            Plugins.Add(new RequestCorrelationFeature
            {
                HeaderName = HeaderNames.RequestId,
                IdentityGenerator = new DateTimeIdentityGenerator()
            });
        }
    }

    public class DemoService : Service
    {
        public object Any(DemoRequest demoRequest)
        {
            var header = Request.Headers[HeaderNames.RequestId];

            $"Demo service received request with {header} value for {HeaderNames.RequestId} header".Print();

            return new DemoResponse { Message = $"Request id = {header}" };
        }
    }

    public class DemoRequest : IReturn<DemoResponse>
    {
    }

    public class DemoResponse
    {
        public string Message { get; set; }
    }

    public static class HeaderNames
    {
        public const string RequestId = "x-my-requestId";
    }

    public class DateTimeIdentityGenerator : IIdentityGenerator
    {
        public string GenerateIdentity()
        {
            return DateTime.Now.ToString("O");
        }
    }
}