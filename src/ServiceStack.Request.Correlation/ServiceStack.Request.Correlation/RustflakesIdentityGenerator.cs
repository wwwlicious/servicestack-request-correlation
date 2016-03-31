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
