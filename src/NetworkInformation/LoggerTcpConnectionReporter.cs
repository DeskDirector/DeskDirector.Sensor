using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DdManager.Sensor.NetworkInformation
{
    public class LoggerTcpConnectionReporter : ITcpConnectionReporter
    {
        private readonly ILogger<LoggerTcpConnectionReporter> _logger;

        public LoggerTcpConnectionReporter(ILogger<LoggerTcpConnectionReporter> logger)
        {
            _logger = logger;
        }

        public void Report(TcpConnectionAnalyzeResult result)
        {
            if (result == null) {
                throw new ArgumentNullException(nameof(result));
            }

            ReportLocal(result.LocalAddressPortCounts);
            ReportRemote(result.RemoteAccessCounts);
        }

        private void ReportLocal(IReadOnlyList<LocalAddressPortCountResult> data)
        {
            if (data.Count == 0) {
                _logger.LogInformation("No Local IP Address holding any Ephemeral ports");
                return;
            }

            foreach (LocalAddressPortCountResult local in data) {
                _logger.LogInformation("<Local>".PadRight(30) + local.Address.PadRight(30) + $"={local.Count} / {local.Allowed}");
            }

            LocalAddressPortCountResult first = data.First();
            ProcessCount? offend = first.TopProcessCounts.FirstOrDefault();
            RemoteAddressCount? offendRemote = first.TopRemoteAccessCounts.FirstOrDefault();

            _logger.LogInformation($"# Local {first.Address} used {first.Percentage:0.0}% of allowed ephemeral ports");

            if (offend != null) {
                _logger.LogInformation($"* Top Offend Process: {offend.Name} ({offend.UserName})/{offend.Id}".PadRight(60) + offend.Count);
            }

            if (offendRemote != null) {
                _logger.LogInformation($"* Top Offend Remote Address: {offendRemote.Location}".PadRight(60) + offendRemote.Count);
            }
        }

        private void ReportRemote(IReadOnlyList<RemoteAddressCount> data)
        {
            if (data.Count == 0) {
                _logger.LogInformation("No outbound connections");
                return;
            }

            foreach (RemoteAddressCount remote in data.Take(3)) {
                _logger.LogInformation("<Remote>".PadRight(30) + remote.Location.PadRight(30) + $"={remote.Count}");
            }
        }
    }
}