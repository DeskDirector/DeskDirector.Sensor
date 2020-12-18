using System;
using System.Collections.Generic;
using System.Linq;

namespace DdManager.Sensor.NetworkInformation
{
    public record TcpConnectionAnalyzeResult
    {
        /// <summary>
        /// Amount of Ephemeral Port used by each local Address
        /// Key => (IP)
        /// </summary>
        public IReadOnlyList<LocalAddressPortCountResult> LocalAddressPortCounts { get; }

        /// <summary>
        /// Amount of connection used to access a given remote address
        /// Key => (IP:Port)
        /// </summary>
        public IReadOnlyList<RemoteAddressCount> RemoteAccessCounts { get; }

        public TcpConnectionAnalyzeResult(
            IReadOnlyDictionary<string, LocalAddressPortCount> localPorts,
            IReadOnlyDictionary<string, RemoteAddressCount> remote)
        {
            if (localPorts == null) {
                throw new ArgumentNullException(nameof(localPorts));
            }
            if (remote == null) {
                throw new ArgumentNullException(nameof(remote));
            }

            LocalAddressPortCounts = localPorts
                .Values
                .OrderByDescending(r => r.Count)
                .Take(3)
                .Select(r => r.Seal())
                .ToArray();
            RemoteAccessCounts = remote
                .Values
                .OrderByDescending(r => r.Count)
                .Take(3)
                .ToArray();
        }
    }
}