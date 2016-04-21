// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Request.Correlation
{
    using System;
    using Extensions;
    using Web;

    public class ServiceGatewayFactoryBaseDecorator : ServiceGatewayFactoryBase
    {
        private readonly string headerName;
        private readonly ServiceGatewayFactoryBase gatewayFactory;
        private string correlationId;

        public ServiceGatewayFactoryBaseDecorator(string headerName, ServiceGatewayFactoryBase factory)
        {
            headerName.ThrowIfNullOrEmpty(nameof(headerName));
            factory.ThrowIfNull(nameof(factory));

            this.headerName = headerName;
            gatewayFactory = factory;
        }

        public override IServiceGateway GetServiceGateway(IRequest request)
        {
            correlationId = request.GetCorrelationId(headerName);

            // This call needs to be made to ensure the internal localGateway is setup
            gatewayFactory.GetServiceGateway(request);
            return this;
        }

        public override IServiceGateway GetGateway(Type requestType)
        {
            var serviceGateway = gatewayFactory.GetGateway(requestType);

            if (string.IsNullOrEmpty(correlationId))
            {
                return serviceGateway;
            }

            var restClient = serviceGateway as IRestClient;
            if (restClient == null)
            {
                // Internal call, no need to do anything as using same request/response objects
                return serviceGateway;
            }

            // External call, add this to the headers collection to be added to outgoing request object
            restClient.AddHeader(headerName, correlationId);
            return serviceGateway;
        }
    }
}