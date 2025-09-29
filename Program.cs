using GameEngine.Physics;
using JoltPhysicsSharp;

namespace GameEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            RigTest game = new(960, 540);
            game.Run();
        }
    }
}