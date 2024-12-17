using System.Configuration;
using System.Data;
using System.Windows;

namespace Digital_Piano {
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            string savedLicenseKey = System.IO.File.Exists("license.key") ? System.IO.File.ReadAllText("license.key") : null;

            if (savedLicenseKey == null || !Crypto.CombinedProtection.IsLaunchAllowed(savedLicenseKey)) {
                MessageBox.Show("Лицензия не найдена или не действительна.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
