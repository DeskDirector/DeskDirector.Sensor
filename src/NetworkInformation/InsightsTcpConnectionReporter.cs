using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;

namespace DdManager.Sensor.NetworkInformation
{
    public class InsightsTcpConnectionReporter : ITcpConnectionReporter
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IOptions<SensorOptions> _options;

        public InsightsTcpConnectionReporter(TelemetryClient telemetryClient, IOptions<SensorOptions> options)
        {
            _telemetryClient = telemetryClient;
            _options = options;
        }

        public void Report(TcpConnectionAnalyzeResult result)
        {
            MetricTelemetry? telemetry = ToTelemetry(result.LocalAddressPortCounts);
            if (telemetry != null) {
                _telemetryClient.Track(telemetry);
            }
        }

        private MetricTelemetry? ToTelemetry(IReadOnlyList<LocalAddressPortCountResult> data)
        {
            if (data.Count == 0) {
                return null;
            }

            LocalAddressPortCountResult first = data.First();
            ProcessCount? offendProcess = first.TopProcessCounts.FirstOrDefault();
            RemoteAddressCount? offendRemote = first.TopRemoteAccessCounts.FirstOrDefault();

            var metric = new MetricTelemetry {
                Name = "sensor_tcp_ephemeral_ports",
                Sum = first.Count,
                Count = first.Count,
                Properties =
                {
                    ["device"] = _options.Value?.Device ?? "unknown"
                }
            };

            if (offendProcess != null) {
                metric.Properties["offend_name"] = offendProcess.Name;
                metric.Properties["offend_id"] = offendProcess.Id.ToString();
                metric.Properties["offend_count"] = offendProcess.Count.ToString();
            }
            if (offendRemote != null) {
                metric.Properties["offend_remote"] = offendRemote.Location;
                metric.Properties["offend_remote_count"] = offendRemote.Count.ToString();
            }

            metric.Properties["local_ip"] = first.Address;
            metric.Properties["allowed_count"] = first.Allowed.ToString();
            metric.Properties["percentage"] = first.Percentage.ToString("0.0");

            return metric;
        }
    }
}