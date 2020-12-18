using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DdManager.Sensor.NetworkInformation
{
    public class DynamicPortDetector
    {
        private readonly ILogger _logger;

        public DynamicPortDetector(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<DynamicPortRange> GetAsync()
        {
            using Process netsh = new() {
                StartInfo = new ProcessStartInfo {
                    FileName = "netsh",
                    Arguments = "int ipv4 show dynamicportrange tcp",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            string output = await ReadNetshOutputAsync(netsh);
            return ParseNetshOutput(output);
        }

        private static DynamicPortRange ParseNetshOutput(string? output)
        {
            output = output?.Trim();
            if (String.IsNullOrEmpty(output) ||
                !output.StartsWith("Protocol tcp Dynamic Port Range", StringComparison.Ordinal)) {
                return DynamicPortRange.Default;
            }

            string[] lines = output.Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries);
            uint? start = ParsePortRangeFor(lines, "Start Port");
            uint? range = ParsePortRangeFor(lines, "Number of Ports");

            return start != null && range != null
                ? new DynamicPortRange(start.Value, range.Value)
                : DynamicPortRange.Default;
        }

        private static uint? ParsePortRangeFor(string[] lines, string prefix)
        {
            string? line = lines.FirstOrDefault(l => l.TrimStart().StartsWith(prefix, StringComparison.Ordinal));
            if (line == null) {
                return null;
            }
            return ParsePortRangeLine(line);
        }

        private static uint? ParsePortRangeLine(ReadOnlySpan<char> line)
        {
            int indexOfSplit = line.IndexOf(':');
            if (indexOfSplit <= 0) {
                return null;
            }

            return UInt32.TryParse(line.Slice(indexOfSplit + 1).Trim(), out uint result) ? result : null;
        }

        private async Task<string> ReadNetshOutputAsync(Process process)
        {
            try {
                process.Start();
                string result = await process.StandardOutput.ReadToEndAsync();
                return result ?? String.Empty;
            } catch (Exception e) {
                _logger.LogError(e, "Unable to execute netsh command");
                return String.Empty;
            }
        }
    }
}