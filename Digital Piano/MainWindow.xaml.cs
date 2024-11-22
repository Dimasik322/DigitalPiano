using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Digital_Piano {
    public partial class MainWindow : Window {

        private Piano piano;

        public MainWindow() {
            piano = new Piano();
            InitializeComponent();
        }

        bool isNotesNamesVisible = true;
        private bool isMenuVisible = false;

        private void ToggleMenuButton_Click(object sender, RoutedEventArgs e) {
            if (isMenuVisible) {
                CloseMenu();
            }
            else {
                OpenMenu();
            }
        }

        private void OpenMenu() {
            // Скрытие кнопки ToggleMenu
            ToggleMenuButton.Visibility = Visibility.Collapsed;

            // Показ меню и Overlay
            Overlay.Visibility = Visibility.Visible;

            // Запуск анимации для открытия меню
            Storyboard showMenu = (Storyboard)FindResource("ShowMenuAnimation");
            showMenu.Begin();

            isMenuVisible = true;
        }

        private void CloseMenu() {
            // Закрытие меню
            Storyboard hideMenu = (Storyboard)FindResource("HideMenuAnimation");
            hideMenu.Completed += (s, ev) =>
            {
                // Скрытие Overlay и кнопки ToggleMenu
                Overlay.Visibility = Visibility.Collapsed;
                ToggleMenuButton.Visibility = Visibility.Visible;
            };
            hideMenu.Begin();

            isMenuVisible = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            // Обработка сохранения данных
            MessageBox.Show("Данные сохранены!");

            // Закрыть меню после сохранения
            CloseMenu();
        }

        private void KeyClick(object sender, RoutedEventArgs e) {
            Button clickedButton = sender as Button;
            if (clickedButton != null) {
                int semitoneOffset = int.Parse(clickedButton.Tag.ToString());
                if (piano != null) {
                    piano.PlayTone(semitoneOffset, 1);
                }
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (VolumeSlider != null) {
                int newVolume = (int)VolumeSlider.Value;
                piano.volume = newVolume;
                if (VolumeLabel != null) {
                    VolumeLabel.Text = $"Volume: {newVolume}%";
                }
            }
        }
    }
}
