using JoltPhysicsSharp;
using OpenTK.Mathematics;
using GameEngine.World;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameEngine.Physics
{
    public class Rigidbody
    {
        public enum BodyType
        {
            Box,
            Sphere,
            Floor
        }
        public JoltPhysics physics;
        public System.Numerics.Vector3 position;
        public System.Numerics.Quaternion rotation;
        public System.Numerics.Vector3 scale;

        private BodyType bodyType;
        public Body body = null!;
        public BodyID bodyID;
        public bool isStatic;

        public float speed = 8f;
        private float moveSpeed;

        private bool initialized;

        public Rigidbody(JoltPhysics physics, BodyType bodyType, bool isStatic)
        {
            this.physics = physics;
            this.bodyType = bodyType;
            this.isStatic = isStatic;
        }

        public void Initialize(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = new System.Numerics.Vector3(position.X, position.Y, position.Z);
            this.rotation = new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            this.scale = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);

            switch (bodyType)
            {
                case BodyType.Box:
                    body = CreateBoxRigidbody();
                    break;
                case BodyType.Sphere:
                    body = CreateSphereRigidbody();
                    break;
                case BodyType.Floor:
                    body = Game.physics.CreateFloor(scale.Length * 0.5f, JoltPhysics.Layers.NonMoving);
                    break;
            }

            initialized = true;
        }

        public void UpdateTransform()
        {
            if (!initialized)
                return;

            var transform = physics.BodyInterface.GetTransformedShape(physics.BodyLockInterface, bodyID);

            position = transform.ShapePositionCOM;
            rotation = transform.ShapeRotation;
        }

        private Body CreateBoxRigidbody()
        {
            Body box =  physics.CreateBox(
                scale * 0.5f,
                position,
                rotation,
                isStatic ? MotionType.Static : MotionType.Dynamic,
                isStatic ? JoltPhysics.Layers.NonMoving : JoltPhysics.Layers.Moving,
                Activation.Activate
            );
            bodyID = box.ID;
            return box;
        }

        private Body CreateSphereRigidbody()
        {
            Body sphere = physics.CreateSphere(
                0.7f,
                position,
                rotation,
                isStatic ? MotionType.Static : MotionType.Dynamic,
                isStatic ? JoltPhysics.Layers.NonMoving : JoltPhysics.Layers.Moving,
                Activation.Activate
            );
            bodyID = sphere.ID;
            return sphere;
        }

        public void Move(KeyboardState keyboardInput, float deltaTime)
        {
            if (!initialized)
                return;

            moveSpeed = speed * deltaTime;

            Console.WriteLine(moveSpeed);

            System.Numerics.Vector3 force = System.Numerics.Vector3.Zero;

            //if (keyboardInput.IsKeyDown(Keys.RightControl)) { moveSpeed *= 1.25f; }

            if (keyboardInput.IsKeyDown(Keys.Up)) { force.Z += moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Left)) { force.X -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Down)) { force.Z -= moveSpeed; }
            if (keyboardInput.IsKeyDown(Keys.Right)) { force.X += moveSpeed; }

            body.AddForce(force);
        }
    }
}