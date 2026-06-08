using System;
using System.Windows.Forms;
using Quispe_Almache_ReproductorMusical.Models;
using Quispe_Almache_ReproductorMusical.Views;

namespace Quispe_Almache_ReproductorMusical.Controllers
{
    public class ReproductorController
    {
        private readonly ReproductorModel _model;
        private readonly FrmReproductor _view;

        public ReproductorController(ReproductorModel model, FrmReproductor view)
        {
            _model = model;
            _view = view;

            // Subscribe to Model events
            _model.AudioDataUpdated += Model_AudioDataUpdated;

            // Subscribe to View events
            _view.LoadFileRequested += View_LoadFileRequested;
            _view.PlayRequested += View_PlayRequested;
            _view.PauseRequested += View_PauseRequested;
            _view.StopRequested += View_StopRequested;
            _view.VolumeChanged += View_VolumeChanged;
            _view.TimerTicked += View_TimerTicked;
            _view.FormClosingRequested += View_FormClosingRequested;
        }

        public void Run()
        {
            Application.Run(_view);
        }

        private void View_LoadFileRequested(object sender, string filePath)
        {
            _model.LoadFile(filePath);
        }

        private void View_PlayRequested(object sender, EventArgs e)
        {
            _model.Play();
        }

        private void View_PauseRequested(object sender, EventArgs e)
        {
            _model.Pause();
        }

        private void View_StopRequested(object sender, EventArgs e)
        {
            _model.Stop();
        }

        private void View_VolumeChanged(object sender, float volume)
        {
            _model.SetVolume(volume);
        }

        private void View_TimerTicked(object sender, EventArgs e)
        {
            _model.UpdateAnalysis();
            if (_model.IsPlaying)
            {
                _view.UpdatePlaybackTime();
            }
        }

        private void Model_AudioDataUpdated(object sender, AudioDataEventArgs e)
        {
            _view.RenderAudioData(e);
        }
        
        private void View_FormClosingRequested(object sender, FormClosingEventArgs e)
        {
            _model.Stop();
        }
    }
}
