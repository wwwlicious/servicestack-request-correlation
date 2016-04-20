namespace DemoService
{
    using System;
    using ServiceStack;

    public class MyGatewayFactory : ServiceGatewayFactoryBase
    {
        // from https://forums.servicestack.net/t/servicestack-discovery-consul-rfc/2042/21
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