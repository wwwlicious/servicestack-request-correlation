// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace DemoService
{
    using System;
    using System.Diagnostics;
    using Funq;
    using ServiceStack;
    using ServiceStack.Logging;
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

            LogManager.LogFactory = new ConsoleLogFactory();

            // Need to register a ServiceGatewayFactory or it falls over
            //Container.Register<IServiceGatewayFactory>(x => new MyGatewayFactory()).ReusedWithin(ReuseScope.None);

            // Default plugin with x-mac-requestId headername and Rustflakes generator
            // Plugins.Add(new RequestCorrelationFeature());

            // Customised plugin
            Plugins.Add(new RequestCorrelationFeature
            {
                HeaderName = HeaderNames.CorrelationId,
                IdentityGenerator = new IncrementingIdentityGenerator()
            });
        }
    }


    public class DemoResponse
    {
        public string Message { get; set; }
    }
    
    public static class HeaderNames
    {
        public const string CorrelationId = "x-my-requestId";
    }

    public class DateTimeIdentityGenerator : IIdentityGenerator
    {
        public string GenerateIdentity()
        {
            return DateTime.Now.ToString("O");
        }
    }

    public class IncrementingIdentityGenerator : IIdentityGenerator
    {
        private int count;
        public string GenerateIdentity()
        {
            return (++count).ToString();
        }
    }
}