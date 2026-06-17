using UnityEngine;

/// <summary>
/// Stationary Shaman boss. It does NOT move or chase — it stands in the boss
/// arena and damages the player on contact. Pair with EnemyHealth (isBoss=true)
/// so defeating it wins the game, and a BossFightZone to start the music.
/// </summary>
public class BossController : MonoBehaviour
{
    [Tooltip("Damage dealt to the player on contact (player max is 100).")]
    public float contactDamage = 34f;
    public float damageInterval = 1f;
    public string playerTag = "Player";

    float nextDamageTime;

    void OnCollisionStay2D(Collision2D col) => TryDamage(col.collider);
    void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    void TryDamage(Collider2D other)
    {
        if (Time.time < nextDamageTime) return;
        if (!other.CompareTag(playerTag)) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
