using System;
using System.Collections.Generic;
using System.Diagnostics;
using DdManager.Sensor.NetworkInformation.NetTCPIP;

namespace DdManager.Sensor.NetworkInformation
{
    public class LocalAddressPortCount
    {
        public string Address { get; }

        public int Count { get; private set; }

        public uint Allowed { get; }

        private readonly Dictionary<uint, ProcessCount> _processCountMap;
        public IReadOnlyDictionary<uint, ProcessCount> ProcessCountMap => _processCountMap;

        private readonly Dictionary<string, RemoteAddressCount> _remoteAccessCounts;
        public IReadOnlyDictionary<string, RemoteAddressCount> RemoteAccessCounts => _remoteAccessCounts;

        public LocalAddressPortCount(string address, DynamicPortRange range)
        {
            if (range == null) {
                throw new ArgumentNullException(nameof(range));
            }

            Allowed = range.Count;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Count = 0;
            _processCountMap = new Dictionary<uint, ProcessCount>();
            _remoteAccessCounts = new Dictionary<string, RemoteAddressCount>();
        }

        public void Increment(NetTcpConnection connection, IReadOnlyDictionary<uint, Process> processes)
        {
            if (connection == null) {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.LocalAddress != Address) {
                throw new ArgumentException("IP address not equal, unable to increment");
            }

            Count++;
            if (_processCountMap.TryGetValue(connection.ProcessId, out ProcessCount? processCount)) {
                processCount.Increment();
            } else {
                processCount = new ProcessCount(connection, processes);
                processCount.Increment();

                _processCountMap[connection.ProcessId] = processCount;
            }

            _remoteAccessCounts.Increment(connection);
        }

        public LocalAddressPortCountResult Seal()
        {
            return new(this);
        }
    }

    public class ProcessCount
    {
        public string Name { get; }

        public uint Id { get; }

        public int Count { get; private set; }

        public ProcessCount(NetTcpConnection connection, IReadOnlyDictionary<uint, Process> processes)
        {
            Id = connection.ProcessId;
            Count = 1;
            Name = processes.TryGetValue(connection.ProcessId, out Process? p) ? p.ProcessName : "unknown";
        }

        public void Increment()
        {
            Count++;
        }
    }
}