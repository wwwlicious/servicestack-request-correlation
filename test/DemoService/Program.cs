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
    using ServiceStack.Web;

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
            Container.Register<IServiceGatewayFactory>(x => new MyGatewayFactory()).ReusedWithin(ReuseScope.None);

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

    public class DemoGatewayService : Service
    {
        public object Any(DemoGatewayRequest demoRequest)
        {
            if (demoRequest.Internal)
                return Gateway.Send(demoRequest.ConvertTo<DemoRequest>());

            return Gateway.Send(demoRequest.ConvertTo<DemoExternalRequest>());
        }
    }

    public class DemoGatewayRequest : IReturn<DemoResponse>
    {
        public bool Internal { get; set; }
    }

    public class DemoService : Service
    {
        public object Any(DemoRequest demoRequest)
        {
            var header = Request.Headers[HeaderNames.CorrelationId];

            // Request.Items[HeaderNames.CorrelationId]

            $"Demo service received request with {header} value for {HeaderNames.CorrelationId} header".Print();

            return new DemoResponse { Message = $"Internal request id = {header}" };
        }
    }

    public class DemoRequest : IReturn<DemoResponse>
    {
    }

    public class DemoResponse
    {
        public string Message { get; set; }
    }

    public class DemoExternalService : Service
    {
        public object Any(DemoExternalRequest demoRequest)
        {
            var header = Request.Headers[HeaderNames.CorrelationId];

            $"Demo service received external request with {header} value for {HeaderNames.CorrelationId} header".Print();

            return new DemoResponse { Message = $"External request id = {header}" };
        }
    }

    public class DemoExternalRequest : IReturn<DemoResponse>
    {
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

    public class MyGatewayFactory : ServiceGatewayFactoryBase
    {
        public override IServiceGateway GetGateway(Type requestType)
        {
            // If dto contains "External" then make an external request to it, else inProc
            var gateway = requestType.Name.Contains("External")
            ? new JsonServiceClient("http://127.0.0.1:8090/")
            : (IServiceGateway)localGateway;
            return gateway;
        }
    }
}