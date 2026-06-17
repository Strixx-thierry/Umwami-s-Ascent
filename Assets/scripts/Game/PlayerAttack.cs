using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The King's spirit-spear strike. Press J or Left-Click (or call Attack() from
/// a mobile on-screen button) to fire a RAYCAST in the facing direction. Any
/// EnemyHealth it hits takes damage. A short LineRenderer "slash" gives visual
/// feedback (no attack animation needed).
/// This is the project's required Raycast mechanic.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PlayerAttack : MonoBehaviour
{
    // ---- Inspector-tunable values (all visible on the component) ----
    public int damage = 1;            // how much HP one hit removes (boss has 5)
    public float range = 2.5f;        // how far the slash reaches, in world units
    public float cooldown = 0.4f;     // minimum seconds between attacks
    [Tooltip("Pushes the ray start in front of the player so it doesn't hit itself.")]
    public float originOffset = 0.6f;
    public Color slashColor = new Color(1f, 0.9f, 0.5f, 1f); // colour of the slash line

    // ---- cached references / internal state ----
    LineRenderer line;         // draws the visible slash
    Animator anim;             // optional; used to trigger the "Slash" animation
    float nextAttackTime;      // Time.time at which we're allowed to attack again

    void Awake()
    {
        anim = GetComponent<Animator>();

        // Configure the LineRenderer once at startup so the slash is ready to
        // draw. It's kept disabled until an attack actually fires.
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;     // positions are world coords, not local
        line.positionCount = 2;        // a straight line = 2 points (start, end)
        line.widthMultiplier = 0.08f;
        line.numCapVertices = 2;       // rounds the line ends slightly
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = slashColor;
        line.endColor = slashColor;
        line.sortingOrder = 60;        // draw on top of the sprites
        line.enabled = false;          // hidden until we slash
    }

    void Update()
    {
        // Read attack input via the NEW Input System (no legacy Input class).
        // J on the keyboard or the left mouse button both trigger an attack.
        bool pressed = false;
        var kb = Keyboard.current;
        if (kb != null && kb.jKey.wasPressedThisFrame) pressed = true;
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) pressed = true;

        if (pressed) Attack();
    }

    /// <summary>Public so a mobile UI button can call it via OnClick.</summary>
    public void Attack()
    {
        // Enforce the cooldown: ignore the press if we attacked too recently.
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        // Play the slash animation if the Animator has a "Slash" trigger.
        if (HasParam("Slash")) anim.SetTrigger("Slash");

        // Work out which way we're facing from the sprite's X scale, then build
        // the ray: it starts a little in front of the player (originOffset) so it
        // doesn't immediately hit the player's own collider.
        float dir = Mathf.Sign(transform.localScale.x);
        if (dir == 0) dir = 1f;
        Vector2 direction = new Vector2(dir, 0f);
        Vector2 origin = (Vector2)transform.position + direction * originOffset;
        Vector2 end = origin + direction * range;   // default line end if we hit nothing

        Debug.Log($"[Attack] fired, facing {(dir > 0 ? "right" : "left")}");

        // Scan EVERY collider along the ray (triggers included) and pick the
        // closest one that actually has an EnemyHealth. This skips trigger
        // volumes we're standing in (boss zone, danger zone) and the ground,
        // which would otherwise block a single Raycast before reaching the boss.
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;     // include trigger colliders (the boss is one)
        filter.useLayerMask = false;   // don't restrict by layer
        RaycastHit2D[] results = new RaycastHit2D[16];               // reusable buffer
        int count = Physics2D.Raycast(origin, direction, filter, results, range);

        // Walk the hits and keep the nearest object that has an EnemyHealth.
        EnemyHealth target = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            var col = results[i].collider;
            if (col == null) continue;
            var enemy = col.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null && results[i].distance < bestDistance)
            {
                bestDistance = results[i].distance;
                target = enemy;
                end = results[i].point;   // draw the slash up to the point we hit
            }
        }

        // Apply damage to whatever enemy we found (if any).
        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"[Attack] HIT {target.name} for {damage}");
        }
        else
        {
            Debug.Log("[Attack] no enemy in range (facing away or too far)");
        }

        // Flash the slash line from origin to end for a moment.
        StartCoroutine(ShowSlash(origin, end));
    }

    // Safe check: does the Animator actually have a parameter with this name?
    // Lets the script work even if no Animator/parameter is set up.
    bool HasParam(string n)
    {
        if (anim == null) return false;
        foreach (var p in anim.parameters) if (p.name == n) return true;
        return false;
    }

    // Briefly show the slash line, then hide it again (visual feedback only).
    IEnumerator ShowSlash(Vector2 a, Vector2 b)
    {
        line.SetPosition(0, a);
        line.SetPosition(1, b);
        line.enabled = true;
        yield return new WaitForSeconds(0.15f);
        line.enabled = false;
    }
}
