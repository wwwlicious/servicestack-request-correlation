// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation
{
    using System;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;
    using Logging;

    // TODO Extract this to an interface to allow it to be switched out.
    // TODO Create Docker implementation. Use hash of Docker ContainerName + ImageName to get unique. Add on startup as outside concern.
    public class MachineIdentity
    {
        private static readonly ILog log;

        static MachineIdentity()
        {
            log = LogManager.GetLogger(typeof(MachineIdentity));
        }

        // Give 1 second to generate the identifier
        private const int TimeOut = 1000;

        /// <summary>
        /// Gets a unique machine identifier from the machines MAC address.
        /// </summary>
        /// <returns></returns>
        public static uint GetMachineIdentifier()
        {
            // Get the first MAC address and use that as machine identifier.
            try
            {
                var task = Task.Factory.StartNew(GetMacAddressBasedIdentifier);

                if (!task.Wait(TimeOut))
                {
                    throw new TimeoutException("Timeout exceeded generating machine identifier");
                }

                // No MAC addresses, return current timestamp.
                return task.Result ?? Convert.ToUInt32(DateTime.Now.TimeOfDay.TotalMilliseconds);
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    log.Error("Error getting machine identifier", e);
                }

                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error getting machine identifier", ex);
                throw;
            }           
        }

        private static uint? GetMacAddressBasedIdentifier()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // discard because of standard reasons
                if (IsLoopBackOrTunnel(ni))
                    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if (IsVirtualCard(ni))
                    continue;

                if (IsMicrosoftLoopback(ni))
                    continue;

                var address = ni.GetPhysicalAddress();
                var bytes = address.GetAddressBytes();

                if (bytes.Length > 0)
                {
                    var uniqueId = BitConverter.ToUInt32(bytes, 0);
                    return uniqueId;
                }
            }

            log.Info("Failed to get a unique identifier from mac address");
            return null;
        }

        private static bool IsLoopBackOrTunnel(NetworkInterface ni)
        {
            return (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                   (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel);
        }

        private static bool IsVirtualCard(NetworkInterface ni)
        {

            const string @virtual = "virtual";
            return (ni.Description.IndexOf(@virtual, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (ni.Name.IndexOf(@virtual, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsMicrosoftLoopback(NetworkInterface ni)
        {
            // "Microsoft Loopback Adapter" will not show as NetworkInterfaceType.Loopback
            const string loopbackAdapter = "Microsoft Loopback Adapter";
            return ni.Description.Equals(loopbackAdapter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
