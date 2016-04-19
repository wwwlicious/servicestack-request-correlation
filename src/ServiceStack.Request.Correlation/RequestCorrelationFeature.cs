// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{
    using System;
    using Interfaces;
    using Logging;
    using ServiceStack;
    using Web;

    public class RequestCorrelationFeature : IPlugin, IPostInitPlugin
    {
        public string HeaderName { get; set; } = "x-mac-requestId";

        public IIdentityGenerator IdentityGenerator { get; set; } = new RustflakesIdentityGenerator();

        private readonly ILog log = LogManager.GetLogger(typeof(RequestCorrelationFeature));

        public void Register(IAppHost appHost)
        {
            appHost.PreRequestFilters.InsertAsFirst(ProcessRequest);
        }

        public virtual void ProcessRequest(IRequest request, IResponse response)
        {
            // Check for existence of header. If not there add it in
            var requestId = request.Headers[HeaderName];
            log.Debug($"Got request Id {requestId ?? "<notFound>" } from header {HeaderName} from incoming request object");
            if (string.IsNullOrWhiteSpace(requestId))
            {
                requestId = GenerateRequestId();
                SetRequestId(request, requestId);
                log.Debug($"Generated new request Id {requestId} for header {HeaderName} on incoming request object");
            }

            SetResponseId(response, requestId);
            log.Debug($"Setting request Id {requestId} to header {HeaderName} on response object");

            // Check whether there is an IServiceGateway OR an IServiceGatewayFactory and decorate as required?
        }

        private void SetResponseId(IResponse response, string requestId)
        {
            response.AddHeader(HeaderName, requestId);
        }

        private void SetRequestId(IRequest request, string requestId)
        {
            request.Headers[HeaderName] = requestId;
        }

        private string GenerateRequestId()
        {
            return IdentityGenerator.GenerateIdentity();
        }

        public void AfterPluginsLoaded(IAppHost appHost)
        {
            // Check if an IServiceGatewayFactory has been registered
            var factory = appHost.GetContainer().TryResolve<IServiceGatewayFactory>();

            var factoryBase = factory as ServiceGatewayFactoryBase;
            if (factoryBase == null)
                throw new ApplicationException("Need a ServiceGatewayFactoryBase");
            
            appHost.Register<IServiceGatewayFactory>(new ServiceGatewayFactoryBaseDecorator(HeaderName, factoryBase));
        }
    }
}
