// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace DemoService
{
    using ServiceStack;
    using ServiceStack.Text;

    public class DemoService : Service
    {
        public object Any(DemoRequest demoRequest)
        {
            var header = Request.Headers[HeaderNames.CorrelationId];

            // Request.Items[HeaderNames.CorrelationId]

            $"Demo service received request with {header} value for {HeaderNames.CorrelationId} header".Print();

            return new DemoResponse { Message = $"Internal request id = {header}" };
        }
    }

    public class DemoRequest : IReturn<DemoResponse>
    {
    }
}