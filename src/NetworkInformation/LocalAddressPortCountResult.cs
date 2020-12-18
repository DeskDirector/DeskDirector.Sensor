using System;
using System.Linq;

namespace DdManager.Sensor.NetworkInformation
{
    public record LocalAddressPortCountResult
    {
        public string Address { get; }

        public int Count { get; }

        public uint Allowed { get; }

        public double Percentage { get; }

        public ProcessCount[] TopProcessCounts { get; }

        public RemoteAddressCount[] TopRemoteAccessCounts { get; }

        public LocalAddressPortCountResult(LocalAddressPortCount local)
        {
            if (local == null) {
                throw new ArgumentNullException(nameof(local));
            }

            Address = local.Address;
            Count = local.Count;
            Allowed = local.Allowed;

            Percentage = CalculatePercentage(local);

            TopProcessCounts = local.ProcessCountMap.Values
                .OrderByDescending(p => p.Count)
                .Take(3)
                .ToArray();
            TopRemoteAccessCounts = local.RemoteAccessCounts.Values
                .OrderByDescending(r => r.Count)
                .Take(3)
                .ToArray();
        }

        private static double CalculatePercentage(LocalAddressPortCount local)
        {
            uint allowed = local.Allowed;
            int count = local.Count;
            if (allowed == 0 || count <= 0) {
                return 0;
            }

            return Math.Round(count / (double)allowed, 3) * 100;
        }
    }
}