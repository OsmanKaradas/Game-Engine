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
            Capsule,
            Floor
        }

        private BodyType bodyType;
        private MotionType motionType;

        public JoltPhysics physics;
        private System.Numerics.Vector3 position;
        private System.Numerics.Quaternion rotation;
        private System.Numerics.Vector3 scale;

        public Body body = null!;

        private bool initialized;

        public Rigidbody(JoltPhysics physics, BodyType bodyType, MotionType motionType)
        {
            this.physics = physics;
            this.bodyType = bodyType;
            this.motionType = motionType;
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
                    body = CreateSphereRigidbody(0.7f);
                    break;
                case BodyType.Capsule:
                    body = CreateCapsuleRigidbody(0.65f, 0.6f);
                    break;
                case BodyType.Floor:
                    body = Game.physics.CreateFloor(scale.Length * 0.5f, JoltPhysics.Layers.NonMoving);
                    break;
            }

            if (motionType != MotionType.Static)
                body.MotionProperties.ScaleToMass(1f);

            initialized = true;
        }

        private Body CreateBoxRigidbody()
        {
            Body box = physics.CreateBox(
                scale * 0.5f,
                position,
                rotation,
                motionType,
                motionType == MotionType.Static ? JoltPhysics.Layers.NonMoving : JoltPhysics.Layers.Moving,
                Activation.Activate
            );

            return box;
        }

        private Body CreateSphereRigidbody(float radius)
        {
            Body sphere = physics.CreateSphere(
                radius,
                position,
                rotation,
                motionType,
                motionType == MotionType.Static ? JoltPhysics.Layers.NonMoving : JoltPhysics.Layers.Moving,
                Activation.Activate
            );
            return sphere;
        }

        private Body CreateCapsuleRigidbody(float height, float radius)
        {
            CapsuleShape capsuleShape = new(new(height * 2f, radius * 2f));
            Body capsule = physics.BodyInterface.CreateBody(new BodyCreationSettings(capsuleShape, position, rotation, motionType, motionType == MotionType.Static ? JoltPhysics.Layers.NonMoving : JoltPhysics.Layers.Moving));
            physics.BodyInterface.AddBody(capsule.ID, Activation.Activate);
            return capsule;
        }
    }
}