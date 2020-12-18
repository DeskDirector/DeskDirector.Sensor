using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DdManager.Sensor.NetworkInformation.NetTCPIP;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Infrastructure;

namespace DdManager.Sensor.NetworkInformation
{
    public class TcpConnectionExporter
    {
        private readonly ILogger _logger;

        public TcpConnectionExporter(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            IReadOnlyDictionary<uint, Process> processes = Process.GetProcesses().ToDictionary(p => (uint)p.Id);
            DynamicPortRange range = await new DynamicPortDetector(_logger).GetAsync();
            _logger.LogInformation($"TCP Dynamic port range from {range.Start} to {range.End}");

            DirectoryInfo directory = new(Path.Combine(AppContext.BaseDirectory, "export"));
            if (!directory.Exists) {
                directory.Create();
            }
            FileInfo file = new(Path.Combine(directory.FullName, $"{DateTime.Now:yyyy-MM-ddTHH-mm-ss} connections.csv"));
            if (file.Exists) {
                file.Delete();
            }

            await using var writer = new StreamWriter(file.FullName);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            await WriteHeaderAsync(csv);

            foreach (NetTcpConnection connection in GetConnections()) {
                if (connection.LocalPort < range.Start || connection.LocalPort > range.End) {
                    continue;
                }

                if (!processes.TryGetValue(connection.ProcessId, out Process? process)) {
                    continue;
                }

                Report report = new(process, connection);

                await WriteRowAsync(csv, report);
            }
        }

        private async Task WriteHeaderAsync(CsvWriter csv)
        {
            csv.WriteField("Process Name");
            csv.WriteField("Process ID");
            csv.WriteField("Local Address");
            csv.WriteField("Local Port");
            csv.WriteField("Remote Address");
            csv.WriteField("Remote Port");
            csv.WriteField("State");

            await csv.NextRecordAsync();
        }

        private async Task WriteRowAsync(CsvWriter csv, Report report)
        {
            csv.WriteField(report.ProcessName);
            csv.WriteField(report.ProcessId);
            csv.WriteField(report.LocalAddress);
            csv.WriteField(report.LocalPort);
            csv.WriteField(report.RemoteAddress);
            csv.WriteField(report.RemotePort);
            csv.WriteField(report.State);

            await csv.NextRecordAsync();
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

        public class Report
        {
            [Name("Process Name")]
            public string ProcessName { get; }

            [Name("Process ID")]
            public uint ProcessId { get; }

            [Name("Local Address")]
            public string LocalAddress { get; }

            [Name("Local Port")]
            public ushort LocalPort { get; }

            [Name("Remote Address")]
            public string RemoteAddress { get; }

            [Name("Remote Port")]
            public ushort RemotePort { get; }

            [Name("State")]
            public TcpState State { get; }

            public Report(Process process, NetTcpConnection connection)
            {
                ProcessName = process.ProcessName;
                ProcessId = connection.ProcessId;
                LocalAddress = connection.LocalAddress;
                LocalPort = connection.LocalPort;
                RemoteAddress = connection.RemoteAddress;
                RemotePort = connection.RemotePort;
                State = connection.State;
            }
        }
    }
}