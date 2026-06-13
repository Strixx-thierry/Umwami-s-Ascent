using UnityEngine.SceneManagement;

/// <summary>
/// Central place for scene names and navigation. Keep these names in sync
/// with the scene file names and the Build Settings list.
/// </summary>
public static class GameFlow
{
    public const string MainMenu = "MainMenu";
    public const string Gameplay = "Gameplay";
    public const string Win = "Win";
    public const string Lose = "Lose";

    public static void LoadMainMenu() => SceneManager.LoadScene(MainMenu);
    public static void LoadGameplay() => SceneManager.LoadScene(Gameplay);
    public static void LoadWin() => SceneManager.LoadScene(Win);
    public static void LoadLose() => SceneManager.LoadScene(Lose);

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }
}
