using System.Threading.Tasks;

namespace DdManager.Sensor.NetworkInformation
{
    public interface ITcpConnectionReporter
    {
        void Report(TcpConnectionAnalyzeResult result);
    }
}