using OpenTK.Mathematics;
using GameEngine.World;
using System.Collections.Generic;

namespace GameEngine.Physics
{
    public class RigidbodyPrototype
    {
        public GameObject owner;
        public Vector3 velocity;
        public float mass = 1f;
        public bool isStatic = false;
        public bool collided;
        private readonly Vector3 gravityForce = new Vector3(0, -9.81f, 0);
        private Vector3 initialColor;

        public RigidbodyPrototype(GameObject owner, bool isStatic)
        {
            this.owner = owner;
            this.isStatic = isStatic;
            velocity = Vector3.Zero;
            initialColor = owner.color;
        }

        public void Update(float deltaTime, List<RigidbodyPrototype> others)
        {
            owner.UpdateBounds();
            if (isStatic) return;

            // Apply gravity
            velocity += gravityForce / mass * deltaTime;

            // Apply velocity
            owner.position += velocity * deltaTime;

            foreach(var other in others)
                CheckCollision(other);
        }

        private bool CheckCollision(RigidbodyPrototype other)
        {
            if (SAT.CheckOBBCollision(owner.obbBounds, other.owner.obbBounds, out Vector3 mtv))
            {
                collided = true;
                owner.position -= mtv;

                Vector3 mtvNormal = mtv.Normalized();
                float velAlongNormal = Vector3.Dot(velocity, mtvNormal);
                if (velAlongNormal > 0)
                    velocity -= velAlongNormal * mtvNormal;

                if (!other.isStatic)
                    owner.color = new Vector3(1f, 0f, 0f);
                return true;
            }
            owner.color = initialColor;
            collided = false;
            return false;
        }
    }
}