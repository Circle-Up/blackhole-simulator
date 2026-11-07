using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BlackHoleSimulation
{
    /// <summary>
    /// Handles rendering a simple black hole sphere.
    /// Uses a VertexBuffer and BasicEffect for efficient GPU drawing.
    /// </summary>
    public class BlackHoleRender
    {
        private GraphicsDevice _graphics;       // Reference to the graphics device
        private BasicEffect _effect;            // Basic shader for colored vertices
        private VertexBuffer _vertexBuffer;     // Vertex buffer containing sphere vertices
        private int _segments = 20;             // Number of segments along latitude/longitude
        private float _radius = 2f;             // Radius of the black hole sphere

        /// <summary>
        /// Create a new renderer for a black hole sphere.
        /// </summary>
        /// <param name="graphics">The GraphicsDevice to render on</param>
        public BlackHoleRender(GraphicsDevice graphics)
        {
            _graphics = graphics;

            // Simple color shader
            _effect = new BasicEffect(graphics)
            {
                VertexColorEnabled = true
            };

            // Generate vertex buffer for a sphere
            CreateSphere();
        }

        /// <summary>
        /// Generate a sphere's vertices in a latitude-longitude pattern
        /// and store them in a vertex buffer.
        /// </summary>
        private void CreateSphere()
        {
            var vertices = new System.Collections.Generic.List<VertexPositionColor>();

            // Iterate over latitude lines
            for (int i = 0; i <= _segments; i++)
            {
                float lat = MathHelper.PiOver2 - i * MathHelper.Pi / _segments; // latitude angle from top
                float y = _radius * (float)Math.Sin(lat);                        // vertical position
                float r = _radius * (float)Math.Cos(lat);                        // horizontal radius at this latitude

                // Iterate over longitude lines
                for (int j = 0; j <= _segments; j++)
                {
                    float lon = j * MathHelper.TwoPi / _segments;               // longitude angle
                    float x = r * (float)Math.Cos(lon);
                    float z = r * (float)Math.Sin(lon);

                    vertices.Add(new VertexPositionColor(new Vector3(x, y, z), Color.Black));
                }
            }

            // Create GPU vertex buffer and upload vertices
            _vertexBuffer = new VertexBuffer(_graphics,
                typeof(VertexPositionColor),
                vertices.Count,
                BufferUsage.WriteOnly);

            _vertexBuffer.SetData(vertices.ToArray());
        }

        /// <summary>
        /// Draw the black hole at the specified world position.
        /// </summary>
        /// <param name="view">Camera view matrix</param>
        /// <param name="projection">Projection matrix</param>
        /// <param name="position">World position of the black hole</param>
        public void Draw(Matrix view, Matrix projection, Vector3 position)
        {
            _effect.View = view;
            _effect.Projection = projection;
            _effect.World = Matrix.CreateTranslation(position);

            // Apply shader and draw vertex buffer as triangle strip
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphics.SetVertexBuffer(_vertexBuffer);

                // TriangleStrip with vertex count / 2 triangles
                _graphics.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vertexBuffer.VertexCount / 2);
            }
        }
    }
}
