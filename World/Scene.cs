using GameEngine.Physics;
using GameEngine.Graphics;

namespace GameEngine.World
{
    public class Scene
    {
        public List<GameObject> gameObjects;
        public List<ShaderProgram> shaders;
        private JoltPhysics physics;

        public Scene()
        {
            gameObjects = new List<GameObject>();
            shaders = new List<ShaderProgram>();
            physics = new JoltPhysics();
        }
    }
}