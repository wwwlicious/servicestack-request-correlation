// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{ 
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

            appHost.GlobalResponseFilters.Add(SetResponseCorrelationId);
        }

        public virtual void ProcessRequest(IRequest request, IResponse response)
        {
            // Check for existence of header. If not there add it in
            var correlationId = request.Headers[HeaderName];
            log.Debug($"Got correlation Id {correlationId ?? "<notFound>" } from header {HeaderName} from incoming request object");
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = IdentityGenerator.GenerateIdentity();
                request.Headers[HeaderName] = correlationId;
                log.Debug($"Generated new correlation Id {correlationId} for header {HeaderName} on incoming request object");
            }

            request.Items[HeaderName] = correlationId;

            //SetResponseId(response, correlationId);
            log.Debug($"Setting correlation Id {correlationId} to header {HeaderName} on response object");
        }

        public virtual void SetResponseCorrelationId(IRequest request, IResponse response, object dto)
        {
            var correlationId = GetRequestId(request);
            SetResponseId(response, correlationId);
        }

        private void SetResponseId(IResponse response, string requestId)
        {
            response.AddHeader(HeaderName, requestId);
        }

        private string GetRequestId(IRequest request)
        {
            return request.Headers[HeaderName];
        }

        public void AfterPluginsLoaded(IAppHost appHost)
        {
            // Check if an IServiceGatewayFactory has been registered
            var factory = appHost.GetContainer().TryResolve<IServiceGatewayFactory>();

            var factoryBase = factory as ServiceGatewayFactoryBase;
            if (factoryBase != null)
                appHost.Register<IServiceGatewayFactory>(new ServiceGatewayFactoryBaseDecorator(HeaderName, factoryBase));
        }
    }
}
