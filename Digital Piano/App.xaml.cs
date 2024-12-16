using System.Configuration;
using System.Data;
using System.Windows;

namespace Digital_Piano {
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            string savedLicenseKey = System.IO.File.Exists("license.key") ? System.IO.File.ReadAllText("license.key") : null;

            if (savedLicenseKey == null || !Crypto.HardwareBinding.ValidateLicenseKey(savedLicenseKey)) {
                MessageBox.Show("Лицензия недействительна или отсутствует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            if (!Crypto.TimeLimitedAccess.IsAccessAllowed()) {
                MessageBox.Show("Срок действия приложения истек", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            if (!Crypto.LaunchLimit.IsLaunchAllowed()) {
                MessageBox.Show("Лимит запусков приложения исчерпан", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            Crypto.LaunchLimit.IncrementLaunchCount();
            Crypto.TimeLimitedAccess.DisplayRemainingTime();

            MainWindow mainWindow = new MainWindow();
            MainWindow.Show();
        }
    }

}
