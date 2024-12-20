using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Management;
using System.Net.Sockets;
using System.Net;


namespace Digital_Piano {
    public class Crypto {
        public class CombinedProtection {
            private static readonly string FilePath = "launch_count.dat";
            private static readonly string LicenseFile = "license.key";
            private static readonly string NtpServer = "time.google.com";

            private static string GetDriveSerialNumber() {
                string serialNumber = string.Empty;
                try {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                    foreach (ManagementObject disk in searcher.Get()) {
                        serialNumber = disk["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serialNumber)) {
                            break;
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Ошибка при получении серийного номера диска: {ex.Message}");
                }
                return serialNumber;
            }

            private static string GenerateLicenseKey(DateTime expirationDate, string serialNumber, int maxLaunches) {
                return $"{expirationDate:yyyy-MM-dd}:{serialNumber}:{maxLaunches}";
            }

            private static bool ValidateLicenseKey(string licenseKey, out DateTime expirationDate, out string serialNumber, out int maxLaunches) {
                expirationDate = DateTime.MinValue;
                serialNumber = string.Empty;
                maxLaunches = 0;

                if (string.IsNullOrWhiteSpace(licenseKey)) return false;

                string[] parts = licenseKey.Split(':');
                if (parts.Length != 3 ||
                    !DateTime.TryParse(parts[0], out expirationDate) ||
                    !int.TryParse(parts[2], out maxLaunches)) {
                    return false;
                }

                serialNumber = parts[1];
                return true;
            }

            private static int GetCurrentLaunchCount() {
                if (!File.Exists(FilePath)) {
                    File.WriteAllText(FilePath, "0");
                }
                string countStr = File.ReadAllText(FilePath);
                return int.TryParse(countStr, out int count) ? count : 0;
            }

            private static void IncrementLaunchCount() {
                int currentCount = GetCurrentLaunchCount();
                File.WriteAllText(FilePath, (currentCount + 1).ToString());
            }
                
            private static DateTime GetNetworkTime() {
                const int ntpDataLength = 48;
                byte[] ntpData = new byte[ntpDataLength];
                ntpData[0] = 0x1B;

                try {
                    IPAddress[] addresses = Dns.GetHostEntry(NtpServer).AddressList;
                    IPEndPoint endPoint = new IPEndPoint(addresses[0], 123);
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                        socket.Connect(endPoint);
                        socket.Send(ntpData);
                        socket.Receive(ntpData);
                    }

                    const byte serverReplyTime = 40;
                    ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                    ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);

                    ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                    DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

                    return networkDateTime.ToLocalTime();
                }
                catch (Exception ex) {
                    Console.WriteLine($"Ошибка получения времени с NTP-сервера: {ex.Message}");
                    throw;
                }
            }

            private static uint SwapEndianness(ulong x) {
                return (uint)(((x & 0x000000ff) << 24) +
                              ((x & 0x0000ff00) << 8) +
                              ((x & 0x00ff0000) >> 8) +
                              ((x & 0xff000000) >> 24));
            }

            public static bool IsLaunchAllowed(string licenseKey) {
                if (!ValidateLicenseKey(licenseKey, out DateTime expirationDate, out string serialNumber, out int maxLaunches))
                    return false;

                string actualSerialNumber = GetDriveSerialNumber();
                if (serialNumber != actualSerialNumber)
                    return false;

                int currentCount = GetCurrentLaunchCount();
                if (currentCount >= maxLaunches)
                    return false;

                DateTime currentTime = GetNetworkTime();
                if (currentTime >= expirationDate) {
                    return false;
                }

                IncrementLaunchCount();
                return true;
            }
        }
    }
}
