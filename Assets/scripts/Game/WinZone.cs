using UnityEngine;

/// <summary>
/// Drop this on a trigger collider at the end of a level. When the player
/// enters it, the Win scene loads. Tag your player object "Player".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WinZone : MonoBehaviour
{
    public string playerTag = "Player";
    [Tooltip("If true, the throne only works after the boss is defeated.")]
    public bool requireBossDefeated = true;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (requireBossDefeated && (GameManager.Instance == null || !GameManager.Instance.BossDefeated))
        {
            Debug.Log("The throne is sealed - defeat the Shaman first.");
            return;
        }

        if (GameManager.Instance != null) GameManager.Instance.Win();
        else GameFlow.LoadWin();
    }
}
