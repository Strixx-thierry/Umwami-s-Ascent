using UnityEngine;

/// <summary>
/// Drop this on a trigger collider around a boss arena. When the player enters,
/// the bossfight music starts; the lobby music returns when the fight ends.
///
/// End the fight either by leaving the zone (endOnExit) or by calling
/// EndFight() from your boss-death logic.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossFightZone : MonoBehaviour
{
    public string playerTag = "Player";
    [Tooltip("If true, leaving the zone also ends the bossfight music.")]
    public bool endOnExit = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && MusicManager.Instance != null)
            MusicManager.Instance.EnterBossFight();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (endOnExit && other.CompareTag(playerTag) && MusicManager.Instance != null)
            MusicManager.Instance.ExitBossFight();
    }

    /// <summary>Call this when the boss is defeated.</summary>
    public void EndFight()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.ExitBossFight();
    }
}
