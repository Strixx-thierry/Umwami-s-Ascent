using UnityEngine;

/// <summary>
/// A pass-through danger area (e.g. a red block). No Rigidbody, collider is a
/// trigger, so the player walks through it but loses health continuously while
/// inside. Step out and PlayerHealth slowly recovers.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DangerZone : MonoBehaviour
{
    [Tooltip("Health drained per second while the player stands in the zone.")]
    public float damagePerSecond = 40f;
    public string playerTag = "Player";

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damagePerSecond * Time.deltaTime);
    }
}
