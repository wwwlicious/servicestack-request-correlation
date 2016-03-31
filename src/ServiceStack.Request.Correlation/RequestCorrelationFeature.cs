// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{
    using Interfaces;
    using ServiceStack;
    using Web;

    public class RequestCorrelationFeature : IPlugin
    {
        public string HeaderName { get; set; } = "x-mac-requestId";

        public IIdentityGenerator IdentityGenerator { get; set; } = new RustflakesIdentityGenerator();

        public void Register(IAppHost appHost)
        {
            appHost.PreRequestFilters.InsertAsFirst(ProcessRequest);
        }

        public virtual void ProcessRequest(IRequest request, IResponse response)
        {
            // Check for existence of header. If not there add it in
            var requestId = request.Headers[HeaderName];
            if (string.IsNullOrWhiteSpace(requestId))
            {
                requestId = SetRequestId(request);
            }

            SetResponseId(response, requestId);
        }

        private string SetRequestId(IRequest request)
        {
            var requestId = GenerateRequestId();
            request.Headers[HeaderName] = requestId;
            return requestId;
        }

        private void SetResponseId(IResponse response, string requestId)
        {
            response.AddHeader(HeaderName, requestId);
        }

        private string GenerateRequestId()
        {
            return IdentityGenerator.GenerateIdentity();
        }
    }
}
