using JoltPhysicsSharp;
using OpenTK.Mathematics;
using GameEngine.World;

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
        public System.Numerics.Vector3 position;
        public System.Numerics.Quaternion rotation;
        public System.Numerics.Vector3 scale;

        private BodyType bodyType;
        public Body? body;
        public BodyID bodyID;
        public bool isStatic;

        private bool isSetup;
        public Rigidbody(BodyType bodyType, bool isStatic)
        {
            this.bodyType = bodyType;
            this.isStatic = isStatic;
        }

        public void Setup(Vector3 position, Quaternion rotation, Vector3 scale)
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

            isSetup = true;
        }

        public void Update()
        {
            if (!isSetup)
                return;

            var transform = Game.physics.BodyInterface.GetTransformedShape(Game.physics.BodyLockInterface, bodyID);

            position = transform.ShapePositionCOM;
            rotation = transform.ShapeRotation;
        }

        public Body CreateBoxRigidbody()
        {
            Body box = Game.physics.CreateBox(
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

        public Body CreateSphereRigidbody()
        {
            Body sphere = Game.physics.CreateSphere(
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
    }
}