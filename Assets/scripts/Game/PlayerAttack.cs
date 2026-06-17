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
    public int damage = 1;
    public float range = 2.5f;
    public float cooldown = 0.4f;
    [Tooltip("Pushes the ray start in front of the player so it doesn't hit itself.")]
    public float originOffset = 0.6f;
    public Color slashColor = new Color(1f, 0.9f, 0.5f, 1f);

    LineRenderer line;
    Animator anim;
    float nextAttackTime;

    void Awake()
    {
        anim = GetComponent<Animator>();
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = 0.08f;
        line.numCapVertices = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = slashColor;
        line.endColor = slashColor;
        line.sortingOrder = 60;
        line.enabled = false;
    }

    void Update()
    {
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
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cooldown;

        if (HasParam("Slash")) anim.SetTrigger("Slash");

        float dir = Mathf.Sign(transform.localScale.x);
        if (dir == 0) dir = 1f;
        Vector2 direction = new Vector2(dir, 0f);
        Vector2 origin = (Vector2)transform.position + direction * originOffset;
        Vector2 end = origin + direction * range;

        Debug.Log($"[Attack] fired, facing {(dir > 0 ? "right" : "left")}");

        // Scan EVERY collider along the ray (triggers included) and pick the
        // closest one that actually has an EnemyHealth. This skips trigger
        // volumes we're standing in (boss zone, danger zone) and the ground,
        // which would otherwise block a single Raycast before reaching the boss.
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;
        RaycastHit2D[] results = new RaycastHit2D[16];
        int count = Physics2D.Raycast(origin, direction, filter, results, range);

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
                end = results[i].point;
            }
        }

        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"[Attack] HIT {target.name} for {damage}");
        }
        else
        {
            Debug.Log("[Attack] no enemy in range (facing away or too far)");
        }

        StartCoroutine(ShowSlash(origin, end));
    }

    bool HasParam(string n)
    {
        if (anim == null) return false;
        foreach (var p in anim.parameters) if (p.name == n) return true;
        return false;
    }

    IEnumerator ShowSlash(Vector2 a, Vector2 b)
    {
        line.SetPosition(0, a);
        line.SetPosition(1, b);
        line.enabled = true;
        yield return new WaitForSeconds(0.15f);
        line.enabled = false;
    }
}
