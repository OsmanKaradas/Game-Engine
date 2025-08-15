using JoltPhysicsSharp;
using OpenTK.Mathematics;
using GameEngine.World;

namespace GameEngine.Physics
{
    public class JoltRigidbody
    {
        GameObject owner;
        public Body body;
        public BodyID bodyID;
        public BodyInterface bodyInterface;
        public bool isStatic;

        public JoltRigidbody(GameObject owner, bool isStatic)
        {
            this.owner = owner;
            this.isStatic = isStatic;
            if (!isStatic)
            {
                body = Game.physicsSample.CreateBox(
                    new System.Numerics.Vector3(owner.scale.X * 0.5f, owner.scale.Y * 0.5f, owner.scale.Z * 0.5f),
                    new System.Numerics.Vector3(owner.position.X, owner.position.Y, owner.position.Z),
                    new System.Numerics.Quaternion(new System.Numerics.Vector3(owner.rotation.X, owner.rotation.Y, owner.rotation.Z), 1f),
                    MotionType.Dynamic,
                    JoltPhysicsSample.Layers.Moving,
                    Activation.Activate
                );
            }
            else
            {
                body = Game.physicsSample.CreateBox(
                    new System.Numerics.Vector3(owner.scale.X * 0.5f, owner.scale.Y * 0.5f, owner.scale.Z * 0.5f),
                    new System.Numerics.Vector3(owner.position.X, owner.position.Y, owner.position.Z),
                    new System.Numerics.Quaternion(new System.Numerics.Vector3(owner.rotation.X, owner.rotation.Y, owner.rotation.Z), 1f),
                    MotionType.Static,
                    JoltPhysicsSample.Layers.NonMoving,
                    Activation.Activate
                );
            }

            bodyID = body.ID;
        }

        public void UpdateTransform()
        {
            var transform = Game.physicsSample.BodyInterface.GetTransformedShape(Game.physicsSample.BodyLockInterface, bodyID);

            owner.position = new Vector3(transform.ShapePositionCOM.X, transform.ShapePositionCOM.Y, transform.ShapePositionCOM.Z);
            owner.rotation = new Quaternion(transform.ShapeRotation.X, transform.ShapeRotation.Y, transform.ShapeRotation.Z, transform.ShapeRotation.W);
        }

        public void CreateSphereRigidbody()
        {
            Body sphere = Game.physicsSample.CreateSphere(
                0.7f,
                new System.Numerics.Vector3(owner.position.X, owner.position.Y, owner.position.Z),
                new System.Numerics.Quaternion(owner.rotation.X, owner.rotation.Y, owner.rotation.Z, owner.rotation.W),
                isStatic ? MotionType.Static : MotionType.Dynamic,
                isStatic ? JoltPhysicsSample.Layers.NonMoving : JoltPhysicsSample.Layers.Moving,
                Activation.Activate
            );
        }

        public void CreateBoxRigidbody()
        {
            Body box = Game.physicsSample.CreateBox(
                new System.Numerics.Vector3(owner.scale.X, owner.scale.Y, owner.scale.Z),
                new System.Numerics.Vector3(owner.position.X, owner.position.Y, owner.position.Z),
                new System.Numerics.Quaternion(owner.rotation.X, owner.rotation.Y, owner.rotation.Z, owner.rotation.W),
                isStatic ? MotionType.Static : MotionType.Dynamic,
                isStatic ? JoltPhysicsSample.Layers.NonMoving : JoltPhysicsSample.Layers.Moving,
                Activation.Activate
            );
        }
    }
}