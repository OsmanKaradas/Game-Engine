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
        private GameObject owner;
        public BodyHandle body;
        public StaticHandle? staticBody;

        public Box collider;
        public RigidPose pose;
        public BodyInertia mass;

        public BodyVelocity velocity;

        public bool isStatic;

        public BepuRigidbody(Simulation simulation, GameObject owner, System.Numerics.Vector3 position, float mass, bool isStatic)
        {
            this.owner = owner;
            this.simulation = simulation;
            this.pose = new RigidPose(position);
            this.collider = new Box(owner.scale.X * 0.5f, owner.scale.Y * 0.5f, owner.scale.Z * 0.5f);
            this.mass = collider.ComputeInertia(mass);
            velocity = new BodyVelocity();
            this.isStatic = isStatic;

            InitializeBody();
        }

        private void InitializeBody()
        {
            if (!isStatic)
            {
                body = simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, mass, simulation.Shapes.Add(collider), 0.01f));
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
                return new Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return new Quaternion(pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z);
            }
        } 
    }
}