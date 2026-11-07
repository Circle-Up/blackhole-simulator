using Microsoft.Xna.Framework;
using System;

namespace BlackHoleSimulation
{
    /// <summary>
    /// Represents a simple black hole that can exert gravitational forces on particles.
    /// </summary>
    public class BlackHole
    {
        public Vector3 Position;  // World position of the black hole
        public float Mass;        // Gravitational mass of the black hole

        /// <summary>
        /// Create a black hole at a given position with a specified mass.
        /// </summary>
        /// <param name="position">World coordinates</param>
        /// <param name="mass">Gravitational mass</param>
        public BlackHole(Vector3 position, float mass)
        {
            Position = position;
            Mass = mass;
        }

        /// <summary>
        /// Applies a simple Newtonian gravity effect to a particle over a timestep.
        /// Updates the particle's velocity and position.
        /// </summary>
        /// <param name="p">The particle to affect</param>
        /// <param name="dt">Time step (seconds)</param>
        public void ApplyGravity(Particle p, float dt)
        {
            // Direction vector from particle to black hole
            Vector3 dir = Position - p.Position;

            // Distance squared to avoid sqrt; prevents division by zero
            float distSq = dir.LengthSquared();
            distSq = Math.Max(distSq, 0.1f); // clamp to avoid extreme forces

            // Simple inverse-square law: F = GM / r^2 (ignoring G for simplicity)
            float force = Mass / distSq;

            // Convert to acceleration vector
            Vector3 accel = Vector3.Normalize(dir) * force;

            // Update velocity and integrate position
            p.Velocity += accel * dt;
            p.Position += p.Velocity * dt;
        }
    }
}
