using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Quispe_Almache_ReproductorMusical.Views
{
    public enum ModoVisualizacion
    {
        BarrasEspectro,
        OndaSonora,
        Particulas,
        FormasGeometricas,
        EspectroCircular
    }

    public abstract class Visualizador
    {
        protected int ancho;
        protected int alto;
        protected Random aleatorio;
        protected Color colorBase;
        protected List<Color> paletaColores;

        public Visualizador(int ancho, int alto)
        {
            this.ancho = ancho;
            this.alto = alto;
            this.aleatorio = new Random();
            this.colorBase = Color.FromArgb(0, 120, 215);
            InicializarPaletaColores();
        }

        protected void InicializarPaletaColores()
        {
            paletaColores = new List<Color>
            {
                Color.FromArgb(255, 0, 100),
                Color.FromArgb(0, 150, 255),
                Color.FromArgb(100, 255, 100),
                Color.FromArgb(255, 200, 0),
                Color.FromArgb(200, 0, 255),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(255, 100, 100)
            };
        }

        public abstract void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe);

        protected Color ObtenerColorDePaleta(int indice)
        {
            return paletaColores[indice % paletaColores.Count];
        }

        protected Color ObtenerColorPorIntensidad(float intensidad)
        {
            // Limitar intensidad a un rango válido [0, 1]
            intensidad = Math.Max(0, Math.Min(1, intensidad));
            
            int r = (int)(intensidad * 255);
            int g = (int)((1 - intensidad) * 150);
            int b = (int)(intensidad * 200 + 55);
            return Color.FromArgb(Math.Max(0, Math.Min(255, r)), Math.Max(0, Math.Min(255, g)), Math.Max(0, Math.Min(255, b)));
        }

        public void EstablecerTamanio(int ancho, int alto)
        {
            this.ancho = ancho;
            this.alto = alto;
        }
    }

    public class VisualizadorBarrasEspectro : Visualizador
    {
        private int cantidadBarras;
        private float[] alturasBarras;
        private float[] alturasObjetivo;
        private float factorSuavizado = 0.3f;

        public VisualizadorBarrasEspectro(int ancho, int alto) : base(ancho, alto)
        {
            cantidadBarras = 64;
            alturasBarras = new float[cantidadBarras];
            alturasObjetivo = new float[cantidadBarras];
        }

        public override void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            int anchoBarra = ancho / cantidadBarras;
            int brecha = 2;

            // Actualizar alturas objetivo basadas en datos de frecuencia con reacción a golpe
            for (int i = 0; i < cantidadBarras && i < datosFrecuencia.Length; i++)
            {
                float aumentoGolpe = intensidadGolpe > 0.3f ? 1.3f : 1.0f;
                alturasObjetivo[i] = datosFrecuencia[i] * alto * 0.8f * volumen * aumentoGolpe;
            }

            // Interpolación suave
            for (int i = 0; i < cantidadBarras; i++)
            {
                alturasBarras[i] += (alturasObjetivo[i] - alturasBarras[i]) * factorSuavizado;
            }

            // Dibujar barras
            for (int i = 0; i < cantidadBarras; i++)
            {
                int x = i * anchoBarra;
                int altoBarra = Math.Max(1, (int)alturasBarras[i]); // Asegurar altura mínima de 1
                int y = alto - altoBarra;
                int anchoReal = Math.Max(1, anchoBarra - brecha); // Asegurar ancho mínimo de 1

                Color colorBarra = ObtenerColorPorIntensidad(alturasBarras[i] / alto);
                using (LinearGradientBrush brocha = new LinearGradientBrush(
                    new Rectangle(x, y, anchoReal, altoBarra),
                    colorBarra,
                    Color.FromArgb(colorBarra.R / 2, colorBarra.G / 2, colorBarra.B / 2),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brocha, x, y, anchoReal, altoBarra);
                }

                // Añadir efecto de reflejo
                int altoReflejo = Math.Max(1, altoBarra / 3); // Asegurar altura mínima de 1
                using (LinearGradientBrush brochaReflejo = new LinearGradientBrush(
                    new Rectangle(x, alto, anchoReal, altoReflejo),
                    Color.FromArgb(50, colorBarra),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brochaReflejo, x, alto, anchoReal, altoReflejo);
                }
            }
        }
    }

    public class VisualizadorOnda : Visualizador
    {
        private List<PointF> puntosOnda;

        public VisualizadorOnda(int ancho, int alto) : base(ancho, alto)
        {
            puntosOnda = new List<PointF>();
        }

        public override void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            puntosOnda.Clear();

            int centroY = alto / 2;
            float paso = (float)ancho / (datosAudio.Length > 1 ? datosAudio.Length - 1 : 1);

            // Dibujar forma de onda con influencia de bajos
            for (int i = 0; i < datosAudio.Length; i++)
            {
                // Limitar datos de audio a un rango válido [0, 1]
                float datosNormalizados = Math.Max(0, Math.Min(1, datosAudio[i]));
                float influenciaBajo = bandaBajos.Length > 0 ? bandaBajos[i % bandaBajos.Length] : 0;
                float amplitud = datosNormalizados * alto * 0.4f * volumen * (1 + influenciaBajo * 0.5f);
                float x = i * paso;
                float y = centroY - amplitud;

                puntosOnda.Add(new PointF(x, y));
            }

            if (puntosOnda.Count > 1)
            {
                // Dibujar onda principal
                using (Pen boligrafo = new Pen(Color.FromArgb(0, 255, 200), 2))
                {
                    g.DrawLines(boligrafo, puntosOnda.ToArray());
                }

                // Dibujar onda reflejada
                for (int i = 0; i < puntosOnda.Count; i++)
                {
                    puntosOnda[i] = new PointF(puntosOnda[i].X, centroY + (centroY - puntosOnda[i].Y));
                }

                using (Pen boligrafo = new Pen(Color.FromArgb(0, 100, 255), 1))
                {
                    g.DrawLines(boligrafo, puntosOnda.ToArray());
                }

                // Añadir efecto de brillo
                for (int i = 0; i < puntosOnda.Count; i++)
                {
                    float intensidad = datosAudio[i % datosAudio.Length];
                    int alfa = Math.Max(0, Math.Min(255, (int)(intensidad * 100)));
                    Color colorBrillo = Color.FromArgb(alfa, 0, 255, 200);
                    using (Brush brocha = new SolidBrush(colorBrillo))
                    {
                        g.FillEllipse(brocha, puntosOnda[i].X - 3, puntosOnda[i].Y - 3, 6, 6);
                    }
                }
            }
        }
    }

    public class VisualizadorParticulas : Visualizador
    {
        private List<Particula> particulas;
        private int particulasMaximas = 1000;

        public VisualizadorParticulas(int ancho, int alto) : base(ancho, alto)
        {
            particulas = new List<Particula>();
        }

        public override void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            float intensidadPromedio = datosFrecuencia.Average();
            float energiaBajos = bandaBajos.Length > 0 ? bandaBajos.Average() : 0;

            // Actualizar y dibujar particulas primero
            for (int i = particulas.Count - 1; i >= 0; i--)
            {
                particulas[i].Actualizar(ancho, alto, datosFrecuencia, volumen);
                particulas[i].Dibujar(g);

                if (particulas[i].EstaMuerta())
                {
                    particulas.RemoveAt(i);
                }
            }

            // Generar onda expansiva SOLO en golpes fuertes
            if (intensidadGolpe > 0.3f)
            {
                int particulasEnAnillo = 36; // 36 partículas forman un círculo
                float pasoAngulo = (float)(Math.PI * 2) / particulasEnAnillo;
                
                // Color basado en la energía de graves
                Color colorAnillo = Color.FromArgb(
                    Math.Max(50, Math.Min(255, (int)(energiaBajos * 300))),
                    Math.Max(50, Math.Min(255, (int)((1 - energiaBajos) * 150))),
                    200
                );

                for (int i = 0; i < particulasEnAnillo; i++)
                {
                    if (particulas.Count < particulasMaximas)
                    {
                        float angulo = i * pasoAngulo;
                        particulas.Add(new Particula(ancho / 2, alto / 2, angulo, intensidadGolpe * volumen * 20, colorAnillo));
                    }
                }
            }

            // Dibujar Núcleo Central que reacciona a intensidad promedio
            float radioPulso = 40 + intensidadPromedio * volumen * 150;
            if (intensidadGolpe > 0.3f)
            {
                radioPulso *= 1.3f; // Se expande enormemente en un golpe
            }
            int alfaPulso = Math.Max(0, Math.Min(255, (int)(intensidadPromedio * 200) + 50));
            using (Brush brochaPulso = new SolidBrush(Color.FromArgb(alfaPulso, 0, 150, 255)))
            {
                g.FillEllipse(brochaPulso, 
                    ancho / 2 - radioPulso / 2, 
                    alto / 2 - radioPulso / 2, 
                    radioPulso, 
                    radioPulso);
            }
        }
    }

    public class Particula
    {
        private float x, y;
        private float vx, vy;
        private float vida;
        private float tamanio;
        private Color color;

        public Particula(float inicioX, float inicioY, float angulo, float velocidad, Color color)
        {
            this.x = inicioX;
            this.y = inicioY;
            this.vx = (float)Math.Cos(angulo) * velocidad;
            this.vy = (float)Math.Sin(angulo) * velocidad;
            
            this.vida = 1.0f;
            this.tamanio = 6f; // Tamaño inicial de la partícula
            this.color = color;
        }

        public void Actualizar(int ancho, int alto, float[] datosFrecuencia, float volumen)
        {
            x += vx;
            y += vy;
            vida -= 0.03f; // Desaparecer con el tiempo
            tamanio += 0.2f; // Se expande poco a poco
        }

        public void Dibujar(Graphics g)
        {
            float alfa = Math.Max(0, Math.Min(1, vida));
            int r = Math.Max(0, Math.Min(255, (int)(alfa * color.R)));
            int verde = Math.Max(0, Math.Min(255, (int)(alfa * color.G)));
            int b = Math.Max(0, Math.Min(255, (int)(alfa * color.B)));
            Color colorDibujo = Color.FromArgb(r, verde, b);

            float tamanioDibujo = Math.Max(1, tamanio);
            
            using (Brush brocha = new SolidBrush(colorDibujo))
            {
                g.FillEllipse(brocha, x - tamanioDibujo / 2, y - tamanioDibujo / 2, tamanioDibujo, tamanioDibujo);
            }
            
            // Añadir un rastro de brillo hacia el centro
            using (Pen boligrafo = new Pen(Color.FromArgb((int)(alfa * 100), color), 2))
            {
                g.DrawLine(boligrafo, x, y, x - vx * 2, y - vy * 2);
            }
        }

        public bool EstaMuerta()
        {
            return vida <= 0;
        }
    }

    public class VisualizadorFormasGeometricas : Visualizador
    {
        private float rotacion;
        private List<FiguraGeometrica> figuras;

        public VisualizadorFormasGeometricas(int ancho, int alto) : base(ancho, alto)
        {
            rotacion = 0;
            figuras = new List<FiguraGeometrica>();
            InicializarFiguras();
        }

        private void InicializarFiguras()
        {
            figuras.Add(new FiguraGeometrica(3, 80, Color.Red));    // Triángulo (Bajos)
            figuras.Add(new FiguraGeometrica(6, 100, Color.Green)); // Hexágono (Medios)
            figuras.Add(new FiguraGeometrica(8, 80, Color.Blue));   // Octágono (Altos)
        }

        public override void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            float energiaBajos = bandaBajos.Length > 0 ? bandaBajos.Average() : 0;
            float energiaMedios = bandaMedios.Length > 0 ? bandaMedios.Average() : 0;
            float energiaAltos = bandaAltos.Length > 0 ? bandaAltos.Average() : 0;
            
            rotacion += 0.02f + datosFrecuencia[0] * 0.1f * volumen;
            
            // Reaccionar al golpe
            if (intensidadGolpe > 0.3f)
            {
                rotacion += 0.1f; // Rotación extra
            }

            // Dibujar 3 formas (Izquierda, Medio, Derecha)
            if (figuras.Count >= 3)
            {
                // Forma Bajos (Izquierda)
                float escala1 = 1 + energiaBajos * 3;
                figuras[0].Dibujar(g, ancho * 0.25f, alto / 2, rotacion, escala1, energiaBajos);

                // Forma Medios (Centro)
                float escala2 = 1 + energiaMedios * 3;
                figuras[1].Dibujar(g, ancho * 0.5f, alto / 2, -rotacion, escala2, energiaMedios);

                // Forma Altos (Derecha)
                float escala3 = 1 + energiaAltos * 3;
                figuras[2].Dibujar(g, ancho * 0.75f, alto / 2, rotacion * 1.5f, escala3, energiaAltos);
            }

            // Dibujar lineas conectoras
            using (Pen boligrafoLinea = new Pen(Color.FromArgb(100, 255, 255), 2))
            {
                g.DrawLine(boligrafoLinea, ancho * 0.25f, alto / 2, ancho * 0.5f, alto / 2);
                g.DrawLine(boligrafoLinea, ancho * 0.5f, alto / 2, ancho * 0.75f, alto / 2);
            }
        }
    }

    public class FiguraGeometrica
    {
        private int lados;
        private float radioBase;
        private Color color;

        public FiguraGeometrica(int lados, float radio, Color color)
        {
            this.lados = lados;
            this.radioBase = radio;
            this.color = color;
        }

        public void Dibujar(Graphics g, float centroX, float centroY, float rotacion, float escala, float intensidad)
        {
            float radio = radioBase * escala;
            PointF[] puntos = new PointF[lados];

            for (int i = 0; i < lados; i++)
            {
                float angulo = rotacion + (i * 2 * (float)Math.PI / lados);
                puntos[i] = new PointF(
                    centroX + (float)Math.Cos(angulo) * radio,
                    centroY + (float)Math.Sin(angulo) * radio
                );
            }

            // Limitar intensidad
            intensidad = Math.Max(0, Math.Min(1, intensidad));
            
            int r = Math.Max(0, Math.Min(255, (int)(color.R * (0.5 + intensidad * 0.5))));
            int verde = Math.Max(0, Math.Min(255, (int)(color.G * (0.5 + intensidad * 0.5))));
            int b = Math.Max(0, Math.Min(255, (int)(color.B * (0.5 + intensidad * 0.5))));
            Color colorDibujo = Color.FromArgb(r, verde, b);

            using (Pen boligrafo = new Pen(colorDibujo, 3))
            {
                g.DrawPolygon(boligrafo, puntos);
            }

            int alfaRelleno = Math.Max(0, Math.Min(255, (int)(intensidad * 100)));
            using (Brush brocha = new SolidBrush(Color.FromArgb(alfaRelleno, colorDibujo)))
            {
                g.FillPolygon(brocha, puntos);
            }
        }
    }

    public class VisualizadorEspectroCircular : Visualizador
    {
        private int cantidadBarras;
        private float[] alturasBarras;
        private float[] alturasObjetivo;

        public VisualizadorEspectroCircular(int ancho, int alto) : base(ancho, alto)
        {
            cantidadBarras = 128;
            alturasBarras = new float[cantidadBarras];
            alturasObjetivo = new float[cantidadBarras];
        }

        public override void Renderizar(Graphics g, float[] datosAudio, float[] datosFrecuencia, float volumen, 
            float[] bandaBajos, float[] bandaMedios, float[] bandaAltos, float intensidadGolpe)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            float centroX = ancho / 2;
            float centroY = alto / 2;
            float radioBase = Math.Min(ancho, alto) * 0.25f;

            // Actualizar alturas
            for (int i = 0; i < cantidadBarras && i < datosFrecuencia.Length; i++)
            {
                float aumentoGolpe = intensidadGolpe > 0.3f ? 1.4f : 1.0f;
                alturasObjetivo[i] = datosFrecuencia[i] * radioBase * volumen * aumentoGolpe;
            }

            // Interpolación
            for (int i = 0; i < cantidadBarras; i++)
            {
                alturasBarras[i] += (alturasObjetivo[i] - alturasBarras[i]) * 0.3f;
            }

            // Dibujar
            for (int i = 0; i < cantidadBarras; i++)
            {
                float angulo = (i * 2 * (float)Math.PI / cantidadBarras) - (float)Math.PI / 2;
                float largoBarra = alturasBarras[i];

                float x1 = centroX + (float)Math.Cos(angulo) * radioBase;
                float y1 = centroY + (float)Math.Sin(angulo) * radioBase;
                float x2 = centroX + (float)Math.Cos(angulo) * (radioBase + largoBarra);
                float y2 = centroY + (float)Math.Sin(angulo) * (radioBase + largoBarra);

                Color colorBarra = ObtenerColorPorIntensidad(largoBarra / radioBase);
                using (Pen boligrafo = new Pen(colorBarra, 2))
                {
                    g.DrawLine(boligrafo, x1, y1, x2, y2);
                }
            }

            // Centro
            float intensidadPromedio = datosFrecuencia.Average() * volumen;
            float radioCentro = radioBase * 0.8f + intensidadPromedio * 20;
            if (intensidadGolpe > 0.3f)
            {
                radioCentro *= 1.3f;
            }
            int alfaCentro = Math.Max(0, Math.Min(255, (int)(intensidadPromedio * 150)));
            using (Brush brochaCentro = new SolidBrush(Color.FromArgb(alfaCentro, 0, 150, 255)))
            {
                g.FillEllipse(brochaCentro, 
                    centroX - radioCentro, 
                    centroY - radioCentro, 
                    radioCentro * 2, 
                    radioCentro * 2);
            }
        }
    }

    public static class ExtensionesAleatorio
    {
        public static float NextFloat(this Random aleatorio, float min, float max)
        {
            return (float)aleatorio.NextDouble() * (max - min) + min;
        }
    }
}
