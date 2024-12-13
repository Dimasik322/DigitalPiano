using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;


namespace Digital_Piano {
    public class Piano {
        private const int SampleRate = 44100;
        public double basefreq;
        public int sustain;
        public int reverb;
        public int chorus;
        internal int volume;
        public int pitch;
        public Metro metro;

        private int lowest_semitone = -33;
        private int highest_semitone = 39;
        private double timeToPercent = Math.Log(100);

        public Dictionary<int, float[]> toneCache = new Dictionary<int, float[]>();

        public class Metro {
            public int tempo;
            public int currentSig;
            public int[] timeSig;

            public bool isMetroStarted = false;
            private Dictionary<int, float[]> beatCache = new Dictionary<int, float[]>();
            private CancellationTokenSource cancellationTokenSource;

            public Metro() {
                this.tempo = 60;
                this.currentSig = 4;
                this.timeSig = new int[] { 3, 4, 5, 6, 7 };

                InitializeBeats();    
            }

            private const int SampleRate = 44100;
            private const double StrongBeatFrequency = 1000.0;
            private const double WeakBeatFrequency = 800.0;

            public void InitializeBeats() {
                beatCache[0] = GenerateClick(StrongBeatFrequency, 0.3);
                beatCache[1] = GenerateClick(WeakBeatFrequency, 0.3);
            }

            private float[] GenerateClick(double frequency, double durationSeconds) {
                int samplesCount = (int)(SampleRate * durationSeconds);
                var buffer = new float[samplesCount];
                double attackDuration = 0.1;
                double decayDuration = 0.1;
                int attackSamples = (int)(SampleRate * attackDuration);
                int decaySamples = (int)(SampleRate * decayDuration);

                for (int i = 0; i < samplesCount; i++) {
                    double t = i / (double)SampleRate;
                    double amplitude = 1.0;
                    if (i < attackSamples) {
                        amplitude = (double)i / attackSamples;
                    }
                    else if (i < attackSamples + decaySamples) {
                        amplitude = 1.0 - (double)(i - attackSamples) / decaySamples;
                    }
                    else {
                        amplitude = 0.0;
                    }
                    buffer[i] = (float)(Math.Sin(2 * Math.PI * frequency * t) * Math.Exp(-5 * t) * amplitude);
                }
                return buffer;
            }

            private async Task PlayClick(float[] buffer) {
                using (var waveOut = new WaveOutEvent()) {
                    var waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1));
                    waveProvider.AddSamples(GetByteArrayFromFloatArray(buffer), 0, buffer.Length * sizeof(float));
                    waveOut.Init(waveProvider);
                    waveOut.Play();
                    await Task.Delay(buffer.Length * 1000 / SampleRate);
                }
            }

            private byte[] GetByteArrayFromFloatArray(float[] buffer) {
                var byteArray = new byte[buffer.Length * sizeof(float)];
                Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);
                return byteArray;
            }

            public async Task StartMetronome() {
                if (tempo <= 0 || currentSig <= 0)
                    throw new ArgumentException("Invalid tempo or time signature.");

                if (isMetroStarted)
                    return;

                isMetroStarted = true;
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                double beatInterval = 60.0 / tempo - 0.3;

                try {
                    while (!token.IsCancellationRequested) {
                        for (int i = 0; i < currentSig; i++) {
                            if (token.IsCancellationRequested)
                                break;

                            float[] click = (i == 0) ? beatCache[0] : beatCache[1];
                            await PlayClick(click);
                            await Task.Delay((int)(beatInterval * 1000), token);
                        }
                    }
                }
                catch (TaskCanceledException) {
                    // Handle cancellation
                }
                finally {
                    isMetroStarted = false;
                }
            }

            public void StopMetronome() {
                if (!isMetroStarted)
                    return;

                cancellationTokenSource?.Cancel();
                isMetroStarted = false;
            }
        }

        public Piano() {
            this.basefreq = 440.0;
            this.sustain = 0;
            this.reverb = 1;
            this.chorus = 0;
            this.volume = 50;
            this.pitch = 0;

            InitializeTones();

            this.metro = new Metro();
        }

        private void GenerateTone(int semitoneOffset, double durationSeconds) {
            double adjustedDuration = durationSeconds * timeToPercent;
            int samplesCount = (int)(SampleRate * adjustedDuration);
            var buffer = new float[samplesCount];
            double frequency = GetNoteFrequency(semitoneOffset);
            for (int i = 0; i < samplesCount; i++) {
                double t = i / (double)SampleRate;
                buffer[i] = (float)Math.Sin(2 * Math.PI * frequency * t);
            }

            buffer = AddOvertones(buffer, semitoneOffset);
            buffer = AddReverb(buffer);
            buffer = AddChorus(buffer);

            toneCache[semitoneOffset] = buffer;
        }


        public void InitializeTones() {
            for (int i = lowest_semitone; i <= highest_semitone; i++) {
                GenerateTone(i, 1.0);
            }
        }

        private double GetNoteFrequency(int semitoneOffset) {
            return basefreq * Math.Pow(2, semitoneOffset / 12.0);
        }

        public async Task PlayCachedTone(int semitoneOffset) {
            if (toneCache.TryGetValue((semitoneOffset + pitch * 12), out var buffer)) {
                float volumeFactor = volume / 2000f;
                double lambda = sustain == 0 ? 8.0 : (sustain == 1 ? 3.0 : 1.0);
                double durationSeconds = buffer.Length / (double)SampleRate;
                var adjustedBuffer = new float[buffer.Length];
                for (int i = 0; i < buffer.Length; i++) {
                    double t = i / (double)SampleRate;
                    double amplitude = Math.Exp(-lambda * t);
                    adjustedBuffer[i] = buffer[i] * volumeFactor * (float)amplitude;
                }
                using (var waveOut = new WaveOutEvent()) {
                    var waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1));
                    waveProvider.AddSamples(GetByteArrayFromFloatArray(adjustedBuffer), 0, adjustedBuffer.Length * sizeof(float));
                    waveOut.Init(waveProvider);
                    waveOut.Play();
                    await Task.Delay((int)(durationSeconds * 1000 / lambda));
                }
            }
            else {
                Console.WriteLine("Тон не найден в кэше.");
            }
        }

        private byte[] GetByteArrayFromFloatArray(float[] buffer) {
            var byteArray = new byte[buffer.Length * sizeof(float)];
            Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        private float[] AddOvertones(float[] buffer, int semitoneOffset) {
            const int maxOvertone = 6;
            for (int overtone = 2; overtone <= maxOvertone; overtone++) {
                double overtoneFreq = GetNoteFrequency(semitoneOffset) * overtone;
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] += (float)(Math.Sin(2 * Math.PI * overtoneFreq * i / SampleRate) / overtone);
                }
            }
            return buffer;
        }
        private float[] AddReverb(float[] buffer) {
            if (reverb == 0) return buffer;
            int maxDelay = (int)(SampleRate * 0.1);
            int numDelays = 5;
            float decayFactor = 0.6f;
            float reverbFactor = reverb / 10.0f;
            float[] result = new float[buffer.Length];
            Array.Copy(buffer, result, buffer.Length);
            for (int delayIndex = 1; delayIndex <= numDelays; delayIndex++) {
                int delaySamples = delayIndex * maxDelay / numDelays;
                float decay = (float)Math.Pow(decayFactor, delayIndex) * reverbFactor;
                for (int i = delaySamples; i < buffer.Length; i++) {
                    result[i] += buffer[i - delaySamples] * decay;
                }
            }
            return result;
        }
        private float[] AddChorus(float[] buffer) {
            if (chorus == 0) return buffer;
            int modDepth = (int)(SampleRate * 0.002);
            double[] modRates = { 0.25, 0.33, 0.4 }; 
            float chorusFactor = chorus / 3.0f; 
            float[] result = new float[buffer.Length];
            Array.Copy(buffer, result, buffer.Length);
            foreach (double modRate in modRates) {
                for (int i = 0; i < buffer.Length; i++) {
                    int modOffset = (int)(modDepth * Math.Sin(2 * Math.PI * modRate * i / SampleRate));
                    int index = Math.Clamp(i + modOffset, 0, buffer.Length - 1);
                    result[i] += buffer[index] * chorusFactor / modRates.Length;
                }
            }
            return result;
        }

    }


}
