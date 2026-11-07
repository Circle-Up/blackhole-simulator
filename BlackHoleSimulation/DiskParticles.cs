using Microsoft.Xna.Framework;
using System;

namespace BlackHoleSimulation
{
    /// <summary>
    /// Represents a single particle orbiting in the accretion disk.
    /// Particles have a radius, angle, height, and rotation speed.
    /// </summary>
    public class DiskParticle
    {
        private static readonly Random _rng = new(); // Random number generator for initializing particle properties

        // Distance from the black hole center
        public float Radius;

        // Current angular position around the black hole
        public float Angle;

        // Vertical offset from the disk plane
        public float Height;

        // How fast this particle rotates around the black hole
        public float Speed;

        /// <summary>
        /// Create a new disk particle at a random position and speed.
        /// </summary>
        /// <param name="inner">Minimum radius from black hole</param>
        /// <param name="outer">Maximum radius from black hole</param>
        public DiskParticle(float inner, float outer)
        {
            // Pick a random radius within the allowed disk bounds
            Radius = MathHelper.Lerp(inner, outer, (float)_rng.NextDouble());

            // Random starting angle around the disk
            Angle = MathHelper.TwoPi * (float)_rng.NextDouble();

            // Slight vertical variation to make the disk look volumetric
            Height = MathHelper.Lerp(-0.3f, 0.3f, (float)_rng.NextDouble());

            // Random orbital speed (radians per second)
            Speed = MathHelper.Lerp(0.3f, 1.2f, (float)_rng.NextDouble());
        }

        /// <summary>
        /// Update the particle's angle based on its speed and elapsed time.
        /// </summary>
        /// <param name="dt">Time delta in seconds</param>
        public void Update(float dt)
        {
            Angle += Speed * dt;
        }

        /// <summary>
        /// Compute the particle's current 3D position in world space.
        /// </summary>
        public Vector3 Position =>
            new Vector3(
                Radius * MathF.Cos(Angle), // X coordinate
                Height,                     // Y coordinate (vertical offset)
                Radius * MathF.Sin(Angle)  // Z coordinate
            );
    }
}
