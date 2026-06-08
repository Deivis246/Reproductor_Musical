using System;
using System.Windows.Forms;
using Quispe_Almache_ReproductorMusical.Models;
using Quispe_Almache_ReproductorMusical.Views;

namespace Quispe_Almache_ReproductorMusical.Controllers
{
    public class ReproductorController
    {
        private readonly ReproductorModel _modelo;
        private readonly FrmReproductor _vista;

        public ReproductorController(ReproductorModel modelo, FrmReproductor vista)
        {
            _modelo = modelo;
            _vista = vista;

            // Suscribirse a eventos del Modelo
            _modelo.DatosAudioActualizados += Modelo_DatosAudioActualizados;

            // Suscribirse a eventos de la Vista
            _vista.ArchivoCargadoSolicitado += Vista_ArchivoCargadoSolicitado;
            _vista.ReproduccionSolicitada += Vista_ReproduccionSolicitada;
            _vista.PausaSolicitada += Vista_PausaSolicitada;
            _vista.DetencionSolicitada += Vista_DetencionSolicitada;
            _vista.VolumenCambiado += Vista_VolumenCambiado;
            _vista.TemporizadorTick += Vista_TemporizadorTick;
            _vista.CierreFormularioSolicitado += Vista_CierreFormularioSolicitado;
        }

        public void Iniciar()
        {
            Application.Run(_vista);
        }

        private void Vista_ArchivoCargadoSolicitado(object sender, string rutaArchivo)
        {
            _modelo.CargarArchivo(rutaArchivo);
        }

        private void Vista_ReproduccionSolicitada(object sender, EventArgs e)
        {
            _modelo.Reproducir();
        }

        private void Vista_PausaSolicitada(object sender, EventArgs e)
        {
            _modelo.Pausar();
        }

        private void Vista_DetencionSolicitada(object sender, EventArgs e)
        {
            _modelo.Detener();
        }

        private void Vista_VolumenCambiado(object sender, float volumen)
        {
            _modelo.EstablecerVolumen(volumen);
        }

        private void Vista_TemporizadorTick(object sender, EventArgs e)
        {
            _modelo.ActualizarAnalisis();
            if (_modelo.EstaReproduciendo)
            {
                _vista.ActualizarTiempoReproduccion();
            }
        }

        private void Modelo_DatosAudioActualizados(object sender, DatosAudioEventArgs e)
        {
            _vista.RenderizarDatosAudio(e);
        }
        
        private void Vista_CierreFormularioSolicitado(object sender, FormClosingEventArgs e)
        {
            _modelo.Detener();
        }
    }
}
