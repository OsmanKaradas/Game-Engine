using GameEngine.Physics;
using JoltPhysicsSharp;

namespace GameEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            ShadowTest game = new(1920, 1080);
            game.Run();
        }
    }
}