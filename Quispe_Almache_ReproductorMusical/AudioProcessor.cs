using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quispe_Almache_ReproductorMusical
{
    public class AudioProcessor
    {
        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        private SoundPlayer player;
        private string currentFile;
        private bool isPlaying;
        private bool isPaused;
        private float volume = 1.0f;
        private float[] audioData;
        private float[] frequencyData;
        private int fftSize = 512;

        public event EventHandler<AudioDataEventArgs> AudioDataUpdated;

        public AudioProcessor()
        {
            player = new SoundPlayer();
            isPlaying = false;
            isPaused = false;
            audioData = new float[fftSize];
            frequencyData = new float[fftSize / 2];
        }

        public void LoadFile(string filePath)
        {
            try
            {
                player.Stop();
                player.SoundLocation = filePath;
                player.Load();
                currentFile = filePath;
                isPlaying = false;
                isPaused = false;
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
                    if (isPaused)
                    {
                        isPaused = false;
                    }
                    player.Play();
                    isPlaying = true;
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
                player.Stop();
                isPaused = true;
                isPlaying = false;
            }
        }

        public void Stop()
        {
            player.Stop();
            isPlaying = false;
            isPaused = false;
        }

        public void SetVolume(float vol)
        {
            volume = Math.Max(0, Math.Min(1, vol));
            uint newVolume = (uint)(volume * 0xFFFF);
            waveOutSetVolume(IntPtr.Zero, newVolume | (newVolume << 16));
        }

        public float GetVolume()
        {
            return volume;
        }

        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        public string CurrentFile => currentFile;

        // Simulate audio data analysis (since SoundPlayer doesn't provide real-time data)
        public void UpdateAnalysis()
        {
            if (isPlaying)
            {
                Random random = new Random();
                
                // Generate simulated audio data based on volume
                for (int i = 0; i < fftSize; i++)
                {
                    audioData[i] = (float)(random.NextDouble() * volume);
                }

                // Perform FFT to get frequency data
                PerformFFT(audioData, frequencyData);

                // Raise event with updated data
                AudioDataUpdated?.Invoke(this, new AudioDataEventArgs(audioData, frequencyData, volume));
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

        public AudioDataEventArgs(float[] audioData, float[] frequencyData, float volume)
        {
            AudioData = audioData;
            FrequencyData = frequencyData;
            Volume = volume;
        }
    }
}
