using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Management;


namespace Digital_Piano {
    public class Crypto {

        public class HardwareBinding {
            public static string GetDriveSerialNumber() {
                string serialNumber = string.Empty;
                try {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                    foreach (ManagementObject disk in searcher.Get()) {
                        serialNumber = disk["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serialNumber)) {
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(serialNumber)) {
                        throw new Exception("Не удалось получить серийный номер диска.");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Ошибка при получении серийного номера диска: {ex.Message}");
                }
                return serialNumber;
            }

            public static string GenerateLicenseKey() {
                string driveSerialNumber = GetDriveSerialNumber();
                using (SHA256 sha256 = SHA256.Create()) {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(driveSerialNumber));
                    return Convert.ToBase64String(hashBytes);
                }
            }

            public static bool ValidateLicenseKey(string providedKey) {
                string generatedKey = GenerateLicenseKey();
                return generatedKey == providedKey;
            }
        }


        public class TimeLimitedAccess {
            private static DateTime StartDate = new DateTime(2024, 12, 1);
            private static int DaysAllowed = 30;

            public static bool IsAccessAllowed() {
                DateTime currentDate = DateTime.Now;
                return currentDate <= StartDate.AddDays(DaysAllowed);
            }

            public static void DisplayRemainingTime() {
                DateTime endDate = StartDate.AddDays(DaysAllowed);
                TimeSpan remainingTime = endDate - DateTime.Now;
                if (remainingTime.TotalSeconds > 0) {
                    Console.WriteLine($"Осталось дней: {remainingTime.Days}");
                }
                else {
                    Console.WriteLine("Срок использования истек.");
                }
            }
        }

        public class LaunchLimit {
            private static string FilePath = "launch_count.dat";
            private static int MaxLaunches = 100;

            public static int GetCurrentLaunchCount() {
                if (!File.Exists(FilePath)) {
                    File.WriteAllText(FilePath, "0");
                }

                string countStr = File.ReadAllText(FilePath);
                return int.TryParse(countStr, out int count) ? count : 0;
            }

            public static void IncrementLaunchCount() {
                int currentCount = GetCurrentLaunchCount();
                File.WriteAllText(FilePath, (currentCount + 1).ToString());
            }

            public static bool IsLaunchAllowed() {
                int currentCount = GetCurrentLaunchCount();
                return currentCount < MaxLaunches;
            }
        }
    }
}
