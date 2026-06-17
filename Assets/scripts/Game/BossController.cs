using UnityEngine;

/// <summary>
/// The Shaman boss. It stands idle until the player enters the boss arena, then
/// CHASES the player across the floor, turning to face them, and damages on
/// contact. The chase is switched on/off by the BossFightZone (BeginChase /
/// StopChase) so the boss is only aggressive while the player is in the arena.
///
/// The boss uses a Kinematic Rigidbody2D, so it ignores gravity and simply
/// glides horizontally at its own height — it never falls or needs ground.
/// Pair with EnemyHealth (isBoss=true) so defeating it unlocks the throne.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Chase")]
    [Tooltip("Horizontal speed while chasing (player speed is 6, so keep this lower to stay escapable).")]
    public float moveSpeed = 2.5f;
    [Tooltip("Stop closing in once this near the player (keeps the boss from jittering on top of them).")]
    public float stopDistance = 1f;
    [Tooltip("Tick if the boss sprite faces RIGHT by default; untick if it faces left.")]
    public bool spriteFacesRight = true;

    [Header("Contact damage")]
    [Tooltip("Damage dealt to the player on contact (player max is 100).")]
    public float contactDamage = 34f;
    public float damageInterval = 1f;        // seconds between contact-damage ticks
    public string playerTag = "Player";

    // ---- cached references / internal state ----
    Rigidbody2D rb;            // moved by setting its velocity each physics step
    Animator anim;            // optional; drives an "isWalking" bool if present
    Transform player;         // the target we chase (set when the fight starts)
    float facingMagnitude;    // remembered |scale.x| so flipping keeps the size
    bool chasing;             // are we currently chasing? toggled by the zone
    float nextDamageTime;     // Time.time at which we may deal contact damage again

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        // Store the sprite's size so that when we flip left/right we only change
        // the SIGN of the X scale, never its magnitude.
        facingMagnitude = Mathf.Abs(transform.localScale.x);
        rb.freezeRotation = true;
    }

    /// <summary>Called by BossFightZone when the player enters the arena.</summary>
    public void BeginChase(Transform target)
    {
        player = target;
        chasing = true;
    }

    /// <summary>Called by BossFightZone when the player leaves (or the fight ends).</summary>
    public void StopChase()
    {
        chasing = false;
        player = null;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);  // halt horizontally
        SetWalking(false);
    }

    // Movement runs in FixedUpdate because it touches the Rigidbody (physics).
    void FixedUpdate()
    {
        // Not chasing (or no target yet): stand still.
        if (!chasing || player == null)
        {
            SetWalking(false);
            return;
        }

        // Compare X positions to decide which way to move and whether we're
        // close enough to stop.
        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);
        float dir = Mathf.Sign(dx);          // -1 = player is left, +1 = player is right

        bool moving = distance > stopDistance;
        float vx = moving ? dir * moveSpeed : 0f;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);  // glide horizontally

        if (dir != 0f) FacePlayer(dir);      // always look toward the player
        SetWalking(moving);
    }

    // Flips the boss horizontally to look at the player by setting the scale sign.
    void FacePlayer(float dir)
    {
        float sign = spriteFacesRight ? dir : -dir;
        transform.localScale = new Vector3(
            sign * facingMagnitude, transform.localScale.y, transform.localScale.z);
    }

    // Tell the Animator whether we're walking (only if such a parameter exists).
    void SetWalking(bool walking)
    {
        if (anim != null && HasParam("isWalking")) anim.SetBool("isWalking", walking);
    }

    bool HasParam(string n)
    {
        foreach (var p in anim.parameters) if (p.name == n) return true;
        return false;
    }

    // ---- contact damage: hurt the player while we're touching them ----
    // Works whether the boss collider is solid (OnCollisionStay2D) or a
    // trigger (OnTriggerStay2D).
    void OnCollisionStay2D(Collision2D col) => TryDamage(col.collider);
    void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    void TryDamage(Collider2D other)
    {
        if (Time.time < nextDamageTime) return;        // respect the damage cooldown
        if (!other.CompareTag(playerTag)) return;      // only the player gets hurt

        // Find the PlayerHealth (on the object or a parent) and damage it.
        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
