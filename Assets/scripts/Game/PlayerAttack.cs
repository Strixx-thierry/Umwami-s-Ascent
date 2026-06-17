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
    float nextAttackTime;

    void Awake()
    {
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

        float dir = Mathf.Sign(transform.localScale.x);
        if (dir == 0) dir = 1f;
        Vector2 direction = new Vector2(dir, 0f);
        Vector2 origin = (Vector2)transform.position + direction * originOffset;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range);
        Vector2 end = origin + direction * range;

        if (hit.collider != null)
        {
            end = hit.point;
            var enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        StartCoroutine(ShowSlash(origin, end));
    }

    IEnumerator ShowSlash(Vector2 a, Vector2 b)
    {
        line.SetPosition(0, a);
        line.SetPosition(1, b);
        line.enabled = true;
        yield return new WaitForSeconds(0.08f);
        line.enabled = false;
    }
}
