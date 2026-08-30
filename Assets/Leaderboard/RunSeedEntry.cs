using UnityEngine;

public static class RunSeedEntry
{
    const string SeedKey = "run_seed_entry";
    const string SeededKey = "run_seed_entered";

    public static void Set(string seed, bool userEntered)
    {
        PlayerPrefs.SetString(SeedKey, seed ?? "");
        PlayerPrefs.SetInt(SeededKey, userEntered ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static string Peek() => PlayerPrefs.GetString(SeedKey, "");

    public static bool WasEntered() => PlayerPrefs.GetInt(SeededKey, 0) == 1;

    public static string Consume()
    {
        string seed = Peek();
        PlayerPrefs.DeleteKey(SeedKey);
        PlayerPrefs.Save();
        return seed;
    }

    public static string Generate()
    {
        return Random.Range(100000000, int.MaxValue).ToString();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SeedKey);
        PlayerPrefs.DeleteKey(SeededKey);
        PlayerPrefs.Save();
    }
}
