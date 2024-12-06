using System;
using System.Globalization;
using System.IO;
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
using Newtonsoft.Json;


namespace Digital_Piano {
    public partial class MainWindow : Window {

        private Piano piano;

        private List<Task> tasks;

        public MainWindow() {
            piano = new Piano();
            tasks = new List<Task>();

            InitializeComponent();
            InitializeKeyButtonMap();
        }

        private bool isNotesNamesVisible = false;
        private bool isMenuVisible = false;
        private bool isExitMenuVisible = false;
        private bool isInstructionsVisible = false;
        private static readonly int[] time_sig = new int[] { 3, 4, 5, 6, 7 };
        private Dictionary<Key, Button> keyButtonMap;
        public class KeyButtonMapping {
            public List<Mapping> Mappings { get; set; }
        }
        public class Mapping {
            public string Key { get; set; }
            public string ButtonName { get; set; }
        }


        private void InitializeKeyButtonMap() {
            string jsonFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeyMap.json");
            if (File.Exists(jsonFilePath)) {
                var jsonData = File.ReadAllText(jsonFilePath);
                var mappings = JsonConvert.DeserializeObject<KeyButtonMapping>(jsonData);

                keyButtonMap = new Dictionary<Key, Button>();
                foreach (var mapping in mappings.Mappings) {
                    Key key = (Key)Enum.Parse(typeof(Key), mapping.Key);
                    Button button = FindName(mapping.ButtonName) as Button;
                    if (button != null) {
                        keyButtonMap[key] = button;
                    }
                }
            }
            else {
                MessageBox.Show($"Путь к JSON файлу: {jsonFilePath}");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (keyButtonMap.TryGetValue(e.Key, out Button button)) {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
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
            if (e.Key == Key.Enter) {
                if (isExitMenuVisible) {
                    Application.Current.Shutdown();
                }
            }
            if (keyButtonMap.ContainsKey(e.Key)) {
                Button button = keyButtonMap[e.Key];
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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

            if (double.TryParse(FreqInputField.Text, out double currentFreq)) {
                piano.basefreq = currentFreq;
            }
            if (int.TryParse(ReverbInputField.Text, out int currentReverb)) {
                piano.reverb = currentReverb;
            }
            if (int.TryParse(ChorusInputField.Text, out int currentChorus)) {
                piano.chorus = currentChorus;
            }
            if (int.TryParse(MetroTempoInputField.Text, out int currentTempo)) {
                piano.metro.tempo = currentTempo;
            }
            string currentSig = MetroSignInputField.Text;
            string[] parts = currentSig.Split('/');
            if (parts.Length == 2) {
                if (int.TryParse(parts[0], out int firstNumber)) {
                    int index = firstNumber - 3;
                    firstNumber = time_sig[index - 1];
                    piano.metro.current_size = firstNumber;
                }
            }

            piano.InitializeTones();
            //piano.metro.InitializeMetro();

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
                    Task task = piano.PlayCachedTone(semitoneOffset);
                    tasks.Add(task);
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
        private void PitchSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (PitchSlider != null) {
                int newPitch = (int)PitchSlider.Value;
                piano.pitch = newPitch;
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
                if (current_value < 10) {
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
                if (current_value < 4) {
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

        //private void ReadMelodyFile(string path) {
        //    double newFreq;
        //    int newReverb;
        //    int newChorus;
        //    int newTempo;
        //    int newTimeSig;

        //    using (var reader = new StreamReader(path)) {
        //        string configLine = reader.ReadLine();
        //        if (configLine != null) {
        //            var parts = configLine.Split('|');
        //            newFreq = double.Parse(parts[0]);
        //            newReverb = int.Parse(parts[1]);
        //            newChorus = int.Parse(parts[2]);
        //            newTempo = int.Parse(parts[3]);
        //            newTimeSig = int.Parse(parts[4]);
        //        }
        //        var chordsList = new List<(double startTime, List<(int semitone, double time)>)>();

        //        while (!reader.EndOfStream) {
        //            string line = reader.ReadLine();
        //            if (string.IsNullOrWhiteSpace(line)) continue;
        //            var chordStartTime = double(line.Split(':')[0]);
        //            var chordNotes = line.Split('|');

        //            melodyConfig.StartTime = double.Parse(startTimeParts[0]);

        //            // Обработка нот
        //            for (int i = 1; i < noteParts.Length; i++) {
        //                if (string.IsNullOrWhiteSpace(noteParts[i])) continue;

        //                var noteInfo = noteParts[i].Split(':');
        //                int semitoneOffset = int.Parse(noteInfo[0]);
        //                double time = double.Parse(noteInfo[1]);
        //                notesList.Add((time, semitoneOffset));
        //            }
        //        }

        //        melodyConfig.Notes = notesList.ToArray();
        //    }

        //    return melodyConfig;
        //}
    }
}
