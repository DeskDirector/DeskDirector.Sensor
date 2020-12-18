using System;
using Microsoft.Management.Infrastructure;

namespace DdManager.Sensor.NetworkInformation.NetTCPIP
{
    public static class CimExtensions
    {
        public static string? GetPropertyAsString(this CimInstance instance, string propertyName)
        {
            object? value = instance.GetPropertyValue(propertyName);
            if (value == null) {
                return null;
            }

            switch (value) {
                case string str:
                    return str;

                default:
                    return value.ToString();
            }
        }

        public static UInt32? GetPropertyAsUInt32(this CimInstance instance, string propertyName)
        {
            object? value = instance.GetPropertyValue(propertyName);
            if (value == null) {
                return null;
            }

            switch (value) {
                case UInt32 uint32:
                    return uint32;

                case UInt16 uint16:
                    return uint16;

                case string str:
                    return UInt32.TryParse(str, out UInt32 uint32Value) ? uint32Value : null;

                default:
                    return null;
            }
        }

        public static UInt16? GetPropertyAsUInt16(this CimInstance instance, string propertyName)
        {
            object? value = instance.GetPropertyValue(propertyName);
            if (value == null) {
                return null;
            }

            switch (value) {
                case UInt16 uint16:
                    return uint16;

                case string str:
                    return UInt16.TryParse(str, out UInt16 uint16Value) ? uint16Value : null;

                default:
                    return null;
            }
        }

        public static byte? GetPropertyAsByte(this CimInstance instance, string propertyName)
        {
            object? value = instance.GetPropertyValue(propertyName);
            if (value == null) {
                return null;
            }

            switch (value) {
                case byte b:
                    return b;

                case string str:
                    return Byte.TryParse(str, out byte strByte) ? strByte : null;

                default:
                    return null;
            }
        }

        private static object? GetPropertyValue(this CimInstance instance, string propertyName)
        {
            if (instance == null) {
                throw new ArgumentNullException(nameof(instance));
            }

            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            CimProperty? property = instance.CimInstanceProperties[propertyName];
            return property?.Value;
        }

        public static TcpState ConvertState(this byte state)
        {
            switch (state) {
                case <= 12:
                    return (TcpState)state;

                case 100:
                    return TcpState.Bound;

                default:
                    return TcpState.Unknown;
            }
        }
    }
}