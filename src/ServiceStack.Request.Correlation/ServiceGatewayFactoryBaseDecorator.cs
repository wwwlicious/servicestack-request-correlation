namespace ServiceStack.Request.Correlation
{
    using System;
    using Logging;
    using Web;

    public class ServiceGatewayFactoryBaseDecorator : ServiceGatewayFactoryBase
    {
        private readonly string headerName;
        private ServiceGatewayFactoryBase gatewayFactory;
        private readonly ILog log = LogManager.GetLogger(typeof(ServiceGatewayFactoryBaseDecorator));
        private string correlationId;

        public ServiceGatewayFactoryBaseDecorator(string headerName, ServiceGatewayFactoryBase factory)
        {
            this.headerName = headerName;
            gatewayFactory = factory;
        }

        public override IServiceGateway GetServiceGateway(IRequest request)
        {
            correlationId = request.Headers[headerName] ?? request.Items[headerName].ToString();

            // This call needs to be made to ensure the internal localGateway is setup
            gatewayFactory.GetServiceGateway(request);
            return this;
        }

        public override IServiceGateway GetGateway(Type requestType)
        {
            log.Debug($"Setting {headerName} header to {correlationId} in decorator factory");

            var serviceGateway = gatewayFactory.GetGateway(requestType);

            var serviceClientBase = serviceGateway as ServiceClientBase;
            if (serviceClientBase == null)
            {
                // Internal call, no need to do anything as using same request/response objects
                log.Debug("ServiceClientBase null, setting header on requests");
                return serviceGateway;
            }

            // External call, add this to the headers collection
            log.Debug("ServiceClientBase not null, setting header collection for new requests");
            serviceClientBase.Headers.Set(headerName, correlationId);

            return serviceClientBase;
        }
    }
}