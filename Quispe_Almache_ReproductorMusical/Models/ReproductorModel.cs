using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Quispe_Almache_ReproductorMusical.Models
{
    public class ReproductorModel
    {
        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        private string currentFile;
        private bool isPlaying;
        private bool isPaused;
        private float volume = 1.0f;
        private float[] audioData;
        private float[] frequencyData;
        private int fftSize = 512;
        private Random random;
        private string alias = "myAudio";
        private DateTime playbackStartTime;
        private int frameCount;
        private float[] bassBand;
        private float[] midBand;
        private float[] trebleBand;
        private float beatIntensity;
        private float previousBeatIntensity;

        public event EventHandler<AudioDataEventArgs> AudioDataUpdated;

        public ReproductorModel()
        {
            isPlaying = false;
            isPaused = false;
            audioData = new float[fftSize];
            frequencyData = new float[fftSize / 2];
            bassBand = new float[32];
            midBand = new float[64];
            trebleBand = new float[128];
            random = new Random();
            frameCount = 0;
            beatIntensity = 0;
            previousBeatIntensity = 0;
        }

        public void LoadFile(string filePath)
        {
            try
            {
                // Close any existing audio
                mciSendString($"close {alias}", null, 0, IntPtr.Zero);

                // Open the new file
                string command = $"open \"{filePath}\" type mpegvideo alias {alias}";
                int result = mciSendString(command, null, 0, IntPtr.Zero);

                if (result == 0)
                {
                    currentFile = filePath;
                    isPlaying = false;
                    isPaused = false;
                }
                else
                {
                    MessageBox.Show($"Error loading file: Could not open audio file. Error code: {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}");
            }
        }

        public void Play()
        {
            if (currentFile != null && !isPlaying)
            {
                try
                {
                    int result = mciSendString($"play {alias}", null, 0, IntPtr.Zero);
                    if (result == 0)
                    {
                        isPlaying = true;
                        isPaused = false;
                        playbackStartTime = DateTime.Now;
                        frameCount = 0;
                    }
                    else
                    {
                        MessageBox.Show($"Error playing: Error code: {result}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error playing: {ex.Message}");
                }
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                try
                {
                    int result = mciSendString($"pause {alias}", null, 0, IntPtr.Zero);
                    if (result == 0)
                    {
                        isPaused = true;
                        isPlaying = false;
                    }
                    else
                    {
                        MessageBox.Show($"Error pausing: Error code: {result}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error pausing: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            try
            {
                int result = mciSendString($"stop {alias}", null, 0, IntPtr.Zero);
                if (result == 0)
                {
                    mciSendString($"seek {alias} to start", null, 0, IntPtr.Zero);
                    isPlaying = false;
                    isPaused = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping: {ex.Message}");
            }
        }

        public void SetVolume(float vol)
        {
            volume = Math.Max(0, Math.Min(1, vol));
            uint newVolume = (uint)(volume * 0xFFFF);
            waveOutSetVolume(IntPtr.Zero, newVolume | (newVolume << 16));
            
            // Also set MCI volume
            int mciVolume = (int)(volume * 1000);
            mciSendString($"setaudio {alias} volume to {mciVolume}", null, 0, IntPtr.Zero);
        }

        public float GetVolume()
        {
            return volume;
        }

        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        public string CurrentFile => currentFile;

        // Simulate audio data analysis with improved synchronization
        public void UpdateAnalysis()
        {
            if (isPlaying)
            {
                frameCount++;
                double timeSinceStart = (DateTime.Now - playbackStartTime).TotalSeconds;
                
                // Simulate realistic audio patterns based on time
                double beatPattern = Math.Sin(timeSinceStart * 8) * 0.5 + 0.5; // ~120 BPM
                double bassPattern = Math.Sin(timeSinceStart * 4) * 0.7 + 0.3;
                double midPattern = Math.Sin(timeSinceStart * 12) * 0.4 + 0.6;
                double treblePattern = Math.Sin(timeSinceStart * 16) * 0.3 + 0.7;
                
                // Detect beat (sudden increase in energy)
                float currentBeatEnergy = (float)bassPattern * volume;
                beatIntensity = currentBeatEnergy - previousBeatIntensity;
                previousBeatIntensity = currentBeatEnergy;
                
                // Generate audio data with frequency band separation
                for (int i = 0; i < fftSize; i++)
                {
                    float sample = 0;
                    
                    // Bass frequencies (low indices)
                    if (i < fftSize / 8)
                    {
                        sample = (float)(bassPattern * volume * (1 + random.NextDouble() * 0.3));
                        bassBand[i % bassBand.Length] = sample;
                    }
                    // Mid frequencies
                    else if (i < fftSize / 2)
                    {
                        sample = (float)(midPattern * volume * (1 + random.NextDouble() * 0.2));
                        midBand[(i - fftSize / 8) % midBand.Length] = sample;
                    }
                    // Treble frequencies (high indices)
                    else
                    {
                        sample = (float)(treblePattern * volume * (1 + random.NextDouble() * 0.1));
                        trebleBand[(i - fftSize / 2) % trebleBand.Length] = sample;
                    }
                    
                    audioData[i] = Math.Max(0, Math.Min(1, sample));
                }

                // Perform FFT to get frequency data
                PerformFFT(audioData, frequencyData);

                // Raise event with updated data including frequency bands
                AudioDataUpdated?.Invoke(this, new AudioDataEventArgs(audioData, frequencyData, volume, 
                    bassBand, midBand, trebleBand, beatIntensity));
            }
        }

        private void PerformFFT(float[] input, float[] output)
        {
            int n = input.Length;
            int m = (int)Math.Log(n, 2);

            // Bit-reversal permutation
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, m);
                if (j > i)
                {
                    float temp = input[i];
                    input[i] = input[j];
                    input[j] = temp;
                }
            }

            // Cooley-Tukey FFT
            for (int s = 1; s <= m; s++)
            {
                int m2 = 1 << s;
                int m1 = m2 >> 1;
                float[] wm = new float[2];
                wm[0] = (float)Math.Cos(2 * Math.PI / m2);
                wm[1] = (float)Math.Sin(2 * Math.PI / m2);

                for (int k = 0; k < n; k += m2)
                {
                    float[] w = new float[2] { 1, 0 };
                    for (int j = 0; j < m1; j++)
                    {
                        int idx1 = k + j;
                        int idx2 = k + j + m1;

                        float tRe = w[0] * input[idx2] - w[1] * 0;
                        float tIm = w[0] * 0 + w[1] * input[idx2];

                        input[idx2] = input[idx1] - tRe;
                        input[idx1] = input[idx1] + tRe;

                        float wRe = w[0] * wm[0] - w[1] * wm[1];
                        float wIm = w[0] * wm[1] + w[1] * wm[0];
                        w[0] = wRe;
                        w[1] = wIm;
                    }
                }
            }

            // Copy magnitude to output
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = (float)Math.Sqrt(input[i] * input[i] + input[i + n/2] * input[i + n/2]);
            }
        }

        private int ReverseBits(int n, int bits)
        {
            int reversed = 0;
            for (int i = 0; i < bits; i++)
            {
                reversed = (reversed << 1) | (n & 1);
                n >>= 1;
            }
            return reversed;
        }

        public float[] GetAudioData() => audioData;
        public float[] GetFrequencyData() => frequencyData;
    }

    public class AudioDataEventArgs : EventArgs
    {
        public float[] AudioData { get; }
        public float[] FrequencyData { get; }
        public float Volume { get; }
        public float[] BassBand { get; }
        public float[] MidBand { get; }
        public float[] TrebleBand { get; }
        public float BeatIntensity { get; }

        public AudioDataEventArgs(float[] audioData, float[] frequencyData, float volume)
        {
            AudioData = audioData;
            FrequencyData = frequencyData;
            Volume = volume;
            BassBand = new float[0];
            MidBand = new float[0];
            TrebleBand = new float[0];
            BeatIntensity = 0;
        }

        public AudioDataEventArgs(float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            AudioData = audioData;
            FrequencyData = frequencyData;
            Volume = volume;
            BassBand = bassBand;
            MidBand = midBand;
            TrebleBand = trebleBand;
            BeatIntensity = beatIntensity;
        }
    }
}
