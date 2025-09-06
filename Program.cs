using GameEngine.Physics;
using JoltPhysicsSharp;

namespace GameEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            ShadowTest game = new(960, 540);
            game.Run();
        }
    }
}