using UnityEngine;

/// <summary>
/// Simple Shaman boss behaviour: when the player is within range it walks
/// toward them and damages them on contact. Pair with EnemyHealth (isBoss=true)
/// so defeating it wins the game.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    public float moveSpeed = 2.2f;
    public float detectRange = 14f;
    [Tooltip("Damage dealt to the player on contact (player max is 100).")]
    public float contactDamage = 34f;
    public float damageInterval = 1f;
    public string playerTag = "Player";

    Rigidbody2D rb;
    Transform player;
    float baseXScale;
    float nextDamageTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        baseXScale = Mathf.Abs(transform.localScale.x);

        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > detectRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dir = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(dir >= 0 ? baseXScale : -baseXScale,
                                           transform.localScale.y, transform.localScale.z);
    }

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
