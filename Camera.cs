using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace GameEngine
{
    public class Camera
    {
        public enum Mode
        {
            Default,
            LookAround,
            Locked
        }
        public Mode mode = Mode.Default;

        public float speed = 8f;
        public float screenWidth;
        public float screenHeight;
        public float sensitivity;
        public float FOV = 45f;

        // position vars
        public Vector3 position;
        public Vector3 up = new Vector3(0f, 1f, 0f);
        public Vector3 front = new Vector3(0f, 0f, -1f);
        public Vector3 right = new Vector3(1f, 0f, 0f);

        // view rotations
        public float pitch = 0f;
        public float yaw = -90.0f;

        public Camera(float screenWidth, float screenHeight, Vector3 position, float sensitivity)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.position = position;
            this.sensitivity = sensitivity;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + front, up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), screenWidth / screenHeight, 0.1f, 100f);
        }

        private void UpdateVectors()
        {
            if (mode == Mode.Default)
            {
                // up
                if (pitch > 89.0f) pitch = 89.0f;
                // down
                if (pitch < -89.0f) pitch = -89.0f;
            }
            else if (mode == Mode.LookAround)
            {
                // up
                if (pitch > 10.0f) pitch = 10.0f;
                // down
                if (pitch < -30.0f) pitch = -30.0f;
            }

            front.X = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Sin(MathHelper.DegreesToRadians(yaw));

            front = Vector3.Normalize(front);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
        public void Update(GameWindow window, KeyboardState input, MouseState mouse, FrameEventArgs e)
        {
            UpdateVectors();
            float deltaTime = (float)e.Time;
            float velocity = speed * deltaTime;

            switch (mode)
            {
                case Mode.Default:
                    if (input.IsKeyDown(Keys.LeftShift))
                    {
                        velocity *= 7.5f;
                    }

                    if (input.IsKeyDown(Keys.W))
                    {
                        position += front * velocity;
                    }
                    if (input.IsKeyDown(Keys.A))
                    {
                        position -= right * velocity;
                    }
                    if (input.IsKeyDown(Keys.S))
                    {

                        position -= front * velocity;
                    }
                    if (input.IsKeyDown(Keys.D))
                    {
                        position += right * velocity;
                    }

                    if (input.IsKeyDown(Keys.Space))
                    {
                        position.Y += velocity;
                    }

                    if (input.IsKeyDown(Keys.X))
                    {
                        position.Y -= velocity;
                    }

                    yaw += mouse.Delta.X * sensitivity * deltaTime;
                    pitch -= mouse.Delta.Y * sensitivity * deltaTime;
                    window.CursorState = CursorState.Grabbed;
                    break;

                case Mode.LookAround:
                    yaw += mouse.Delta.X * sensitivity * deltaTime;
                    pitch -= mouse.Delta.Y * sensitivity * deltaTime;
                    window.CursorState = CursorState.Grabbed;
                    break;

                case Mode.Locked:
                    window.CursorState = CursorState.Normal;
                    break;
            }

            if (input.IsKeyDown(Keys.F))
            {
                if (input.IsKeyPressed(Keys.D1)) { mode = Mode.Default; }
                if (input.IsKeyPressed(Keys.D2)) { mode = Mode.LookAround; }
                if (input.IsKeyPressed(Keys.D3)) { mode = Mode.Locked; }
            }

        }

        public void SendRayCastFromScreen(GameWindow window, out Vector3 direction)
        {
            float x = (2f * window.MousePosition.X) / window.ClientSize.X - 1f;
            float y = 1f - (2f * window.MousePosition.Y) / window.ClientSize.Y;
            Vector4 clipCoords = new Vector4(x, y, -1.0f, 1.0f);

            Vector4 eyeCoords = Vector4.TransformRow(clipCoords, GetProjectionMatrix().Inverted());
            eyeCoords.Z = -1.0f;
            eyeCoords.W = 0.0f;

            Vector4 worldRay = Vector4.TransformRow(eyeCoords, GetViewMatrix().Inverted());
            direction = Vector3.Normalize(new Vector3(worldRay.X, worldRay.Y, worldRay.Z));
        }
    }
}