using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlackHoleSimulation
{
    public class Camera
    {
        public Vector3 Position;
        public float Pitch;
        public float Yaw;

        public Matrix ViewMatrix => 
            Matrix.CreateLookAt(Position, Position + Forward, Vector3.Up);

        public Matrix ProjectionMatrix;

        public Vector3 Forward =>
            Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0));

        public Vector3 Right =>
            Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0));

        public Camera(GraphicsDevice gd)
        {
            Position = new Vector3(0, 5, 10);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                gd.Viewport.AspectRatio,
                0.1f,
                1000f
            );
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Keyboard movement
            var k = Keyboard.GetState();
            float speed = 10f;

            if (k.IsKeyDown(Keys.W)) Position += Forward * speed * dt;
            if (k.IsKeyDown(Keys.S)) Position -= Forward * speed * dt;
            if (k.IsKeyDown(Keys.A)) Position -= Right * speed * dt;
            if (k.IsKeyDown(Keys.D)) Position += Right * speed * dt;

            // Mouse look
            var m = Mouse.GetState();
            if (m.RightButton == ButtonState.Pressed)
            {
                float sensitivity = 0.003f;

                int dx = m.X - (ViewportWidth / 2);
                int dy = m.Y - (ViewportHeight / 2);

                Yaw -= dx * sensitivity;
                Pitch -= dy * sensitivity;

                Mouse.SetPosition(ViewportWidth / 2, ViewportHeight / 2);
            }
        }

        public static int ViewportWidth;
        public static int ViewportHeight;
    }
}
