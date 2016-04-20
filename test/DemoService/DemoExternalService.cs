// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
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