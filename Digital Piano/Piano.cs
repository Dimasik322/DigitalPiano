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
        public int volume;
        public int pitch;
        public Metro metro;

        private int lowest_semitone = -33;
        private int highest_semitone = 39;
        private double timeToPercent = Math.Log(100);

        public Dictionary<int, float[]> toneCache = new Dictionary<int, float[]>();

        public class Metro {
            public int volume;
            public int tempo;
            public int current_size;
            public int[] time_sig;

            public Metro() {
                this.volume = 50;
                this.tempo = 60;
                this.current_size = 4;
                this.time_sig = new int[] { 3, 4, 5, 6, 7 };
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
            for (int overtone = 2; overtone <= 5; overtone++) {
                double overtoneFreq = GetNoteFrequency(semitoneOffset) * overtone;
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] += (float)(Math.Sin(2 * Math.PI * overtoneFreq * i / SampleRate) / overtone);
                }
            }
            return buffer;
        }
        private float[] AddReverb(float[] buffer) {
            int delaySamples = (int)(SampleRate * 0.03);
            float decay = 0.5f;
            for (int i = delaySamples; i < buffer.Length; i++) {
                buffer[i] += buffer[i - delaySamples] * decay * (reverb / 9.0f);
            }
            return buffer;
        }
        private float[] AddChorus(float[] buffer) {
            int modDepth = (int)(SampleRate * 0.005);
            double modRate = 1.0;
            for (int i = 0; i < buffer.Length; i++) {
                int modOffset = (int)(modDepth * Math.Sin(2 * Math.PI * modRate * i / SampleRate));
                int index = Math.Clamp(i + modOffset, 0, buffer.Length - 1);
                buffer[i] += buffer[index] * (chorus / 3.0f);
            }
            return buffer;
        }
    }


}
