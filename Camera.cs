using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace GameEngine
{
    public class Camera
    {
        public bool cameraMode = true;
        private float SPEED = 8f;
        private float SCREENWIDTH;
        private float SCREENHEIGHT;
        private float SENSITIVITY = 0.1f;
        private float DEPTH_OF_FIELD = 45f;

        // position vars
        public Vector3 position;
        Vector3 up = Vector3.UnitY;
        Vector3 front = -Vector3.UnitZ;
        Vector3 right = Vector3.UnitX;

        // view rotations
        private float pitch;
        private float yaw = -90.0f;

        public Camera(float width, float height, Vector3 position)
        {
            SCREENWIDTH = width;
            SCREENHEIGHT = height;
            this.position = position;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + front, up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(DEPTH_OF_FIELD), SCREENWIDTH / SCREENHEIGHT, 0.1f, 100f);
        }

        private void UpdateVectors()
        {
            // up
            if (pitch > 89.0f) pitch = 89.0f;
            // down
            if (pitch < -89.0f) pitch = -89.0f;

            front.X = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Sin(MathHelper.DegreesToRadians(yaw));

            front = Vector3.Normalize(front);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
        public void InputController(KeyboardState input, MouseState mouse, FrameEventArgs e)
        {
            float deltaTime = (float)e.Time;
            float velocity = SPEED * deltaTime;

            if (cameraMode)
            {
                // Sprinting
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
            }

            if (input.IsKeyPressed(Keys.F))
                cameraMode = !cameraMode;

            yaw += mouse.Delta.X * SENSITIVITY;
            pitch -= mouse.Delta.Y * SENSITIVITY;

            UpdateVectors();
        }

        public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e)
        {
            InputController(input, mouse, e);
        }
    }
}