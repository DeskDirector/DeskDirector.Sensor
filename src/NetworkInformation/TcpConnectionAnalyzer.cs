using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DdManager.Sensor.NetworkInformation.NetTCPIP;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Infrastructure;

namespace DdManager.Sensor.NetworkInformation
{
    public class TcpConnectionAnalyzer
    {
        private readonly ILogger _logger;

        public TcpConnectionAnalyzer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TcpConnectionAnalyzeResult> AnalyzeAsync()
        {
            IReadOnlyDictionary<uint, Process> processes = Process.GetProcesses().ToDictionary(p => (uint)p.Id);
            DynamicPortRange range = await new DynamicPortDetector(_logger).GetAsync();
            _logger.LogInformation($"TCP Dynamic port range from {range.Start} to {range.End}");

            var localMap = new Dictionary<string, LocalAddressPortCount>();
            var remoteMap = new Dictionary<string, RemoteAddressCount>();

            foreach (NetTcpConnection connection in GetConnections()) {
                IncrementLocal(localMap, connection, processes, range);
                IncrementRemote(remoteMap, connection);
            }

            return new TcpConnectionAnalyzeResult(
                localMap,
                remoteMap
            );
        }

        private static IEnumerable<NetTcpConnection> GetConnections()
        {
            using CimSession session = CimSession.Create(null);
            IEnumerable<CimInstance> instances = session
                .QueryInstances(
                    NetTcpConnection.Namespace,
                    "WQL",
                    "Select * From MSFT_NetTCPConnection"
                );

            foreach (CimInstance instance in instances) {
                using (instance) {
                    NetTcpConnection? connection = NetTcpConnection.From(instance);
                    if (connection != null) {
                        yield return connection;
                    }
                }
            }
        }

        private static void IncrementLocal(
            Dictionary<string, LocalAddressPortCount> map,
            NetTcpConnection connection,
            IReadOnlyDictionary<uint, Process> processes,
            DynamicPortRange range)
        {
            if (connection.LocalPort < range.Start || connection.LocalPort > range.End) {
                return;
            }

            if (map.TryGetValue(connection.LocalAddress, out LocalAddressPortCount? count)) {
                count.Increment(connection, processes);
            } else {
                count = new LocalAddressPortCount(connection.LocalAddress, range);
                count.Increment(connection, processes);

                map[count.Address] = count;
            }
        }

        private static readonly IReadOnlySet<ushort> OutboundCommonPort = new HashSet<ushort>
        {
            0,
            80,
            443,
            6443,
            21,
            990,
            25,
            587,
            465
        };

        private static void IncrementRemote(
            Dictionary<string, RemoteAddressCount> map,
            NetTcpConnection connection)
        {
            if (!OutboundCommonPort.Contains(connection.RemotePort)) {
                return;
            }

            map.Increment(connection);
        }
    }
}