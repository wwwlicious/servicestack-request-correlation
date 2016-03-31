// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{
    using System.Globalization;
    using Interfaces;
    using RustFlakes;

    public class RustflakesIdentityGenerator : IIdentityGenerator
    {
        // NOTE DecimalOxidation is not the fastest nor smallest but happy medium
        private readonly DecimalOxidation _generator;

        public RustflakesIdentityGenerator()
        {
            var machineIdentifier = MachineIdentity.GetMachineIdentifier();
            _generator = new DecimalOxidation(machineIdentifier);
        }

        public string GenerateIdentity()
        {
            return _generator.Oxidize().ToString(CultureInfo.InvariantCulture);
        }
    }
}