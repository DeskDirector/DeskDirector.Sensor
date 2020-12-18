using System;
using System.Collections.Generic;
using DdManager.Sensor.NetworkInformation.NetTCPIP;

namespace DdManager.Sensor.NetworkInformation
{
    public static class AnalyzerExtensions
    {
        public static void Increment(
            this Dictionary<string, RemoteAddressCount> map,
            NetTcpConnection connection)
        {
            if (map == null) {
                throw new ArgumentNullException(nameof(map));
            }
            if (connection == null) {
                throw new ArgumentNullException(nameof(connection));
            }

            string key = RemoteAddressCount.ConstructKey(connection);

            if (map.TryGetValue(key, out RemoteAddressCount? count)) {
                count.Increment(connection);
            } else {
                count = new RemoteAddressCount(connection);
                count.Increment(connection);

                map[count.Location] = count;
            }
        }
    }
}