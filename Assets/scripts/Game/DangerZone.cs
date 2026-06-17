using UnityEngine;

/// <summary>
/// A pass-through danger area (e.g. a red block). No Rigidbody, collider is a
/// trigger, so the player walks through it but loses a little health on a timer
/// while inside. Step out and PlayerHealth slowly recovers.
/// Default: 1% every 1.5 seconds.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DangerZone : MonoBehaviour
{
    [Tooltip("Health removed each tick (1 = 1%).")]
    public float drainAmount = 1f;
    [Tooltip("Seconds between drains.")]
    public float drainInterval = 1.5f;
    public string playerTag = "Player";

    float nextDrainTime;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextDrainTime) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(drainAmount);
            nextDrainTime = Time.time + drainInterval;
        }
    }
}
