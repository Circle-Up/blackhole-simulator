using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace BlackHoleSimulation;

// ----------------------------------------------------
// AccretionDisk class
// ----------------------------------------------------
// Represents a 3D black hole accretion disk with both
// static disk geometry and dynamic particle motion.
// Includes optional fake gravitational lensing effects
// for visual distortion near the black hole.
public class AccretionDisk
{
    // Graphics device used for rendering
    private GraphicsDevice _device;

    // Vertex and index data for the disk mesh
    private VertexPositionColor[] _vertices;
    private int[] _indices;

    // Rotation of the disk over time (for animation)
    private float _rotation = 0f;

    // Inner and outer radii of the accretion disk
    private float _innerRadius = 0f;
    private float _outerRadius = 6f;

    // Moving particles on the disk (simulating sparks, gas, or debris)
    private List<DiskParticle> _diskParticles = new();

    // ----------------------------------------------------
    // Constructor
    // ----------------------------------------------------
    public AccretionDisk(GraphicsDevice device)
    {
        _device = device;

        // Initialize disk particles with radius between 2.5 and 7.5
        for (int i = 0; i < 250; i++)
            _diskParticles.Add(new DiskParticle(2.5f, 7.5f));

        // Generate the main disk mesh
        CreateDisk(_innerRadius, _outerRadius, 40, 96);
    }

    // ----------------------------------------------------
    // Update
    // ----------------------------------------------------
    // Updates the disk rotation and particle positions
    public void Update(float dt)
    {
        // Rotate the disk at a fixed rate
        _rotation += dt * 0.8f;

        // Update each particle (position, orbit, etc.)
        foreach (var dp in _diskParticles)
            dp.Update(dt);
    }

    // ----------------------------------------------------
    // Disk geometry generation
    // ----------------------------------------------------
    private void CreateDisk(float innerRadius, float outerRadius, int rings, int slices)
    {
        List<VertexPositionColor> verts = new();
        List<int> inds = new();

        // Colors for inner → outer disk gradient
        Color outerColor = new Color(139, 0, 0);       // dark red-orange
        Color innerColor = new Color(255, 245, 230);   // warm white

        float alpha = 1f; // overall disk opacity

        // Generate vertices ring by ring
        for (int r = 0; r <= rings; r++)
        {
            float t = r / (float)rings;                   // normalized radius 0–1
            float radius = MathHelper.Lerp(innerRadius, outerRadius, t);

            float heat = 1f - t;                          // inner brighter than outer
            heat = heat * heat;                            // smooth falloff
            Color baseColor = Color.Lerp(outerColor, innerColor, heat);

            Color finalColor = new Color(
                baseColor.R,
                baseColor.G,
                baseColor.B,
                (byte)(alpha * 255)
            );

            // Generate vertices around each ring (slices)
            for (int slice = 0; slice <= slices; slice++)
            {
                float angle = MathHelper.TwoPi * slice / slices;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);

                verts.Add(new VertexPositionColor(new Vector3(x, 0f, z), finalColor));
            }
        }

        int rowVerts = slices + 1;

        // Generate triangles for each quad in the disk mesh
        for (int ring = 0; ring < rings; ring++)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                // Indices of the four corners of the current quad
                int bottomLeft  = ring * rowVerts + slice;      
                int bottomRight = bottomLeft + 1;               
                int topLeft     = bottomLeft + rowVerts;        
                int topRight    = topLeft + 1;                  

                // First triangle
                inds.Add(bottomLeft);
                inds.Add(topLeft);
                inds.Add(bottomRight);

                // Second triangle
                inds.Add(bottomRight);
                inds.Add(topLeft);
                inds.Add(topRight);
            }
        }

        // Convert to arrays for rendering
        _vertices = verts.ToArray();
        _indices  = inds.ToArray();
    }

    // ----------------------------------------------------
    // Rendering
    // ----------------------------------------------------
    public void Draw(Matrix view, Matrix projection, Vector3 center)
    {
        BasicEffect effect = new BasicEffect(_device)
        {
            VertexColorEnabled = true,
            View = view,
            Projection = projection,
            World = Matrix.CreateRotationY(_rotation) * Matrix.CreateTranslation(center)
        };

        _device.RasterizerState = RasterizerState.CullNone;
        _device.BlendState = BlendState.Additive;

        _device.DepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false
        };

        // Draw the base disk mesh
        DrawDisk(effect, _vertices);

        // Draw lensed/funnel overlays above and below the disk
        DrawLensedCopy(effect, center, +2);
        DrawLensedCopy(effect, center, -2);

        // Draw moving disk particles
        DrawDiskParticles(view, projection, center);

        // Reset states after rendering
        _device.RasterizerState = RasterizerState.CullCounterClockwise;
        _device.BlendState = BlendState.Opaque;
        _device.DepthStencilState = DepthStencilState.Default;
    }

    // Draw a set of vertices with indices using the given effect
    private void DrawDisk(BasicEffect effect, VertexPositionColor[] verts)
    {
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                verts,
                0,
                verts.Length,
                _indices,
                0,
                _indices.Length / 3
            );
        }
    }

    // ----------------------------------------------------
    // Draw disk particles
    // ----------------------------------------------------
    private void DrawDiskParticles(Matrix view, Matrix projection, Vector3 center)
    {
        BasicEffect effect = new BasicEffect(_device)
        {
            VertexColorEnabled = true,
            View = view,
            Projection = projection,
            World = Matrix.CreateRotationY(_rotation) * Matrix.CreateTranslation(center)
        };

        // Additive blending for glowing particle effect
        _device.BlendState = BlendState.Additive;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            foreach (var e in _diskParticles)
            {
                // Particle color interpolates from orange to white based on radius
                Color c = Color.Lerp(
                    new Color(255, 140, 40),
                    Color.White,
                    1f - (e.Radius - 2.5f) / 5f
                );

                // Draw each particle as a point primitive
                _device.DrawUserPrimitives(
                    PrimitiveType.PointList,
                    new[] { new VertexPositionColor(e.Position, c) },
                    0,
                    1
                );
            }
        }
    }

    // ----------------------------------------------------
    // Fake gravitational lensing / dome overlays
    // ----------------------------------------------------
    private void DrawLensedCopy(BasicEffect baseEffect, XnaVector3 center, int direction)
    {
        // Funnel parameters
        float maxRadiusFunnel = _outerRadius * 3;   // how far funnel extends
        float domeRadius      = _outerRadius * 0.7f; // radius for dome overlay
        float domeHeightMax   = 0.28f * domeRadius;  // max dome height

        float liftStrength = 2.2f;  // funnel vertical scale
        float pullStrength = 0.55f; // funnel inward curvature
        float opacity      = 0.85f; // transparency

        VertexPositionColor[] warpedFunnel = new VertexPositionColor[_vertices.Length];
        VertexPositionColor[] warpedDome   = new VertexPositionColor[_vertices.Length];

        for (int i = 0; i < _vertices.Length; i++)
        {
            var v = _vertices[i];
            XnaVector3 p = v.Position;

            float r = new Vector2(p.X, p.Z).Length();

            // -------------------
            // Funnel: inward curvature / upward "funnel"
            // -------------------
            float tFunnel = MathHelper.Clamp(1f - (r / maxRadiusFunnel), 0f, 1f);
            float funnelHeight = (float)Math.Sqrt(Math.Max(0f, 100f - (tFunnel * 100f)));
            float lensFunnel = MathHelper.SmoothStep(0f, 1f, tFunnel);
            float inwardScale = 1f - lensFunnel * pullStrength;

            warpedFunnel[i] = new VertexPositionColor(
                new XnaVector3(
                    p.X * inwardScale,
                    funnelHeight * liftStrength * direction,
                    p.Z * inwardScale
                ),
                new Color(v.Color.R, v.Color.G, v.Color.B, (byte)(opacity * 255))
            );

            // -------------------
            // Dome: shallow half-dome over inner disk
            // -------------------
            float domeHeight = 0f;

            if (r < domeRadius)
            {
                float t = r / domeRadius; // 0=center, 1=edge
                domeHeight = domeHeightMax * (float)Math.Sqrt(1f - t * t); // shallow spherical cap
            }

            warpedDome[i] = new VertexPositionColor(
                new XnaVector3(
                    p.X * inwardScale,
                    domeHeight * direction,
                    p.Z * inwardScale
                ),
                new Color(v.Color.R, v.Color.G, v.Color.B, (byte)(opacity * 255))
            );
        }

        // Apply rotation + translation
        baseEffect.World = Matrix.CreateRotationY(_rotation) * Matrix.CreateTranslation(center);

        // Draw funnel and dome overlays separately
        DrawDisk(baseEffect, warpedFunnel);
        DrawDisk(baseEffect, warpedDome);
    }
}
