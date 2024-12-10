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

        private bool gameSelected = false;
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

        private async void GuessNoteGameClick(object sender, RoutedEventArgs e) {
            if (gameSelected) {
                cancellationTokenSource?.Cancel();
            }
            else {
                cancellationTokenSource = new CancellationTokenSource();
                await StartGame(cancellationTokenSource.Token);
            }
            gameSelected = !gameSelected;
        }

        private async Task<bool> WaitForUserInput(int expectedNote, CancellationToken cancellationToken) {
            var taskC = new TaskCompletionSource<bool>();
            RoutedEventHandler handler = null;
            handler = (s, e) => {
                Button clickedButton = s as Button;
                if (clickedButton != null) {
                    int semitoneOffset = int.Parse(clickedButton.Tag.ToString());
                    if (expectedNote == semitoneOffset + (piano.pitch * 12)) {
                        taskC.SetResult(true);
                    }
                    else {
                        taskC.SetResult(false);
                    }
                    foreach (var button in FindVisualChildren<Button>(this)) {
                        button.Click -= handler;
                    }
                }
            };
            foreach (var button in FindVisualChildren<Button>(this)) {
                button.Click += handler;
            }
            using (cancellationToken.Register(() => taskC.TrySetCanceled())) {
                return await taskC.Task;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) {
                        yield return (T)child;
                    }
                    foreach (var childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }

        private async Task StartGame(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                int randomNote = GetRandomNote();
                await piano.PlayCachedTone(randomNote);
                bool isCorrect = await WaitForUserInput(randomNote, cancellationToken);
                if (isCorrect) {
                    MessageBox.Show("Вы выиграли!");
                }
                else {
                    MessageBox.Show("Вы проиграли!");
                }
                await Task.Delay(5000);
            }
        }

        private Random random = new Random();
        private CancellationTokenSource cancellationTokenSource;

        private int GetRandomNote() {
            return random.Next((piano.pitch * 12) - 9, (piano.pitch * 12) + 15);
        }

    }
}
