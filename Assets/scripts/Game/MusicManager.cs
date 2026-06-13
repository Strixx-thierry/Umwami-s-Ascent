using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent music player. Survives scene loads, picks the lobby track for
/// menu/win/lose scenes and the game track for the gameplay scene, and applies
/// the two separate volume settings stored in PlayerPrefs.
/// Assign your audio clips in the Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public const string LobbyVolKey = "umwami_lobbyVolume";
    public const string GameVolKey  = "umwami_gameVolume";
    public const float DefaultVolume = 0.7f;

    [Header("Tracks (assign your audio clips)")]
    public AudioClip lobbyMusic;
    public AudioClip gameMusic;

    [Header("Which scene counts as gameplay")]
    public string gameplaySceneName = GameFlow.Gameplay;

    AudioSource source;
    bool playingGameTrack;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyForScene(scene.name);

    void ApplyForScene(string sceneName)
    {
        bool useGame = sceneName == gameplaySceneName;
        AudioClip target = useGame ? gameMusic : lobbyMusic;
        playingGameTrack = useGame;

        if (target != null && source.clip != target)
        {
            source.clip = target;
            source.Play();
        }
        else if (target == null)
        {
            source.Stop();
        }

        RefreshVolume();
    }

    public float LobbyVolume => PlayerPrefs.GetFloat(LobbyVolKey, DefaultVolume);
    public float GameVolume  => PlayerPrefs.GetFloat(GameVolKey, DefaultVolume);

    public void SetLobbyVolume(float v)
    {
        PlayerPrefs.SetFloat(LobbyVolKey, Mathf.Clamp01(v));
        RefreshVolume();
    }

    public void SetGameVolume(float v)
    {
        PlayerPrefs.SetFloat(GameVolKey, Mathf.Clamp01(v));
        RefreshVolume();
    }

    public void RefreshVolume()
    {
        if (source != null)
            source.volume = playingGameTrack ? GameVolume : LobbyVolume;
    }
}
