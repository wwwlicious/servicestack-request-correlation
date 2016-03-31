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
            appHost.PreRequestFilters.Insert(0, ProcessRequest);
        }

        public virtual void ProcessRequest(IRequest request, IResponse response)
        {
            // Check for existance of header. If not there add it in
            var requestId = request.Headers[HeaderName];
            if (string.IsNullOrWhiteSpace(requestId))
            {
                requestId = GenerateRequestId();
                request.Headers[HeaderName] = requestId;
            }

            // Ensure it's in the response too
            response.AddHeader(HeaderName, requestId);
        }

        private string GenerateRequestId()
        {
            return IdentityGenerator.GenerateIdentity();
        }
    }
}
