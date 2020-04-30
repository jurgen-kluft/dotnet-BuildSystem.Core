using System;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace Core
{
    /// <summary>
    /// A unique ID
    /// Uses MAC-Address, Year, Month, Day, Minute, Second, MilliSecond, Process ID and a Counter
    /// </summary>
    public static class GUIDGenerator
    {
        private static GUID sLast = new GUID(0, 0);

        private static UInt64 sMacAddress = 0;
        private static UInt64 sLastLow = 0;
        private static UInt64 sLastCnt = 0;

        public static void Init()
        {
            // Cache the mac-address, requesting it on every generate() call is very slow.
            sMacAddress = 0;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    byte[] macaddress = new byte[8];
                    nic.GetPhysicalAddress().GetAddressBytes().CopyTo(macaddress, 2);
                    sMacAddress = BitConverter.ToUInt64(macaddress, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// Generate a XID (server-side only!)
        /// Uses MAC-Address, Year, Month, Day, Minute, Second, MilliSecond, Process ID and a Counter
        /// </summary>
        /// <returns>A unique ID</returns>
        public static GUID Generate()
        {
            DateTime now = DateTime.Now;

            // MAC-Address

            // Shift in Year and Month from current date-time
            UInt64 high = sMacAddress;
            high = (UInt64)(high | (UInt64)(Int64)((now.Year << 4) & 0xFFF0)) | (UInt64)(Int64)(now.Month & 0x0F);

            // Day, Hour, Minute and MilliSeconds
            UInt64 low = 0;
            low = (UInt64)(low | (UInt64)(Int64)((Int64)now.Day << (64 - 5)) | (UInt64)(Int64)((Int64)now.Hour << (64 - 5 - 6)));
            low = (UInt64)(low | (UInt64)(Int64)((Int64)now.Minute << (64 - 5 - 6 - 6)) | (UInt64)(Int64)((Int64)now.Second << (64 - 5 - 6 - 6 - 6)));
            low = (UInt64)(low | (UInt64)(Int64)((Int64)now.Millisecond << (64 - 5 - 6 - 6 - 6 - 10)) | (UInt64)(Int64)((Int64)System.Diagnostics.Process.GetCurrentProcess().Id & 0xFFFF << (64 - 5 - 6 - 6 - 6 - 10 - 16)));

            if (low == sLastLow)
            {
                low = (UInt64)(low | (UInt64)sLastCnt++);
            }
            else
            {
                sLastCnt = 1;
                sLastLow = low;
            }

            sLast = new GUID(high, low);
            return sLast;
        }
    }

}
