using System;
using System.Drawing;
using System.Windows.Forms;
using Quispe_Almache_ReproductorMusical.Models;
using Quispe_Almache_ReproductorMusical;

namespace Quispe_Almache_ReproductorMusical.Views
{
    public partial class FrmReproductor : Form
    {
        private Visualizador visualizadorActual;
        private Timer temporizadorAnimacion;
        private Bitmap mapaBitsVisualizacion;
        private Graphics graficosVisualizacion;
        private DateTime tiempoInicioReproduccion;
        private TimeSpan duracionTotal;

        public event EventHandler<string> ArchivoCargadoSolicitado;
        public event EventHandler ReproduccionSolicitada;
        public event EventHandler PausaSolicitada;
        public event EventHandler DetencionSolicitada;
        public event EventHandler<float> VolumenCambiado;
        public event EventHandler TemporizadorTick;
        public event EventHandler<FormClosingEventArgs> CierreFormularioSolicitado;

        public FrmReproductor()
        {
            InitializeComponent();
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            visualizadorActual = new VisualizadorBarrasEspectro(panelVisualizacion.Width, panelVisualizacion.Height);

            mapaBitsVisualizacion = new Bitmap(panelVisualizacion.Width, panelVisualizacion.Height);
            graficosVisualizacion = Graphics.FromImage(mapaBitsVisualizacion);

            temporizadorAnimacion = new Timer();
            temporizadorAnimacion.Interval = 30; // ~33 FPS
            temporizadorAnimacion.Tick += TemporizadorAnimacion_Tick;

            cmbModoVisualizacion.SelectedIndex = 0;
            duracionTotal = TimeSpan.Zero;
        }

        private void FrmReproductor_Load(object sender, EventArgs e)
        {
            ActualizarEstado("Listo");
        }

        private void FrmReproductor_Resize(object sender, EventArgs e)
        {
            if (mapaBitsVisualizacion != null && panelVisualizacion.Width > 0 && panelVisualizacion.Height > 0)
            {
                mapaBitsVisualizacion.Dispose();
                mapaBitsVisualizacion = new Bitmap(panelVisualizacion.Width, panelVisualizacion.Height);
                graficosVisualizacion.Dispose();
                graficosVisualizacion = Graphics.FromImage(mapaBitsVisualizacion);

                if (visualizadorActual != null)
                {
                    visualizadorActual.EstablecerTamanio(panelVisualizacion.Width, panelVisualizacion.Height);
                }
            }
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            OpenFileDialog abrirDialogoArchivo = new OpenFileDialog();
            abrirDialogoArchivo.Filter = "Archivos de Audio|*.wav;*.mp3|Todos los archivos|*.*";
            abrirDialogoArchivo.Title = "Seleccionar archivo de audio";

            if (abrirDialogoArchivo.ShowDialog() == DialogResult.OK)
            {
                ArchivoCargadoSolicitado?.Invoke(this, abrirDialogoArchivo.FileName);
                lblNombreArchivo.Text = "Archivo: " + System.IO.Path.GetFileName(abrirDialogoArchivo.FileName);
                ActualizarEstado("Archivo cargado");
                
                try
                {
                    var informacionArchivo = new System.IO.FileInfo(abrirDialogoArchivo.FileName);
                    double minutosEstimados = informacionArchivo.Length / (1024.0 * 1024.0);
                    duracionTotal = TimeSpan.FromMinutes(minutosEstimados);
                }
                catch
                {
                    duracionTotal = TimeSpan.FromMinutes(3);
                }
            }
        }

        private void btnReproducir_Click(object sender, EventArgs e)
        {
            ReproduccionSolicitada?.Invoke(this, EventArgs.Empty);
            temporizadorAnimacion.Start();
            tiempoInicioReproduccion = DateTime.Now;
            ActualizarEstado("Reproduciendo");
        }

        private void btnPausar_Click(object sender, EventArgs e)
        {
            PausaSolicitada?.Invoke(this, EventArgs.Empty);
            temporizadorAnimacion.Stop();
            ActualizarEstado("Pausado");
        }

        private void btnDetener_Click(object sender, EventArgs e)
        {
            DetencionSolicitada?.Invoke(this, EventArgs.Empty);
            temporizadorAnimacion.Stop();
            ActualizarEstado("Detenido");
            lblTiempo.Text = "00:00 / " + FormatearTiempo(duracionTotal);
            
            if (graficosVisualizacion != null)
            {
                graficosVisualizacion.Clear(Color.Black);
                panelVisualizacion.Invalidate();
            }
        }

        private void barraVolumen_Scroll(object sender, EventArgs e)
        {
            float volumen = barraVolumen.Value / 100.0f;
            VolumenCambiado?.Invoke(this, volumen);
            lblVolumen.Text = "Volumen: " + barraVolumen.Value + "%";
        }

        private void cmbModoVisualizacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbModoVisualizacion.SelectedIndex)
            {
                case 0:
                    visualizadorActual = new VisualizadorBarrasEspectro(panelVisualizacion.Width, panelVisualizacion.Height);
                    break;
                case 1:
                    visualizadorActual = new VisualizadorOnda(panelVisualizacion.Width, panelVisualizacion.Height);
                    break;
                case 2:
                    visualizadorActual = new VisualizadorParticulas(panelVisualizacion.Width, panelVisualizacion.Height);
                    break;
                case 3:
                    visualizadorActual = new VisualizadorFormasGeometricas(panelVisualizacion.Width, panelVisualizacion.Height);
                    break;
                case 4:
                    visualizadorActual = new VisualizadorEspectroCircular(panelVisualizacion.Width, panelVisualizacion.Height);
                    break;
            }
        }

        private void TemporizadorAnimacion_Tick(object sender, EventArgs e)
        {
            TemporizadorTick?.Invoke(this, EventArgs.Empty);
        }

        public void ActualizarTiempoReproduccion()
        {
            TimeSpan transcurrido = DateTime.Now - tiempoInicioReproduccion;
            lblTiempo.Text = FormatearTiempo(transcurrido) + " / " + FormatearTiempo(duracionTotal);
        }

        public void RenderizarDatosAudio(DatosAudioEventArgs e)
        {
            if (visualizadorActual != null && graficosVisualizacion != null)
            {
                visualizadorActual.Renderizar(graficosVisualizacion, e.DatosAudio, e.DatosFrecuencia, e.Volumen, 
                    e.BandaBajos, e.BandaMedios, e.BandaAltos, e.IntensidadGolpe);
                panelVisualizacion.CreateGraphics().DrawImage(mapaBitsVisualizacion, 0, 0);
            }
        }

        private void ActualizarEstado(string estado)
        {
            lblEstado.Text = "Estado: " + estado;
        }

        private string FormatearTiempo(TimeSpan tiempo)
        {
            if (tiempo.TotalHours >= 1)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", 
                    (int)tiempo.TotalHours, tiempo.Minutes, tiempo.Seconds);
            }
            else
            {
                return string.Format("{0:00}:{1:00}", tiempo.Minutes, tiempo.Seconds);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            temporizadorAnimacion?.Stop();
            temporizadorAnimacion?.Dispose();
            graficosVisualizacion?.Dispose();
            mapaBitsVisualizacion?.Dispose();
            CierreFormularioSolicitado?.Invoke(this, e);
            base.OnFormClosing(e);
        }
    }
}
