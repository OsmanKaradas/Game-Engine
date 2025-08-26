using GameEngine.Physics;
using JoltPhysicsSharp;

namespace GameEngine
{
    public class Program
    {
        // Entry point of the program
        static void Main(string[] args)
        {
            // Creates game object and disposes of it after leaving the scope

            using(Test game = new Test(1920, 1080))
            {
                // running the game
                game.Run();
            }
        }
    }
}