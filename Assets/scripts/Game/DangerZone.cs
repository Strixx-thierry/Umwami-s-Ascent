using UnityEngine;

/// <summary>
/// A pass-through danger area (e.g. a red block). It has NO Rigidbody and is
/// a trigger, so the player walks through it but loses health while inside.
/// The player's i-frames throttle how often it ticks.
/// Put this on a GameObject that has a Collider2D set to "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DangerZone : MonoBehaviour
{
    public int damage = 1;
    public string playerTag = "Player";

    void Reset()
    {
        // When first added, make the collider a trigger automatically.
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) => TryHurt(other);
    void OnTriggerStay2D(Collider2D other) => TryHurt(other);

    void TryHurt(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage);
    }
}
