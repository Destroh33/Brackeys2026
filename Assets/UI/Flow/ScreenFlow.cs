using UnityEngine.SceneManagement;

public static class ScreenFlow
{
    public const string Title = "Title";
    public const string Options = "Options";
    public const string Level = "Level";
    public const string End = "End";
    public const string Leaderboard = "Leaderboard";

    public static string ReturnScene = Options;

    public static void Go(string scene) => SceneManager.LoadScene(scene);

    public static void GoTitle() => Go(Title);
    public static void GoOptions() => Go(Options);
    public static void GoLevel() => Go(Level);
    public static void GoEnd() => Go(End);

    public static void GoLeaderboard(string from)
    {
        ReturnScene = from;
        Go(Leaderboard);
    }

    public static void GoBack() => Go(ReturnScene);
}
