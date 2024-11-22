using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;


namespace Digital_Piano {
    public class Piano {
        protected const int SampleRate = 44100;
        protected double basefreq;
        protected uint sustain;
        protected uint reverb;
        protected uint chorus;
        public int volume;
        protected int pitch;

        public Piano() {
            this.basefreq = 440.0;
            this.sustain = 0;   
            this.reverb = 0;
            this.chorus = 0;
            this.volume = 50;
            this.pitch = 0;
        }

        private double GetNoteFrequency(int semitoneOffset) {
            return basefreq * Math.Pow(2, semitoneOffset / 12.0);
        }

        public void PlayTone(int semitoneOffset, double durationSeconds) {
            int samplesCount = (int)(SampleRate * durationSeconds);
            var buffer = new float[samplesCount];

            // Генерация синусоиды для нужной частоты
            for (int i = 0; i < samplesCount; i++) {
                buffer[i] = (float)Math.Sin(2 * Math.PI * GetNoteFrequency(semitoneOffset) * i / SampleRate);
            }

            // Используем WaveOutEvent для воспроизведения
            using (var waveOut = new WaveOutEvent()) {
                var waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1));
                waveProvider.AddSamples(GetByteArrayFromFloatArray(buffer), 0, buffer.Length * sizeof(float));
                waveOut.Init(waveProvider);
                waveOut.Play();
                Thread.Sleep((int)(durationSeconds * 1000)); // Задержка для завершения воспроизведения
            }
        }
        private byte[] GetByteArrayFromFloatArray(float[] buffer) {
            var byteArray = new byte[buffer.Length * sizeof(float)];
            Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
    }

}
