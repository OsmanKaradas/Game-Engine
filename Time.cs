using OpenTK.Mathematics;

public class Time
{
    public static float deltaTime;
    public static void Update(double frameTime)
    {
        deltaTime = (float)frameTime;
    }
}