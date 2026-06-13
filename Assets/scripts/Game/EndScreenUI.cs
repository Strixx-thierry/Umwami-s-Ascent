using UnityEngine;

/// <summary>
/// Buttons shared by the Win and Lose scenes.
/// </summary>
public class EndScreenUI : MonoBehaviour
{
    public void OnRetry() => GameFlow.LoadGameplay();
    public void OnMainMenu() => GameFlow.LoadMainMenu();
    public void OnQuit() => GameFlow.Quit();
}
