using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent music player. Survives scene loads.
///
/// Lobby music is the DEFAULT track and plays everywhere (menu, gameplay,
/// win, lose). The boss track only plays during a boss fight: call
/// EnterBossFight() to switch to it and ExitBossFight() to return to lobby.
/// Loading any scene resets back to lobby music.
///
/// Two independent volumes (lobby + boss) are saved in PlayerPrefs.
/// Assign your audio clips in the Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public const string LobbyVolKey = "umwami_lobbyVolume";
    public const string BossVolKey  = "umwami_bossVolume";
    public const float DefaultVolume = 0.7f;

    [Header("Tracks (assign your audio clips)")]
    public AudioClip lobbyMusic;   // default track, plays everywhere
    public AudioClip bossMusic;    // plays only during a boss fight

    [Header("Crossfade")]
    public float fadeDuration = 0.6f;

    AudioSource source;
    bool inBossFight;
    Coroutine fadeRoutine;

    public bool InBossFight => inBossFight;

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
        PlayLobby(instant: true);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // A new scene always starts on lobby music (no fight in progress yet).
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        inBossFight = false;
        PlayLobby(instant: true);
    }

    // ---- public API: call these from your boss logic ----

    public void EnterBossFight()
    {
        if (inBossFight) return;
        inBossFight = true;
        SwitchTo(bossMusic);
    }

    public void ExitBossFight()
    {
        if (!inBossFight) return;
        inBossFight = false;
        SwitchTo(lobbyMusic);
    }

    void PlayLobby(bool instant)
    {
        if (instant)
        {
            if (lobbyMusic != null && source.clip != lobbyMusic)
            {
                source.clip = lobbyMusic;
                source.Play();
            }
            RefreshVolume();
        }
        else
        {
            SwitchTo(lobbyMusic);
        }
    }

    void SwitchTo(AudioClip target)
    {
        if (target == null || source.clip == target)
        {
            RefreshVolume();
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        if (gameObject.activeInHierarchy)
            fadeRoutine = StartCoroutine(Crossfade(target));
        else
        {
            source.clip = target;
            source.Play();
            RefreshVolume();
        }
    }

    IEnumerator Crossfade(AudioClip target)
    {
        float full = TargetVolume();
        float t = 0f;

        // fade out current
        while (t < fadeDuration && source.isPlaying)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(full, 0f, t / fadeDuration);
            yield return null;
        }

        source.clip = target;
        source.Play();

        // fade in new
        t = 0f;
        full = TargetVolume();
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, full, t / fadeDuration);
            yield return null;
        }

        source.volume = full;
        fadeRoutine = null;
    }

    // ---- volume ----

    public float LobbyVolume => PlayerPrefs.GetFloat(LobbyVolKey, DefaultVolume);
    public float BossVolume  => PlayerPrefs.GetFloat(BossVolKey, DefaultVolume);

    public void SetLobbyVolume(float v)
    {
        PlayerPrefs.SetFloat(LobbyVolKey, Mathf.Clamp01(v));
        RefreshVolume();
    }

    public void SetBossVolume(float v)
    {
        PlayerPrefs.SetFloat(BossVolKey, Mathf.Clamp01(v));
        RefreshVolume();
    }

    float TargetVolume() => inBossFight ? BossVolume : LobbyVolume;

    public void RefreshVolume()
    {
        // Don't fight an in-progress crossfade.
        if (fadeRoutine == null && source != null)
            source.volume = TargetVolume();
    }
}
