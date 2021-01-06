using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace DdManager.Sensor.NetworkInformation
{
    public static class ProcessExtensions
    {
        public static string? GetProcessUser(this Process process)
        {
            if (process == null) {
                throw new ArgumentNullException(nameof(process));
            }

            IntPtr processHandle = IntPtr.Zero;
            try {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new(processHandle);
                string user = wi.Name;
                if (String.IsNullOrEmpty(user)) {
                    return null;
                }

                int indexOfSlash = user.IndexOf(@"\", StringComparison.InvariantCulture);
                return indexOfSlash > 0 && indexOfSlash < (user.Length - 1) ? user.Substring(indexOfSlash + 1) : user;
            } catch {
                return null;
            } finally {
                if (processHandle != IntPtr.Zero) {
                    CloseHandle(processHandle);
                }
            }
        }

        // ReSharper disable once StringLiteralTypo
        [DllImport("advapi32.dll", SetLastError = true)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}