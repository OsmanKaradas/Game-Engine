using GameEngine.Physics;
using GameEngine.World;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using JoltPhysicsSharp;

namespace GameEngine
{
    public class Player
    {
        private CharacterVirtual character;
        private JoltPhysics physics;
        private Camera camera;
        public GameObject gameObject;
        private Vector3 velocity;
        public float speed = 7.5f;

        public Player(GameObject gameObject, Shape shape, float mass, Camera camera, JoltPhysics physics)
        {
            this.gameObject = gameObject;
            this.camera = camera;
            this.physics = physics;

            physics.BodyInterface.RemoveBody(gameObject.rigidbody.body.ID);
            gameObject.rigidbody = null!;

            var characterSettings = new CharacterVirtualSettings
            {
                Shape = shape,
                Mass = mass,
                MaxSlopeAngle = 45.0f * MathF.PI / 180f,
                CharacterPadding = 0.05f,
                CollisionTolerance = 0.1f,
                MaxStrength = 100.0f
            };

            character = new(characterSettings, new(gameObject.position.X, gameObject.position.Y, gameObject.position.Z), new(gameObject.rotation.X, gameObject.rotation.Y, gameObject.rotation.Z, gameObject.rotation.W), 0, physics.System);
        }

        public void Update(KeyboardState keyboard)
        {
            Move(keyboard);
            character.Update(
                Time.deltaTime,
                JoltPhysics.Layers.Moving,
                physics.System
            );
        }
        public void Move(KeyboardState keyboard)
        {
            Vector3 moveDir = Vector3.Zero;
            float moveSpeed = speed;

            if (keyboard.IsKeyDown(Keys.LeftShift)) { moveSpeed *= 1.5f; }
            if (keyboard.IsKeyDown(Keys.W)) { moveDir += camera.front; }
            if (keyboard.IsKeyDown(Keys.A)) { moveDir += -camera.right; }
            if (keyboard.IsKeyDown(Keys.S)) { moveDir += -camera.front; }
            if (keyboard.IsKeyDown(Keys.D)) { moveDir += camera.right; }

            moveDir.NormalizeFast();
            velocity = moveDir * moveSpeed;
            velocity.Y -= 9.81f;

            character.LinearVelocity = new(velocity.X, velocity.Y, velocity.Z);
            gameObject.position = new(character.Position.X, character.Position.Y, character.Position.Z);

            if (moveDir == Vector3.Zero)
                return;

            float angle = MathF.Atan2(moveDir.X, moveDir.Z);

            var targetRot = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitY, angle);
            var rot = System.Numerics.Quaternion.Slerp(character.Rotation, targetRot, 8f * Time.deltaTime);

            character.Rotation = rot;
            gameObject.rotation = new(character.Rotation.X, character.Rotation.Y, character.Rotation.Z, character.Rotation.W);
        }
    }
}