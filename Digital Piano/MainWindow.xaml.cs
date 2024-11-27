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
        //Сделать экземляр метронома
        //Сделать обработку, проверку и сохранение изменений в полях

        public MainWindow() {
            piano = new Piano();

            InitializeComponent();
        }

        private bool isNotesNamesVisible = true;
        private bool isMenuVisible = false;
        private bool isExitMenuVisible = false;
        private bool isInstructionsVisible = false;

        private void ToggleMenuButton_Click(object sender, RoutedEventArgs e) {
            Overlay.Visibility = Visibility.Visible;
            Storyboard showMenu = (Storyboard)FindResource("ShowMenuAnimation");
            showMenu.Begin();
            isMenuVisible = true;
        }

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

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (VolumeSlider != null) {
                int newVolume = (int)VolumeSlider.Value;
                piano.volume = newVolume;
            }
        }

        private void SustainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (SustainSlider != null) {
                uint newSustainLevel = (uint)SustainSlider.Value;
                piano.sustain = newSustainLevel;
            }
        }

        private void DecreaseFreqValue(object sender, RoutedEventArgs e) {
            double value = double.Parse(FreqInputField.Text);
            value -= 0.1;
            FreqInputField.Text = value.ToString();
        }

        private void IncreaseFreqValue(object sender, RoutedEventArgs e) {
            double value = int.Parse(FreqInputField.Text);
            value += 0.1;
            FreqInputField.Text = value.ToString();
        }

        private void DecreaseReverbValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(ReverbInputField.Text);
            value -= 1;
            ReverbInputField.Text = value.ToString();
        }

        private void IncreaseReverbValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(ReverbInputField.Text);
            value += 1;
            ReverbInputField.Text = value.ToString();
        }

        private void DecreaseChorusValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(ChorusInputField.Text);
            value -= 1;
            ChorusInputField.Text = value.ToString();
        }

        private void IncreaseChorusValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(ChorusInputField.Text);
            value += 1;
            ChorusInputField.Text = value.ToString();
        }

        private void DecreaseMetroTempoValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(MetroTempoInputField.Text);
            value -= 1;
            ChorusInputField.Text = value.ToString();
        }

        private void IncreaseMetroTempoValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(MetroTempoInputField.Text);
            value += 1;
            ChorusInputField.Text = value.ToString();
        }

        private void DecreaseMetroSignValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(MetroSignInputField.Text);
            value -= 1;
            MetroSignInputField.Text = value.ToString();
        }

        private void IncreaseMetroSignValue(object sender, RoutedEventArgs e) {
            uint value = uint.Parse(MetroSignInputField.Text);
            value += 1;
            MetroSignInputField.Text = value.ToString();
        }

    }
}
