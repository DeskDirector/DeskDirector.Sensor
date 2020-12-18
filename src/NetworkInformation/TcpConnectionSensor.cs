using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DdManager.Sensor.NetworkInformation
{
    public class TcpConnectionSensor : BackgroundService
    {
        private readonly ILogger<TcpConnectionSensor> _logger;
        private readonly IEnumerable<ITcpConnectionReporter> _reporters;

        public TcpConnectionSensor(ILogger<TcpConnectionSensor> logger, IEnumerable<ITcpConnectionReporter> reporters)
        {
            _logger = logger;
            _reporters = reporters;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) {
                await Analyze(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task Analyze(CancellationToken stoppingToken)
        {
            try {
                TcpConnectionAnalyzeResult result = await new TcpConnectionAnalyzer(_logger)
                    .AnalyzeAsync();

                foreach (ITcpConnectionReporter reporter in _reporters) {
                    if (stoppingToken.IsCancellationRequested) {
                        return;
                    }

                    reporter.Report(result);
                }
            } catch (Exception e) {
                _logger.LogError(e, "Unable to analyze TCP connections");
            }
        }
    }
}