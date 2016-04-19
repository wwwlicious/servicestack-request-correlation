namespace ServiceStack.Request.Correlation
{
    using System;
    using Logging;
    using Web;

    public class ServiceGatewayFactoryBaseDecorator : ServiceGatewayFactoryBase
    {
        private readonly string headerName;
        private ServiceGatewayFactoryBase decorated;
        private readonly ILog log = LogManager.GetLogger(typeof(ServiceGatewayFactoryBaseDecorator));
        private string correlationId;

        public ServiceGatewayFactoryBaseDecorator(string headerName, ServiceGatewayFactoryBase factory)
        {
            this.headerName = headerName;
            decorated = factory;
        }

        public override IServiceGateway GetServiceGateway(IRequest request)
        {
            correlationId = request.Headers[headerName];

            // This call needs to be made to ensure the internal localGateway is setup
            decorated.GetServiceGateway(request);
            return this;
        }

        public override IServiceGateway GetGateway(Type requestType)
        {
            log.Debug($"Setting {headerName} header to {correlationId} in decorator factory");

            var serviceGateway = decorated.GetGateway(requestType);

            var serviceClientBase = serviceGateway as ServiceClientBase;
            if (serviceClientBase == null)
            {
                // Internal call, no need to do anything as using same request/response objects
                log.Debug("ServiceClientBase null, setting header on requests");
                return serviceGateway;
            }

            // External call, as this to the headers collection
            log.Debug("ServiceClientBase not null, setting header collection for new requests");
            serviceClientBase.Headers.Set(headerName, correlationId);

            return serviceClientBase;
        }
    }
}