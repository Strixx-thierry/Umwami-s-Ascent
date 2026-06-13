using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal settings: two music volume sliders (lobby + bossfight). Values are
/// read from / written to PlayerPrefs and applied live to the MusicManager.
/// (gameVolumeSlider/gameValueLabel control the BOSSFIGHT music volume.)
/// </summary>
public class SettingsPanelUI : MonoBehaviour
{
    public Slider lobbyVolumeSlider;
    public Slider gameVolumeSlider;     // bossfight music volume
    public TMP_Text lobbyValueLabel;
    public TMP_Text gameValueLabel;     // bossfight music value

    void OnEnable()
    {
        float lobby = PlayerPrefs.GetFloat(MusicManager.LobbyVolKey, MusicManager.DefaultVolume);
        float game  = PlayerPrefs.GetFloat(MusicManager.BossVolKey, MusicManager.DefaultVolume);

        if (lobbyVolumeSlider != null)
        {
            lobbyVolumeSlider.minValue = 0f;
            lobbyVolumeSlider.maxValue = 1f;
            lobbyVolumeSlider.SetValueWithoutNotify(lobby);
            lobbyVolumeSlider.onValueChanged.RemoveListener(OnLobbyChanged);
            lobbyVolumeSlider.onValueChanged.AddListener(OnLobbyChanged);
        }

        if (gameVolumeSlider != null)
        {
            gameVolumeSlider.minValue = 0f;
            gameVolumeSlider.maxValue = 1f;
            gameVolumeSlider.SetValueWithoutNotify(game);
            gameVolumeSlider.onValueChanged.RemoveListener(OnGameChanged);
            gameVolumeSlider.onValueChanged.AddListener(OnGameChanged);
        }

        UpdateLabels(lobby, game);
    }

    void OnLobbyChanged(float v)
    {
        if (MusicManager.Instance != null) MusicManager.Instance.SetLobbyVolume(v);
        else PlayerPrefs.SetFloat(MusicManager.LobbyVolKey, v);
        UpdateLabels(v, gameVolumeSlider != null ? gameVolumeSlider.value : 0f);
    }

    void OnGameChanged(float v)
    {
        if (MusicManager.Instance != null) MusicManager.Instance.SetBossVolume(v);
        else PlayerPrefs.SetFloat(MusicManager.BossVolKey, v);
        UpdateLabels(lobbyVolumeSlider != null ? lobbyVolumeSlider.value : 0f, v);
    }

    void UpdateLabels(float lobby, float game)
    {
        if (lobbyValueLabel != null) lobbyValueLabel.text = Mathf.RoundToInt(lobby * 100f) + "%";
        if (gameValueLabel != null)  gameValueLabel.text  = Mathf.RoundToInt(game * 100f) + "%";
    }
}
