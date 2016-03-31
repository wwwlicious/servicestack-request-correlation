// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation.Interfaces
{
    /// <summary>
    /// Contains method used to generate request correlation id
    /// </summary>
    public interface IIdentityGenerator
    {
        /// <summary>
        /// Generate a string that will uniquely identify a request
        /// </summary>
        /// <returns></returns>
        string GenerateIdentity();
    }
}