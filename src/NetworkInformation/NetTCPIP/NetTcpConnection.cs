using System;
using Microsoft.Management.Infrastructure;

namespace DdManager.Sensor.NetworkInformation.NetTCPIP
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/previous-versions/windows/desktop/nettcpipprov/msft-nettcpconnection">MSFT_NetTCPConnection</see>
    /// </summary>
    public record NetTcpConnection
    {
        public const string Namespace = @"Root\StandardCimv2";

        public uint ProcessId { get; init; }

        public string LocalAddress { get; }

        public ushort LocalPort { get; init; }

        public string RemoteAddress { get; }

        public ushort RemotePort { get; init; }

        public TcpState State { get; init; }

        public NetTcpConnection(string localAddress, string remoteAddress)
        {
            LocalAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
            RemoteAddress = remoteAddress ?? throw new ArgumentNullException(nameof(remoteAddress));
        }

        public static NetTcpConnection? From(CimInstance instance)
        {
            uint? pid = instance.GetPropertyAsUInt32("OwningProcess");
            string? localAddress = instance.GetPropertyAsString("LocalAddress");
            ushort? localPort = instance.GetPropertyAsUInt16("LocalPort");
            string? remoteAddress = instance.GetPropertyAsString("RemoteAddress");
            ushort? remotePort = instance.GetPropertyAsUInt16("RemotePort");
            byte? state = instance.GetPropertyAsByte("State");

            return pid == null || localAddress == null || localPort == null || remoteAddress == null ||
                   remotePort == null || state == null
                ? null
                : new NetTcpConnection(localAddress, remoteAddress) {
                    ProcessId = pid.Value,
                    LocalPort = localPort.Value,
                    RemotePort = remotePort.Value,
                    State = state.Value.ConvertState()
                };
        }
    }
}