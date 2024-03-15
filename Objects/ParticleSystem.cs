using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CS5410
{
    public class ParticleSystem
    {
        private Dictionary<long, Particle> m_particles = new Dictionary<long, Particle>();
        public Dictionary<long, Particle>.ValueCollection particles { get { return m_particles.Values; } }
        private MyRandom m_random = new MyRandom();

        private int m_sizeMean; // pixels
        private int m_sizeStdDev;   // pixels
        private float m_speedMean;  // pixels per millisecond
        private float m_speedStDev; // pixles per millisecond
        private float m_lifetimeMean; // milliseconds
        private float m_lifetimeStdDev; // milliseconds
        private float m_angleStdDev; // radians

        public ParticleSystem(int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev, float angleStdDev)
        {

            m_sizeMean = sizeMean;
            m_sizeStdDev = sizeStdDev;
            m_speedMean = speedMean;
            m_speedStDev = speedStdDev;
            m_lifetimeMean = lifetimeMean;
            m_lifetimeStdDev = lifetimeStdDev;
            m_angleStdDev = angleStdDev;
        }

        public void create(Vector2 center, float angle)
        {
            float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
            angle = (float)m_random.nextGaussian( angle ,m_angleStdDev);
            var p = new Particle(
                    center,
                    new Vector2((float) Math.Cos(angle), (float)Math.Sin(angle)),
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev),
                    new Vector2(size, size),
                    new System.TimeSpan(0, 0, 0, 0, (int)(m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev)))); ;
            m_particles.Add(p.name, p);
        }

        public void update(GameTime gameTime)
        {
            // Update existing particles
            List<long> removeMe = new List<long>();
            foreach (Particle p in m_particles.Values)
            {
                if (!p.update(gameTime))
                {
                    removeMe.Add(p.name);
                }
            }

            // Remove dead particles
            foreach (long key in removeMe)
            {
                m_particles.Remove(key);
            }
        }
    }
}