using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Quispe_Almache_ReproductorMusical
{
    public enum VisualizationMode
    {
        SpectrumBars,
        Waveform,
        Particles,
        GeometricShapes,
        CircularSpectrum
    }

    public abstract class Visualizer
    {
        protected int width;
        protected int height;
        protected Random random;
        protected Color baseColor;
        protected List<Color> colorPalette;

        public Visualizer(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.random = new Random();
            this.baseColor = Color.FromArgb(0, 120, 215);
            InitializeColorPalette();
        }

        protected void InitializeColorPalette()
        {
            colorPalette = new List<Color>
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

        public abstract void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity);

        protected Color GetColorFromPalette(int index)
        {
            return colorPalette[index % colorPalette.Count];
        }

        protected Color GetColorByIntensity(float intensity)
        {
            // Clamp intensity to valid range [0, 1]
            intensity = Math.Max(0, Math.Min(1, intensity));
            
            int r = (int)(intensity * 255);
            int g = (int)((1 - intensity) * 150);
            int b = (int)(intensity * 200 + 55);
            return Color.FromArgb(Math.Max(0, Math.Min(255, r)), Math.Max(0, Math.Min(255, g)), Math.Max(0, Math.Min(255, b)));
        }

        public void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public class SpectrumBarsVisualizer : Visualizer
    {
        private int barCount;
        private float[] barHeights;
        private float[] targetHeights;
        private float smoothingFactor = 0.3f;

        public SpectrumBarsVisualizer(int width, int height) : base(width, height)
        {
            barCount = 64;
            barHeights = new float[barCount];
            targetHeights = new float[barCount];
        }

        public override void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            int barWidth = width / barCount;
            int gap = 2;

            // Update target heights based on frequency data with beat reaction
            for (int i = 0; i < barCount && i < frequencyData.Length; i++)
            {
                float beatBoost = beatIntensity > 0.3f ? 1.3f : 1.0f;
                targetHeights[i] = frequencyData[i] * height * 0.8f * volume * beatBoost;
            }

            // Smooth interpolation
            for (int i = 0; i < barCount; i++)
            {
                barHeights[i] += (targetHeights[i] - barHeights[i]) * smoothingFactor;
            }

            // Draw bars
            for (int i = 0; i < barCount; i++)
            {
                int x = i * barWidth;
                int barHeight = (int)barHeights[i];
                int y = height - barHeight;

                Color barColor = GetColorByIntensity(barHeights[i] / height);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(x, y, barWidth - gap, barHeight),
                    barColor,
                    Color.FromArgb(barColor.R / 2, barColor.G / 2, barColor.B / 2),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, x, y, barWidth - gap, barHeight);
                }

                // Add reflection effect
                int reflectionHeight = barHeight / 3;
                using (LinearGradientBrush reflectionBrush = new LinearGradientBrush(
                    new Rectangle(x, height, barWidth - gap, reflectionHeight),
                    Color.FromArgb(50, barColor),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(reflectionBrush, x, height, barWidth - gap, reflectionHeight);
                }
            }
        }
    }

    public class WaveformVisualizer : Visualizer
    {
        private float[] previousWaveform;
        private List<PointF> wavePoints;

        public WaveformVisualizer(int width, int height) : base(width, height)
        {
            previousWaveform = new float[512];
            wavePoints = new List<PointF>();
        }

        public override void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            wavePoints.Clear();

            int centerY = height / 2;
            int step = width / audioData.Length;

            // Draw waveform with bass influence
            for (int i = 0; i < audioData.Length; i++)
            {
                // Clamp audio data to valid range [0, 1]
                float normalizedData = Math.Max(0, Math.Min(1, audioData[i]));
                float bassInfluence = bassBand.Length > 0 ? bassBand[i % bassBand.Length] : 0;
                float amplitude = normalizedData * height * 0.4f * volume * (1 + bassInfluence * 0.5f);
                float x = i * step;
                float y = centerY - amplitude;

                wavePoints.Add(new PointF(x, y));
            }

            if (wavePoints.Count > 1)
            {
                // Draw main wave
                using (Pen pen = new Pen(Color.FromArgb(0, 255, 200), 2))
                {
                    g.DrawLines(pen, wavePoints.ToArray());
                }

                // Draw mirrored wave
                for (int i = 0; i < wavePoints.Count; i++)
                {
                    wavePoints[i] = new PointF(wavePoints[i].X, centerY + (centerY - wavePoints[i].Y));
                }

                using (Pen pen = new Pen(Color.FromArgb(0, 100, 255), 1))
                {
                    g.DrawLines(pen, wavePoints.ToArray());
                }

                // Add glow effect
                for (int i = 0; i < wavePoints.Count; i++)
                {
                    float intensity = audioData[i % audioData.Length];
                    int alpha = Math.Max(0, Math.Min(255, (int)(intensity * 100)));
                    Color glowColor = Color.FromArgb(alpha, 0, 255, 200);
                    using (Brush brush = new SolidBrush(glowColor))
                    {
                        g.FillEllipse(brush, wavePoints[i].X - 3, wavePoints[i].Y - 3, 6, 6);
                    }
                }
            }
        }
    }

    public class ParticleVisualizer : Visualizer
    {
        private List<Particle> particles;
        private int maxParticles = 200;

        public ParticleVisualizer(int width, int height) : base(width, height)
        {
            particles = new List<Particle>();
        }

        public override void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            // Spawn new particles based on audio intensity and beat
            float avgIntensity = frequencyData.Average();
            float bassEnergy = bassBand.Length > 0 ? bassBand.Average() : 0;
            int particlesToSpawn = (int)(avgIntensity * volume * 10);
            
            // Extra particles on beat
            if (beatIntensity > 0.3f)
            {
                particlesToSpawn += (int)(beatIntensity * 20);
            }

            for (int i = 0; i < particlesToSpawn && particles.Count < maxParticles; i++)
            {
                float intensity = frequencyData[i % frequencyData.Length] * volume;
                particles.Add(new Particle(
                    width / 2,
                    height / 2,
                    random,
                    intensity,
                    bassEnergy
                ));
            }

            // Update and draw particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(width, height, frequencyData, volume);
                particles[i].Draw(g);

                if (particles[i].IsDead())
                {
                    particles.RemoveAt(i);
                }
            }

            // Draw center pulse with beat reaction
            float pulseRadius = 50 + avgIntensity * volume * 100;
            if (beatIntensity > 0.3f)
            {
                pulseRadius *= 1.5f; // Pulse expands on beat
            }
            int pulseAlpha = Math.Max(0, Math.Min(255, (int)(avgIntensity * 100)));
            using (Brush pulseBrush = new SolidBrush(Color.FromArgb(pulseAlpha, 255, 100, 100)))
            {
                g.FillEllipse(pulseBrush, 
                    width / 2 - pulseRadius / 2, 
                    height / 2 - pulseRadius / 2, 
                    pulseRadius, 
                    pulseRadius);
            }
        }
    }

    public class Particle
    {
        private float x, y;
        private float vx, vy;
        private float life;
        private float maxLife;
        private float size;
        private Color color;
        private Random random;

        public Particle(float startX, float startY, Random random, float intensity, float bassEnergy)
        {
            this.random = random;
            this.x = startX;
            this.y = startY;
            
            float angle = random.NextFloat(0, (float)Math.PI * 2);
            float speed = random.NextFloat(2, 8) * intensity * (1 + bassEnergy * 0.5f);
            this.vx = (float)Math.Cos(angle) * speed;
            this.vy = (float)Math.Sin(angle) * speed;
            
            this.life = 1.0f;
            this.maxLife = random.NextFloat(0.5f, 2.0f);
            this.size = random.NextFloat(2, 8);
            
            this.color = Color.FromArgb(
                random.Next(100, 255),
                random.Next(100, 255),
                random.Next(100, 255)
            );
        }

        public void Update(int width, int height, float[] frequencyData, float volume)
        {
            x += vx;
            y += vy;
            life -= 0.02f;

            // React to bass frequencies
            if (frequencyData.Length > 0)
            {
                float bass = frequencyData[0] * volume;
                vx *= (1 + bass * 0.1f);
                vy *= (1 + bass * 0.1f);
            }
        }

        public void Draw(Graphics g)
        {
            float alpha = Math.Max(0, Math.Min(1, life));
            int r = Math.Max(0, Math.Min(255, (int)(alpha * color.R)));
            int green = Math.Max(0, Math.Min(255, (int)(alpha * color.G)));
            int b = Math.Max(0, Math.Min(255, (int)(alpha * color.B)));
            Color drawColor = Color.FromArgb(r, green, b);

            // Ensure size is at least 1 to avoid zero-size rectangles
            float drawSize = Math.Max(1, size);
            
            using (Brush brush = new SolidBrush(drawColor))
            {
                g.FillEllipse(brush, x - drawSize / 2, y - drawSize / 2, drawSize, drawSize);
            }
        }

        public bool IsDead()
        {
            return life <= 0;
        }
    }

    public class GeometricShapesVisualizer : Visualizer
    {
        private float rotation;
        private List<GeometricShape> shapes;

        public GeometricShapesVisualizer(int width, int height) : base(width, height)
        {
            rotation = 0;
            shapes = new List<GeometricShape>();
            InitializeShapes();
        }

        private void InitializeShapes()
        {
            shapes.Add(new GeometricShape(3, 100, Color.Red)); // Triangle
            shapes.Add(new GeometricShape(4, 150, Color.Blue)); // Square
            shapes.Add(new GeometricShape(5, 120, Color.Green)); // Pentagon
            shapes.Add(new GeometricShape(6, 180, Color.Purple)); // Hexagon
            shapes.Add(new GeometricShape(8, 140, Color.Orange)); // Octagon
        }

        public override void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            float bassEnergy = bassBand.Length > 0 ? bassBand.Average() : 0;
            float midEnergy = midBand.Length > 0 ? midBand.Average() : 0;
            float trebleEnergy = trebleBand.Length > 0 ? trebleBand.Average() : 0;
            
            rotation += 0.02f + frequencyData[0] * 0.1f * volume;
            
            // React to beat
            if (beatIntensity > 0.3f)
            {
                rotation += 0.1f; // Extra rotation on beat
            }

            float avgIntensity = frequencyData.Average() * volume;

            // Draw shapes with frequency band influence
            for (int i = 0; i < shapes.Count; i++)
            {
                float scale = 1 + avgIntensity * 2;
                
                // Different shapes react to different frequency bands
                if (i == 0) scale *= (1 + bassEnergy * 0.5f); // Bass affects first shape
                else if (i == 1) scale *= (1 + midEnergy * 0.5f); // Mid affects second shape
                else scale *= (1 + trebleEnergy * 0.5f); // Treble affects others
                
                shapes[i].Draw(g, width / 2, height / 2, rotation + i * 0.5f, scale, avgIntensity);
            }

            // Draw connecting lines
            using (Pen linePen = new Pen(Color.FromArgb(100, 255, 255), 1))
            {
                for (int i = 0; i < shapes.Count - 1; i++)
                {
                    float angle1 = rotation + i * 0.5f;
                    float angle2 = rotation + (i + 1) * 0.5f;
                    
                    PointF p1 = new PointF(
                        width / 2 + (float)Math.Cos(angle1) * 200,
                        height / 2 + (float)Math.Sin(angle1) * 200
                    );
                    PointF p2 = new PointF(
                        width / 2 + (float)Math.Cos(angle2) * 200,
                        height / 2 + (float)Math.Sin(angle2) * 200
                    );
                    
                    g.DrawLine(linePen, p1, p2);
                }
            }
        }
    }

    public class GeometricShape
    {
        private int sides;
        private float baseRadius;
        private Color color;

        public GeometricShape(int sides, float radius, Color color)
        {
            this.sides = sides;
            this.baseRadius = radius;
            this.color = color;
        }

        public void Draw(Graphics g, float centerX, float centerY, float rotation, float scale, float intensity)
        {
            float radius = baseRadius * scale;
            PointF[] points = new PointF[sides];

            for (int i = 0; i < sides; i++)
            {
                float angle = rotation + (i * 2 * (float)Math.PI / sides);
                points[i] = new PointF(
                    centerX + (float)Math.Cos(angle) * radius,
                    centerY + (float)Math.Sin(angle) * radius
                );
            }

            // Clamp intensity to valid range
            intensity = Math.Max(0, Math.Min(1, intensity));
            
            int r = Math.Max(0, Math.Min(255, (int)(color.R * (0.5 + intensity * 0.5))));
            int green = Math.Max(0, Math.Min(255, (int)(color.G * (0.5 + intensity * 0.5))));
            int b = Math.Max(0, Math.Min(255, (int)(color.B * (0.5 + intensity * 0.5))));
            Color drawColor = Color.FromArgb(r, green, b);

            using (Pen pen = new Pen(drawColor, 3))
            {
                g.DrawPolygon(pen, points);
            }

            int fillAlpha = Math.Max(0, Math.Min(255, (int)(intensity * 100)));
            using (Brush brush = new SolidBrush(Color.FromArgb(fillAlpha, drawColor)))
            {
                g.FillPolygon(brush, points);
            }
        }
    }

    public class CircularSpectrumVisualizer : Visualizer
    {
        private int barCount;
        private float[] barHeights;
        private float[] targetHeights;

        public CircularSpectrumVisualizer(int width, int height) : base(width, height)
        {
            barCount = 128;
            barHeights = new float[barCount];
            targetHeights = new float[barCount];
        }

        public override void Render(Graphics g, float[] audioData, float[] frequencyData, float volume, 
            float[] bassBand, float[] midBand, float[] trebleBand, float beatIntensity)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            float centerX = width / 2;
            float centerY = height / 2;
            float baseRadius = Math.Min(width, height) * 0.25f;

            // Update target heights with beat reaction
            for (int i = 0; i < barCount && i < frequencyData.Length; i++)
            {
                float beatBoost = beatIntensity > 0.3f ? 1.4f : 1.0f;
                targetHeights[i] = frequencyData[i] * baseRadius * volume * beatBoost;
            }

            // Smooth interpolation
            for (int i = 0; i < barCount; i++)
            {
                barHeights[i] += (targetHeights[i] - barHeights[i]) * 0.3f;
            }

            // Draw circular spectrum
            for (int i = 0; i < barCount; i++)
            {
                float angle = (i * 2 * (float)Math.PI / barCount) - (float)Math.PI / 2;
                float barLength = barHeights[i];

                float x1 = centerX + (float)Math.Cos(angle) * baseRadius;
                float y1 = centerY + (float)Math.Sin(angle) * baseRadius;
                float x2 = centerX + (float)Math.Cos(angle) * (baseRadius + barLength);
                float y2 = centerY + (float)Math.Sin(angle) * (baseRadius + barLength);

                Color barColor = GetColorByIntensity(barLength / baseRadius);
                using (Pen pen = new Pen(barColor, 2))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            // Draw center circle with beat reaction
            float avgIntensity = frequencyData.Average() * volume;
            float centerRadius = baseRadius * 0.8f + avgIntensity * 20;
            if (beatIntensity > 0.3f)
            {
                centerRadius *= 1.3f; // Pulse on beat
            }
            int centerAlpha = Math.Max(0, Math.Min(255, (int)(avgIntensity * 150)));
            using (Brush centerBrush = new SolidBrush(Color.FromArgb(centerAlpha, 0, 150, 255)))
            {
                g.FillEllipse(centerBrush, 
                    centerX - centerRadius, 
                    centerY - centerRadius, 
                    centerRadius * 2, 
                    centerRadius * 2);
            }
        }
    }

    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }
    }
}
