using System;
using DdManager.Sensor.NetworkInformation.NetTCPIP;

namespace DdManager.Sensor.NetworkInformation
{
    public class RemoteAddressCount
    {
        public string Location { get; }

        public int Count { get; private set; }

        public static string ConstructKey(NetTcpConnection connection) => $"{connection.RemoteAddress}:{connection.RemotePort}";

        public RemoteAddressCount(NetTcpConnection connection)
        {
            if (connection == null) {
                throw new ArgumentNullException(nameof(connection));
            }

            Location = ConstructKey(connection);
            Count = 1;
        }

        public void Increment(NetTcpConnection connection)
        {
            if (connection == null) {
                throw new ArgumentNullException(nameof(connection));
            }

            if (ConstructKey(connection) != Location) {
                throw new ArgumentException("Remote location not equal, unable to increment");
            }

            Count++;
        }
    }
}