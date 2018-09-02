// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{
    using System.Globalization;
    using Interfaces;
    using RustFlakes;

    public class RustFlakesIdentityGenerator : IIdentityGenerator
    {
        // NOTE DecimalOxidation is not the fastest nor smallest but happy medium
        private readonly DecimalOxidation generator;

        public RustFlakesIdentityGenerator()
        {
            var machineIdentifier = MachineIdentity.GetMachineIdentifier();
            generator = new DecimalOxidation(machineIdentifier);
        }

        public string GenerateIdentity()
        {
            return generator.Oxidize().ToString(CultureInfo.InvariantCulture);
        }
    }
}