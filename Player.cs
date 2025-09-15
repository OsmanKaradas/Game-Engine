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
        public float speed = 6f;

        public Player(GameObject gameObject, Shape shape, float mass, Camera camera, JoltPhysics physics)
        {
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

            this.gameObject = gameObject;
            this.camera = camera;
            this.physics = physics;
        }

        public void Update(KeyboardState keyboard)
        {
            Move(keyboard);
            camera.position = gameObject.position + new Vector3(-1f, 3f, -5f);
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

            if (keyboard.IsKeyDown(Keys.LeftShift)) { moveSpeed *= 2f; }
            if (keyboard.IsKeyDown(Keys.W)) { moveDir += camera.front; }
            if (keyboard.IsKeyDown(Keys.A)) { moveDir += -camera.right; }
            if (keyboard.IsKeyDown(Keys.S)) { moveDir += -camera.front; }
            if (keyboard.IsKeyDown(Keys.D)) { moveDir += camera.right; }

            moveDir.Y = 0f;
            moveDir.NormalizeFast();
            velocity = moveDir * moveSpeed;
            velocity.Y = -9.81f;

            character.LinearVelocity = new(velocity.X, velocity.Y, velocity.Z);
            gameObject.position = new(character.Position.X, character.Position.Y, character.Position.Z);
        }
    }
}