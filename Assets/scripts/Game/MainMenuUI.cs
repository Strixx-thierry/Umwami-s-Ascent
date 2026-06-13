using UnityEngine;

/// <summary>
/// Wires the Main Menu buttons. The Settings panel is a child object that is
/// shown/hidden in place (no separate scene).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public GameObject settingsPanel;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnPlay() => GameFlow.LoadGameplay();

    public void OnOpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnQuit() => GameFlow.Quit();
}
