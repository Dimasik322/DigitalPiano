﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;


namespace Digital_Piano {
    public class Piano {
        protected const int SampleRate = 44100;
        public double basefreq;
        public int sustain;
        public int reverb;
        public int chorus;
        public int volume;
        public int pitch;
        public Metro metro;

        int lowest_semitone = -9;
        int highest_semitone = 15;

        public class Metro {
            public int volume;
            public int current_size;
            public int[] time_sig;

            public Metro() {
                this.volume = 50;
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
            this.metro = new Metro();
        }

        private double GetNoteFrequency(int semitoneOffset) {
            return basefreq * Math.Pow(2, semitoneOffset / 12.0);
        }

        public void PlayTone(int semitoneOffset, double durationSeconds) {
            int samplesCount = (int)(SampleRate * durationSeconds);
            var buffer = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++) {
                buffer[i] = (float)Math.Sin(2 * Math.PI * GetNoteFrequency(semitoneOffset) * i / SampleRate);
            }
            buffer = AddOvertones(buffer, semitoneOffset);
            if (reverb > 0) {
                buffer = ApplyReverb(buffer, reverb);
            }
            if (chorus > 0) {
                buffer = ApplyChorus(buffer, chorus);
            }
            using (var waveOut = new WaveOutEvent()) {
                var waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1));
                waveProvider.AddSamples(GetByteArrayFromFloatArray(buffer), 0, buffer.Length * sizeof(float));
                waveOut.Init(waveProvider);
                waveOut.Play();
                Thread.Sleep((int)(durationSeconds * 1000));
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
        private float[] ApplyReverb(float[] buffer, int reverbStrength) {
            int delaySamples = (int)(SampleRate * 0.03);
            float decay = 0.5f;
            for (int i = delaySamples; i < buffer.Length; i++) {
                buffer[i] += buffer[i - delaySamples] * decay * (reverbStrength / 100.0f);
            }
            return buffer;
        }
        private float[] ApplyChorus(float[] buffer, int chorusStrength) {
            int modDepth = (int)(SampleRate * 0.005);
            double modRate = 1.0;
            for (int i = 0; i < buffer.Length; i++) {
                int modOffset = (int)(modDepth * Math.Sin(2 * Math.PI * modRate * i / SampleRate));
                int index = Math.Clamp(i + modOffset, 0, buffer.Length - 1);
                buffer[i] += buffer[index] * (chorusStrength / 100.0f);
            }
            return buffer;
        }

    }

}
