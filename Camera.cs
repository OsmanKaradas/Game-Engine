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
        public float speed = 8f;
        public float SCREENWIDTH;
        public float SCREENHEIGHT;
        private float SENSITIVITY = 40f;
        private float FOV = 45f;

        // position vars
        public Vector3 position;
        public Vector3 up = new Vector3(0f, 1f, 0f);
        public Vector3 front = new Vector3(0f, 0f, -1f);
        public Vector3 right = new Vector3(1f, 0f, 0f);

        // view rotations
        public float pitch = 0f;
        public float yaw = -90.0f;

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
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), SCREENWIDTH / SCREENHEIGHT, 0.1f, 100f);
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
            float velocity = speed * deltaTime;

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

            yaw += mouse.Delta.X * SENSITIVITY * deltaTime;
            pitch -= mouse.Delta.Y * SENSITIVITY * deltaTime;

            UpdateVectors();
        }

        public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e)
        {
            InputController(input, mouse, e);
        }
    }
}