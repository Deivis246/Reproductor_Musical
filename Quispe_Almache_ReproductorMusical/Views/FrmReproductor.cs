using System;
using System.Drawing;
using System.Windows.Forms;
using Quispe_Almache_ReproductorMusical.Models;
using Quispe_Almache_ReproductorMusical;

namespace Quispe_Almache_ReproductorMusical.Views
{
    public partial class FrmReproductor : Form
    {
        private Visualizer currentVisualizer;
        private Timer animationTimer;
        private Bitmap visualizationBitmap;
        private Graphics visualizationGraphics;
        private DateTime playbackStartTime;
        private TimeSpan totalDuration;

        public event EventHandler<string> LoadFileRequested;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler StopRequested;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler TimerTicked;
        public event EventHandler<FormClosingEventArgs> FormClosingRequested;

        public FrmReproductor()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
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
            if (visualizationBitmap != null && visualizationPanel.Width > 0 && visualizationPanel.Height > 0)
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
                LoadFileRequested?.Invoke(this, openFileDialog.FileName);
                lblFileName.Text = "Archivo: " + System.IO.Path.GetFileName(openFileDialog.FileName);
                UpdateStatus("Archivo cargado");
                
                try
                {
                    var fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                    double estimatedMinutes = fileInfo.Length / (1024.0 * 1024.0);
                    totalDuration = TimeSpan.FromMinutes(estimatedMinutes);
                }
                catch
                {
                    totalDuration = TimeSpan.FromMinutes(3);
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
            animationTimer.Start();
            playbackStartTime = DateTime.Now;
            UpdateStatus("Reproduciendo");
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            PauseRequested?.Invoke(this, EventArgs.Empty);
            animationTimer.Stop();
            UpdateStatus("Pausado");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopRequested?.Invoke(this, EventArgs.Empty);
            animationTimer.Stop();
            UpdateStatus("Detenido");
            lblTime.Text = "00:00 / " + FormatTime(totalDuration);
            
            if (visualizationGraphics != null)
            {
                visualizationGraphics.Clear(Color.Black);
                visualizationPanel.Invalidate();
            }
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            float volume = trackBarVolume.Value / 100.0f;
            VolumeChanged?.Invoke(this, volume);
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
            TimerTicked?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePlaybackTime()
        {
            TimeSpan elapsed = DateTime.Now - playbackStartTime;
            lblTime.Text = FormatTime(elapsed) + " / " + FormatTime(totalDuration);
        }

        public void RenderAudioData(AudioDataEventArgs e)
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
            FormClosingRequested?.Invoke(this, e);
            base.OnFormClosing(e);
        }
    }
}
