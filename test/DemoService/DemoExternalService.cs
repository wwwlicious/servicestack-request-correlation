namespace DemoService
{
    using ServiceStack;
    using ServiceStack.Text;

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
}