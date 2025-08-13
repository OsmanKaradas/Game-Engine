using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using OpenTK.Mathematics;
using GameEngine.World;

namespace GameEngine.Physics
{
    public class BepuRigidbody
    {
        private Simulation simulation;
        public BodyHandle body;
        public StaticHandle staticBody;

        public Box collider;
        public RigidPose pose;
        public BodyInertia mass;

        public bool isStatic;

        public BepuRigidbody(Simulation simulation, GameObject owner, float mass, bool isStatic)
        {
            this.simulation = simulation;
            pose = new RigidPose(
                new System.Numerics.Vector3(owner.position.X, owner.position.Y, owner.position.Z),
                new System.Numerics.Quaternion(owner.rotation.X, owner.rotation.Y, owner.rotation.Z, owner.rotation.W)
                );
            collider = new Box(owner.scale.X, owner.scale.Y, owner.scale.Z);
            this.mass = collider.ComputeInertia(mass);
            this.isStatic = isStatic;

            if (!isStatic)
            {
                body = simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, this.mass, simulation.Shapes.Add(collider), 0.01f));
            }
            else
            {
                staticBody = simulation.Statics.Add(new StaticDescription(pose, simulation.Shapes.Add(collider)));
            }
        }

        public Vector3 Position
        {
            get
            {
                return new Vector3(simulation.Bodies[body].Pose.Position.X, simulation.Bodies[body].Pose.Position.Y, simulation.Bodies[body].Pose.Position.Z);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return new Quaternion(simulation.Bodies[body].Pose.Orientation.X, simulation.Bodies[body].Pose.Orientation.Y, simulation.Bodies[body].Pose.Orientation.Z);
            }
        } 
    }
}