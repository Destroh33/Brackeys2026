public static class RunResult
{
    public static bool HasRun { get; private set; }
    public static float Time { get; private set; }
    public static int Deaths { get; private set; }
    public static string Seed { get; private set; } = "";
    public static bool Seeded { get; private set; }

    public static void Record(float seconds, int deaths, string seed, bool seeded)
    {
        HasRun = true;
        Time = seconds;
        Deaths = deaths;
        Seed = seed ?? "";
        Seeded = seeded;
    }

    public static void Clear()
    {
        HasRun = false;
        Time = 0f;
        Deaths = 0;
        Seed = "";
        Seeded = false;
    }
}
