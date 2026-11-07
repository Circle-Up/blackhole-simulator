using Microsoft.Xna.Framework;
using System;

namespace BlackHoleSimulation
{
    public class Particle
    {
        private static readonly Random _rng = new Random();

        public Vector3 Position;
        public Vector3 Velocity;

        private float _life;
        private float _maxLife;

        public bool IsDead => _life <= 0f;

        public Particle(Vector3 position)
        {
            Respawn(position);
        }

        public void Update(float dt)
        {
            _life -= dt;
        }

        public void Respawn(Vector3 position)
        {
            Position = position;
            Velocity = Vector3.Zero;

            _maxLife = 8f + (float)_rng.NextDouble() * 6f; // 8–14 seconds
            _life = _maxLife;
        }

        public float LifeRatio => _life / _maxLife;
    }
}
