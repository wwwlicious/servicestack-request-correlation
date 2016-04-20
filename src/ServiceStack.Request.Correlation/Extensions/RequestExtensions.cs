// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Request.Correlation.Extensions
{
    using Web;
    using System.Collections.Generic;

    public static class RequestExtensions
    {
        public static string GetCorrelationId(this IRequest request, string headerName)
        {
            var correlationId = request.Headers[headerName];

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                object correlationObj;
                return request.Items.TryGetValue(headerName, out correlationObj) ? correlationObj.ToString() : null;
            }

            return correlationId;
        }
    }
}