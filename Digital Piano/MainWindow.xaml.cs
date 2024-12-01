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

        private bool isNotesNamesVisible = false;
        private bool isMenuVisible = false;
        private bool isExitMenuVisible = false;
        private bool isInstructionsVisible = false;
        private static readonly int[] time_sig = new int[] { 3, 4, 5, 6, 7 };

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) {
                if (isInstructionsVisible) {
                    HideHelpPanel();
                    return;
                }
                if (isMenuVisible) {
                    HideSideBarMenu();
                    return;
                }
                if (!isExitMenuVisible) {
                    ShowExitMenu();
                    return;
                }
                HideExitMenu();
                return;
            }
            if (e.Key == Key.Enter) {
                if (isExitMenuVisible) {
                    Application.Current.Shutdown();
                }
            }
        }

        private void ToggleMenuButton_Click(object sender, RoutedEventArgs e) {
            Overlay.Visibility = Visibility.Visible;
            Storyboard showMenu = (Storyboard)FindResource("ShowMenuAnimation");
            showMenu.Begin();
            isMenuVisible = true;
        }
        private void HideSideBarMenu() {
            Storyboard hideMenu = (Storyboard)FindResource("HideMenuAnimation");
            hideMenu.Completed += (s, ev) => {
                Overlay.Visibility = Visibility.Collapsed;
            };
            hideMenu.Begin();
            isMenuVisible = false;
        }

        private void ShowExitMenu() {
            Overlay.Visibility = Visibility.Visible;
            Storyboard showMenu = (Storyboard)FindResource("ShowExitMenuAnimation");
            showMenu.Begin();
            isExitMenuVisible = true;
        }
        private void HideExitMenu() {
            Storyboard hideMenu = (Storyboard)FindResource("HideExitMenuAnimation");
            hideMenu.Completed += (s, e) =>
            {
                Overlay.Visibility = Visibility.Collapsed;
            };
            hideMenu.Begin();
            isExitMenuVisible = false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
        private void NoButton_Click(object sender, RoutedEventArgs e) {
            HideExitMenu();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Данные сохранены!");
            Storyboard hideMenu = (Storyboard)FindResource("HideMenuAnimation");
            hideMenu.Completed += (s, ev) => {
                Overlay.Visibility = Visibility.Collapsed;
            };
            hideMenu.Begin();
            isMenuVisible = false;
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e) {
            if (isInstructionsVisible) {
                HideHelpPanel();
            }
            else {
                ShowHelpPanel();
            }
        }
        private void HideHelpPanel() {
            InstructionsPanel.Visibility = Visibility.Hidden;
            Storyboard hideStoryboard = (Storyboard)FindResource("HideInstructionsAnimation");
            hideStoryboard.Begin();
            isInstructionsVisible = !isInstructionsVisible;
        }
        private void ShowHelpPanel() {
            InstructionsPanel.Visibility = Visibility.Visible;
            Storyboard showStoryboard = (Storyboard)FindResource("ShowInstructionsAnimation");
            showStoryboard.Begin();
            isInstructionsVisible = !isInstructionsVisible;
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

        private void LabelShowClick(object sender, RoutedEventArgs e) {
            if (isNotesNamesVisible) {
                HideText();
            }
            else {
                ShowText();
            }
            isNotesNamesVisible = !isNotesNamesVisible;
        }
        private void HideText() {
            foreach (var button in WhiteKeys.Children.OfType<Button>()) {
                var textBlock = button.Content as TextBlock;
                if (textBlock != null) {
                    textBlock.Visibility = Visibility.Collapsed;
                }
            }
            foreach (var button in BlackKeys.Children.OfType<Button>()) {
                var textBlock = button.Content as TextBlock;
                if (textBlock != null) {
                    textBlock.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void ShowText() {
            foreach (var button in WhiteKeys.Children.OfType<Button>()) {
                var textBlock = button.Content as TextBlock;
                if (textBlock != null) {
                    textBlock.Visibility = Visibility.Visible;
                }
            }
            foreach (var button in BlackKeys.Children.OfType<Button>()) {
                var textBlock = button.Content as TextBlock;
                if (textBlock != null) {
                    textBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (VolumeSlider != null) {
                int newVolume = (int)VolumeSlider.Value;
                piano.volume = newVolume;
            }
        }
        private void SustainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (SustainSlider != null) {
                int newSustainLevel = (int)SustainSlider.Value;
                piano.sustain = newSustainLevel;
            }
        }

        private void DecreaseFreqValue(object sender, RoutedEventArgs e) {
            if (double.TryParse(FreqInputField.Text, out double current_freq)) {
                current_freq = Math.Max(420.0, current_freq - 0.1);
                FreqInputField.Text = current_freq.ToString("F1");
            }
        }
        private void IncreaseFreqValue(object sender, RoutedEventArgs e) {
            if (double.TryParse(FreqInputField.Text, out double current_freq)) {
                current_freq = Math.Min(480.0, current_freq + 0.1);
                FreqInputField.Text = current_freq.ToString("F1");
            }
        }

        private void DecreaseReverbValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(ReverbInputField.Text, out int current_value)) {
                if (current_value > 0) {
                    ReverbInputField.Text = (current_value - 1).ToString();
                }
            }
        }
        private void IncreaseReverbValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(ReverbInputField.Text, out int current_value)) {
                if (current_value < 9) {
                    ReverbInputField.Text = (current_value + 1).ToString();
                }
            }
        }

        private void DecreaseChorusValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(ChorusInputField.Text, out int current_value)) {
                if (current_value > 0) {
                    ChorusInputField.Text = (current_value - 1).ToString();
                }
            }
        }
        private void IncreaseChorusValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(ChorusInputField.Text, out int current_value)) {
                if (current_value < 3) {
                    ChorusInputField.Text = (current_value + 1).ToString();
                }
            }
        }

        private void DecreaseMetroTempoValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(MetroTempoInputField.Text, out int current_value)) {
                if (current_value > 30) {
                    MetroTempoInputField.Text = (current_value - 1).ToString();
                }
            }
        }
        private void IncreaseMetroTempoValue(object sender, RoutedEventArgs e) {
            if (int.TryParse(MetroTempoInputField.Text, out int current_value)) {
                if (current_value < 240) {
                    MetroTempoInputField.Text = (current_value + 1).ToString();
                }
            }
        }

        private void DecreaseMetroSignValue(object sender, RoutedEventArgs e) {
            string currentValue = MetroSignInputField.Text;
            string[] parts = currentValue.Split('/');
            if (parts.Length == 2) {
                if (int.TryParse(parts[0], out int firstNumber)) {
                    int index = firstNumber - 3;
                    if (index > 0) {
                        firstNumber = time_sig[index - 1];
                        MetroSignInputField.Text = $"{firstNumber}/{parts[1]}";
                    }
                }
            }
        }
        private void IncreaseMetroSignValue(object sender, RoutedEventArgs e) {
            string currentValue = MetroSignInputField.Text;
            string[] parts = currentValue.Split('/');
            if (parts.Length == 2) {
                if (int.TryParse(parts[0], out int firstNumber)) {
                    int index = firstNumber - 3;
                    if (index < 4) {
                        firstNumber = time_sig[index + 1];
                        MetroSignInputField.Text = $"{firstNumber}/{parts[1]}";
                    }
                }
            }
        }
    }
}
