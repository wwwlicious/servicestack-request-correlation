namespace DemoService
{
    using ServiceStack;

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
}