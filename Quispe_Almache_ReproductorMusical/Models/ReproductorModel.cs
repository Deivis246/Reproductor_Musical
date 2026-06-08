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

        private string archivoActual;
        private bool estaReproduciendo;
        private bool estaPausado;
        private float volumen = 1.0f;
        private float[] datosAudio;
        private float[] datosFrecuencia;
        private int tamanioFft = 512;
        private Random aleatorio;
        private string alias = "miAudio";
        private DateTime tiempoInicioReproduccion;
        private int contadorFrames;
        private float[] bandaBajos;
        private float[] bandaMedios;
        private float[] bandaAltos;
        private float intensidadGolpe;
        private float intensidadGolpeAnterior;

        public event EventHandler<DatosAudioEventArgs> DatosAudioActualizados;

        public ReproductorModel()
        {
            estaReproduciendo = false;
            estaPausado = false;
            datosAudio = new float[tamanioFft];
            datosFrecuencia = new float[tamanioFft / 2];
            bandaBajos = new float[32];
            bandaMedios = new float[64];
            bandaAltos = new float[128];
            aleatorio = new Random();
            contadorFrames = 0;
            intensidadGolpe = 0;
            intensidadGolpeAnterior = 0;
        }

        public void CargarArchivo(string rutaArchivo)
        {
            try
            {
                // Cerrar cualquier audio existente
                mciSendString($"close {alias}", null, 0, IntPtr.Zero);

                // Abrir el nuevo archivo
                string comando = $"open \"{rutaArchivo}\" type mpegvideo alias {alias}";
                int resultado = mciSendString(comando, null, 0, IntPtr.Zero);

                if (resultado == 0)
                {
                    archivoActual = rutaArchivo;
                    estaReproduciendo = false;
                    estaPausado = false;
                }
                else
                {
                    MessageBox.Show($"Error al cargar archivo: No se pudo abrir el archivo de audio. Código de error: {resultado}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar archivo: {ex.Message}");
            }
        }

        public void Reproducir()
        {
            if (archivoActual != null && !estaReproduciendo)
            {
                try
                {
                    int resultado = mciSendString($"play {alias}", null, 0, IntPtr.Zero);
                    if (resultado == 0)
                    {
                        estaReproduciendo = true;
                        estaPausado = false;
                        tiempoInicioReproduccion = DateTime.Now;
                        contadorFrames = 0;
                    }
                    else
                    {
                        MessageBox.Show($"Error al reproducir: Código de error: {resultado}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al reproducir: {ex.Message}");
                }
            }
        }

        public void Pausar()
        {
            if (estaReproduciendo)
            {
                try
                {
                    int resultado = mciSendString($"pause {alias}", null, 0, IntPtr.Zero);
                    if (resultado == 0)
                    {
                        estaPausado = true;
                        estaReproduciendo = false;
                    }
                    else
                    {
                        MessageBox.Show($"Error al pausar: Código de error: {resultado}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al pausar: {ex.Message}");
                }
            }
        }

        public void Detener()
        {
            try
            {
                int resultado = mciSendString($"stop {alias}", null, 0, IntPtr.Zero);
                if (resultado == 0)
                {
                    mciSendString($"seek {alias} to start", null, 0, IntPtr.Zero);
                    estaReproduciendo = false;
                    estaPausado = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener: {ex.Message}");
            }
        }

        public void EstablecerVolumen(float vol)
        {
            volumen = Math.Max(0, Math.Min(1, vol));
            uint nuevoVolumen = (uint)(volumen * 0xFFFF);
            waveOutSetVolume(IntPtr.Zero, nuevoVolumen | (nuevoVolumen << 16));
            
            // También establecer volumen MCI
            int volumenMci = (int)(volumen * 1000);
            mciSendString($"setaudio {alias} volume to {volumenMci}", null, 0, IntPtr.Zero);
        }

        public float ObtenerVolumen()
        {
            return volumen;
        }

        public bool EstaReproduciendo => estaReproduciendo;
        public bool EstaPausado => estaPausado;
        public string ArchivoActual => archivoActual;

        // Simular análisis de datos de audio con sincronización mejorada
        public void ActualizarAnalisis()
        {
            if (estaReproduciendo)
            {
                contadorFrames++;
                double tiempoDesdeInicio = (DateTime.Now - tiempoInicioReproduccion).TotalSeconds;
                
                // Simular patrones de audio realistas basados en tiempo
                double patronGolpe = Math.Sin(tiempoDesdeInicio * 8) * 0.5 + 0.5; // ~120 BPM
                double patronBajo = Math.Sin(tiempoDesdeInicio * 4) * 0.7 + 0.3;
                double patronMedio = Math.Sin(tiempoDesdeInicio * 12) * 0.4 + 0.6;
                double patronAlto = Math.Sin(tiempoDesdeInicio * 16) * 0.3 + 0.7;
                
                // Detectar golpe (aumento repentino de energía)
                float energiaGolpeActual = (float)patronBajo * volumen;
                intensidadGolpe = energiaGolpeActual - intensidadGolpeAnterior;
                intensidadGolpeAnterior = energiaGolpeActual;
                
                // Generar datos de audio con separación por bandas de frecuencia
                for (int i = 0; i < tamanioFft; i++)
                {
                    float muestra = 0;
                    
                    // Frecuencias bajas (índices bajos)
                    if (i < tamanioFft / 8)
                    {
                        muestra = (float)(patronBajo * volumen * (1 + aleatorio.NextDouble() * 0.3));
                        bandaBajos[i % bandaBajos.Length] = muestra;
                    }
                    // Frecuencias medias
                    else if (i < tamanioFft / 2)
                    {
                        muestra = (float)(patronMedio * volumen * (1 + aleatorio.NextDouble() * 0.2));
                        bandaMedios[(i - tamanioFft / 8) % bandaMedios.Length] = muestra;
                    }
                    // Frecuencias altas (índices altos)
                    else
                    {
                        muestra = (float)(patronAlto * volumen * (1 + aleatorio.NextDouble() * 0.1));
                        bandaAltos[(i - tamanioFft / 2) % bandaAltos.Length] = muestra;
                    }
                    
                    datosAudio[i] = Math.Max(0, Math.Min(1, muestra));
                }

                // Realizar FFT para obtener datos de frecuencia
                RealizarFFT(datosAudio, datosFrecuencia);

                // Lanzar evento con datos actualizados incluyendo bandas de frecuencia
                DatosAudioActualizados?.Invoke(this, new DatosAudioEventArgs(datosAudio, datosFrecuencia, volumen, 
                    bandaBajos, bandaMedios, bandaAltos, intensidadGolpe));
            }
        }

        private void RealizarFFT(float[] entrada, float[] salida)
        {
            int n = entrada.Length;
            int m = (int)Math.Log(n, 2);

            // Permutación bit a bit
            for (int i = 0; i < n; i++)
            {
                int j = InvertirBits(i, m);
                if (j > i)
                {
                    float temporal = entrada[i];
                    entrada[i] = entrada[j];
                    entrada[j] = temporal;
                }
            }

            // FFT Cooley-Tukey
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

                        float tRe = w[0] * entrada[idx2] - w[1] * 0;
                        float tIm = w[0] * 0 + w[1] * entrada[idx2];

                        entrada[idx2] = entrada[idx1] - tRe;
                        entrada[idx1] = entrada[idx1] + tRe;

                        float wRe = w[0] * wm[0] - w[1] * wm[1];
                        float wIm = w[0] * wm[1] + w[1] * wm[0];
                        w[0] = wRe;
                        w[1] = wIm;
                    }
                }
            }

            // Copiar magnitud a la salida
            for (int i = 0; i < salida.Length; i++)
            {
                salida[i] = (float)Math.Sqrt(entrada[i] * entrada[i] + entrada[i + n/2] * entrada[i + n/2]);
            }
        }

        private int InvertirBits(int n, int bits)
        {
            int invertido = 0;
            for (int i = 0; i < bits; i++)
            {
                invertido = (invertido << 1) | (n & 1);
                n >>= 1;
            }
            return invertido;
        }

        public float[] ObtenerDatosAudio() => datosAudio;
        public float[] ObtenerDatosFrecuencia() => datosFrecuencia;
    }

    public class DatosAudioEventArgs : EventArgs
    {
        public float[] DatosAudio { get; }
        public float[] DatosFrecuencia { get; }
        public float Volumen { get; }
        public float[] BandaBajos { get; }
        public float[] BandaMedios { get; }
        public float[] BandaAltos { get; }
        public float IntensidadGolpe { get; }

        public DatosAudioEventArgs(float[] datosAudio, float[] datosFrecuencia, float volumen)
        {
            DatosAudio = datosAudio;
            DatosFrecuencia = datosFrecuencia;
            Volumen = volumen;
            BandaBajos = new float[0];
            BandaMedios = new float[0];
            BandaAltos = new float[0];
            IntensidadGolpe = 0;
        }

        public DatosAudioEventArgs(float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            DatosAudio = datosAudio;
            DatosFrecuencia = datosFrecuencia;
            Volumen = volumen;
            BandaBajos = bandaBajos;
            BandaMedios = bandaMedios;
            BandaAltos = bandaAltos;
            IntensidadGolpe = intensidadGolpe;
        }
    }
}
