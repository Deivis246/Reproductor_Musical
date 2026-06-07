using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quispe_Almache_ReproductorMusical
{
    public partial class Form1 : Form
    {
        private AudioProcessor audioProcessor;
        private Visualizer currentVisualizer;
        private Timer animationTimer;
        private Bitmap visualizationBitmap;
        private Graphics visualizationGraphics;
        private DateTime playbackStartTime;
        private TimeSpan totalDuration;

        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            audioProcessor = new AudioProcessor();
            audioProcessor.AudioDataUpdated += AudioProcessor_AudioDataUpdated;

            currentVisualizer = new SpectrumBarsVisualizer(visualizationPanel.Width, visualizationPanel.Height);

            visualizationBitmap = new Bitmap(visualizationPanel.Width, visualizationPanel.Height);
            visualizationGraphics = Graphics.FromImage(visualizationBitmap);

            animationTimer = new Timer();
            animationTimer.Interval = 30; // ~33 FPS
            animationTimer.Tick += AnimationTimer_Tick;

            cmbVisualizationMode.SelectedIndex = 0;
            totalDuration = TimeSpan.Zero;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateStatus("Listo");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (visualizationBitmap != null)
            {
                visualizationBitmap.Dispose();
                visualizationBitmap = new Bitmap(visualizationPanel.Width, visualizationPanel.Height);
                visualizationGraphics.Dispose();
                visualizationGraphics = Graphics.FromImage(visualizationBitmap);

                if (currentVisualizer != null)
                {
                    currentVisualizer.SetSize(visualizationPanel.Width, visualizationPanel.Height);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de Audio|*.wav;*.mp3|Todos los archivos|*.*";
            openFileDialog.Title = "Seleccionar archivo de audio";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                audioProcessor.LoadFile(openFileDialog.FileName);
                lblFileName.Text = "Archivo: " + System.IO.Path.GetFileName(openFileDialog.FileName);
                UpdateStatus("Archivo cargado");
                
                // Estimate duration (this is a rough estimation)
                try
                {
                    var fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                    // Rough estimation: 1MB ≈ 1 minute for MP3 at 128kbps
                    double estimatedMinutes = fileInfo.Length / (1024.0 * 1024.0);
                    totalDuration = TimeSpan.FromMinutes(estimatedMinutes);
                }
                catch
                {
                    totalDuration = TimeSpan.FromMinutes(3); // Default 3 minutes
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (audioProcessor.CurrentFile != null)
            {
                audioProcessor.Play();
                animationTimer.Start();
                playbackStartTime = DateTime.Now;
                UpdateStatus("Reproduciendo");
            }
            else
            {
                MessageBox.Show("Por favor, cargue un archivo de audio primero.");
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            audioProcessor.Pause();
            animationTimer.Stop();
            UpdateStatus("Pausado");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            audioProcessor.Stop();
            animationTimer.Stop();
            UpdateStatus("Detenido");
            lblTime.Text = "00:00 / " + FormatTime(totalDuration);
            
            // Clear visualization
            if (visualizationGraphics != null)
            {
                visualizationGraphics.Clear(Color.Black);
                visualizationPanel.Invalidate();
            }
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            float volume = trackBarVolume.Value / 100.0f;
            audioProcessor.SetVolume(volume);
            lblVolume.Text = "Volumen: " + trackBarVolume.Value + "%";
        }

        private void cmbVisualizationMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbVisualizationMode.SelectedIndex)
            {
                case 0:
                    currentVisualizer = new SpectrumBarsVisualizer(visualizationPanel.Width, visualizationPanel.Height);
                    break;
                case 1:
                    currentVisualizer = new WaveformVisualizer(visualizationPanel.Width, visualizationPanel.Height);
                    break;
                case 2:
                    currentVisualizer = new ParticleVisualizer(visualizationPanel.Width, visualizationPanel.Height);
                    break;
                case 3:
                    currentVisualizer = new GeometricShapesVisualizer(visualizationPanel.Width, visualizationPanel.Height);
                    break;
                case 4:
                    currentVisualizer = new CircularSpectrumVisualizer(visualizationPanel.Width, visualizationPanel.Height);
                    break;
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            audioProcessor.UpdateAnalysis();
            
            // Update time display
            if (audioProcessor.IsPlaying)
            {
                TimeSpan elapsed = DateTime.Now - playbackStartTime;
                lblTime.Text = FormatTime(elapsed) + " / " + FormatTime(totalDuration);
            }
        }

        private void AudioProcessor_AudioDataUpdated(object sender, AudioDataEventArgs e)
        {
            if (currentVisualizer != null && visualizationGraphics != null)
            {
                currentVisualizer.Render(visualizationGraphics, e.AudioData, e.FrequencyData, e.Volume, 
                    e.BassBand, e.MidBand, e.TrebleBand, e.BeatIntensity);
                visualizationPanel.CreateGraphics().DrawImage(visualizationBitmap, 0, 0);
            }
        }

        private void UpdateStatus(string status)
        {
            lblStatus.Text = "Estado: " + status;
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", 
                    (int)time.TotalHours, time.Minutes, time.Seconds);
            }
            else
            {
                return string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            visualizationGraphics?.Dispose();
            visualizationBitmap?.Dispose();
            audioProcessor?.Stop();
            base.OnFormClosing(e);
        }
    }
}
