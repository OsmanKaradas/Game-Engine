using OpenTK.Mathematics;

public static class Time
{
    public static float deltaTime;
    public static int fps;
    private static float timer;
    private static int frames;

    public static void Update(double frameTime)
    {
        deltaTime = (float)frameTime;
        timer += deltaTime;
        frames++;

        // FPS COUNTER
        if (timer >= 1.0f)
        {
            fps = Convert.ToInt32(frames / timer);
            timer = 0f;
            frames = 0;
        }
    }
}