namespace GameEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            LightTest game = new(1920, 1080);
            game.Run();
        }
    }
}