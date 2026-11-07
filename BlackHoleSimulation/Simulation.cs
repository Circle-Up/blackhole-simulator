using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlackHoleSimulation;

public class Simulation : Game
{
    // Camera control
    private float _yaw = 0f;
    private float _pitch = 0.4f;
    private float _distance = 25f;
    private MouseState _prevMouse;
    private int _centerX;
    private int _centerY;

    // Graphics
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Matrix _view;
    private Matrix _projection;
    private Vector3 _cameraPosition = new Vector3(0, 10, 20);
    private Vector3 _cameraTarget = new Vector3(0, 2f, 0);

    // Simulation objects
    private AccretionDisk _disk;
    private static readonly Random _rng = new Random();
    private BlackHole _bh;
    private List<Particle> _particles = new();
    private BlackHoleRender _bhRenderer;

    // Black hole sphere mesh
    private VertexPositionColor[] _sphereVertices;
    private int[] _sphereIndices;

    public Simulation()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            GraphicsProfile = GraphicsProfile.HiDef // need depth buffer
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Create black hole at origin
        _bh = new BlackHole(Vector3.Zero, mass: 500f);

        // Spawn random particles in a cube
        for (int i = 0; i < 200; i++)
        {
            _particles.Add(new Particle(
                new Vector3(
                    RandomFloat(-20, 20),
                    RandomFloat(-20, 20),
                    RandomFloat(-20, 20)
                )
            ));
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Perspective projection
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            GraphicsDevice.Viewport.AspectRatio,
            0.1f,
            1000f
        );

        // Center mouse
        _centerX = GraphicsDevice.Viewport.Width / 2;
        _centerY = GraphicsDevice.Viewport.Height / 2;
        Mouse.SetPosition(_centerX, _centerY);
        IsMouseVisible = false;

        // Initialize disk and black hole visuals
        _bhRenderer = new BlackHoleRender(GraphicsDevice);
        _disk = new AccretionDisk(GraphicsDevice);

        // Build black hole sphere mesh
        CreateSphere(radius: 2f, slices: 16, stacks: 16);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var mouse = Mouse.GetState();
        var kb = Keyboard.GetState();

        // Mouse orbiting (hold right button)
        if (mouse.RightButton == ButtonState.Pressed)
        {
            float dx = mouse.X - _prevMouse.X;
            float dy = mouse.Y - _prevMouse.Y;

            _yaw -= dx * 0.01f;
            _pitch -= dy * 0.01f;
            _pitch = MathHelper.Clamp(_pitch, -1.3f, 1.3f);
        }

        // Mouse scroll zoom
        if (mouse.ScrollWheelValue != _prevMouse.ScrollWheelValue)
        {
            int delta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
            _distance -= delta * 0.01f;
            _distance = MathHelper.Clamp(_distance, 3f, 200f);
        }

        // Keyboard controls
        if (kb.IsKeyDown(Keys.A)) _yaw += 1f * dt;
        if (kb.IsKeyDown(Keys.D)) _yaw -= 1f * dt;
        if (kb.IsKeyDown(Keys.Q)) _pitch += 1f * dt;
        if (kb.IsKeyDown(Keys.E)) _pitch -= 1f * dt;
        if (kb.IsKeyDown(Keys.W)) _distance -= 10f * dt;
        if (kb.IsKeyDown(Keys.S)) _distance += 10f * dt;

        _distance = MathHelper.Clamp(_distance, 3f, 200f);
        _pitch = MathHelper.Clamp(_pitch, -1.3f, 1.3f);

        // Convert spherical coords to Cartesian
        var cameraOffset = new Vector3(
            (float)(Math.Cos(_pitch) * Math.Sin(_yaw)),
            (float)(Math.Sin(_pitch)),
            (float)(Math.Cos(_pitch) * Math.Cos(_yaw))
        );

        _cameraPosition = _cameraTarget + cameraOffset * _distance;
        _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.Up);
        _prevMouse = mouse;

        // Update disk
        _disk?.Update(dt);

        // Update particles
        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            p.Update(dt);
            _bh.ApplyGravity(p, dt);

            // Respawn particles consumed by BH
            if (p.IsDead || p.Position.Length() < 1.8f)
            {
                p.Respawn(new Vector3(
                    RandomFloat(-20, 20),
                    RandomFloat(-20, 20),
                    RandomFloat(-20, 20)
                ));
            }
        }

        base.Update(gameTime);
    }

    private void CreateSphere(float radius, int slices, int stacks)
    {
        var vertices = new List<VertexPositionColor>();
        var indices = new List<int>();

        // Build vertices
        for (int stack = 0; stack <= stacks; stack++)
        {
            float phi = MathHelper.Pi * stack / stacks;
            for (int slice = 0; slice <= slices; slice++)
            {
                float theta = MathHelper.TwoPi * slice / slices;
                float x = radius * (float)(Math.Sin(phi) * Math.Cos(theta));
                float y = radius * (float)Math.Cos(phi);
                float z = radius * (float)(Math.Sin(phi) * Math.Sin(theta));
                vertices.Add(new VertexPositionColor(new Vector3(x, y, z), Color.Black));
            }
        }

        // Build indices (triangles)
        for (int stack = 0; stack < stacks; stack++)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                int first = (stack * (slices + 1)) + slice;
                int second = first + slices + 1;

                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
            }
        }

        _sphereVertices = vertices.ToArray();
        _sphereIndices = indices.ToArray();
    }

    private void DrawGrid()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        BasicEffect effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = _view,
            Projection = _projection
        };

        var verts = new List<VertexPositionColor>();
        int size = 30;

        // XZ grid
        for (int i = -size; i <= size; i++)
        {
            verts.Add(new VertexPositionColor(new Vector3(i, -10, -size), Color.Gray));
            verts.Add(new VertexPositionColor(new Vector3(i, -10, size), Color.Gray));
            verts.Add(new VertexPositionColor(new Vector3(-size, -10, i), Color.Gray));
            verts.Add(new VertexPositionColor(new Vector3(size, -10, i), Color.Gray));
        }

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts.ToArray(), 0, verts.Count / 2);
        }
    }

    private void DrawParticles()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default; // enable depth
        BasicEffect effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = _view,
            Projection = _projection
        };

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            foreach (var p in _particles)
            {
                var v = new VertexPositionColor[] { new VertexPositionColor(p.Position, Color.White) };
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, v, 0, 1);
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear screen
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

        DrawGrid();
        DrawParticles();

        // Draw disk
        _disk?.Draw(_view, _projection, _bh.Position);

        // Draw black hole sphere
        if (_sphereVertices != null && _sphereIndices != null)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            BasicEffect effect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = _view,
                Projection = _projection
            };

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _sphereVertices,
                    0,
                    _sphereVertices.Length,
                    _sphereIndices,
                    0,
                    _sphereIndices.Length / 3
                );
            }
        }

        // Draw black hole renderer (visual effects)
        _bhRenderer?.Draw(_view, _projection, _bh.Position);

        base.Draw(gameTime);
    }

    private float RandomFloat(float min, float max) => (float)(_rng.NextDouble() * (max - min) + min);
}
