using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine.World;
using OpenTK.Graphics.OpenGL4;

namespace GameEngine.Physics
{
    public class Movement
    {
        private Rigidbody rigidbody = null!;
        public float speed;
        private float moveSpeed;

        private System.Numerics.Vector3 force;
        private System.Numerics.Vector3 velocity;

        private bool initialized = false;
        public Movement(float speed)
        {
            this.speed = speed;
        }

        public void Initialize(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;

            velocity = rigidbody.body.GetLinearVelocity();
            
            initialized = true;
        }

        public void Move(KeyboardState keyboardInput)
        {
            if (!initialized)
                return;

            moveSpeed = speed;
            force = velocity * moveSpeed;

            if (keyboardInput.IsKeyDown(Keys.LeftControl)) { moveSpeed *= 1.25f; }

            if (keyboardInput.IsKeyDown(Keys.W)) { force.Z += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.A)) { force.X -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.S)) { force.Z -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.D)) { force.X += moveSpeed; }

            rigidbody.body.AddForce(force);
        }
    }
}