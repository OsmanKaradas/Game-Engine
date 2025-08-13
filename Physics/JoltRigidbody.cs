using JoltPhysicsSharp;
using OpenTK.Mathematics;
using GameEngine.World;

namespace GameEngine.Physics
{
    public class JoltRigidbody
    {
        public Body body;
        public BodyID bodyID;
        public bool isStatic;

        public JoltRigidbody(GameObject owner, bool isStatic)
        {
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

        public Vector3 Position
        {
            get
            {
                return new Vector3(Game.physicsSample.BodyInterface.GetPosition(body.ID).X, Game.physicsSample.BodyInterface.GetPosition(body.ID).Y, Game.physicsSample.BodyInterface.GetPosition(body.ID).Z);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return new Quaternion(Game.physicsSample.BodyInterface.GetRotation(body.ID).X, Game.physicsSample.BodyInterface.GetRotation(body.ID).Y, Game.physicsSample.BodyInterface.GetRotation(body.ID).Z);
            }
        }
    }
}