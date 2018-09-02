// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
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