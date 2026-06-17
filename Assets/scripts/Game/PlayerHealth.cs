using System;
using UnityEngine;

/// <summary>
/// Player health, 0..maxHealth shown as a percentage. Health drains while in a
/// danger zone (or on boss contact) and slowly regenerates after regenDelay
/// seconds out of danger. When it reaches 0 the Lose scene loads. No lives/chances.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health (0..100)")]
    public float maxHealth = 100f;
    public float regenPerSecond = 12f;
    [Tooltip("Seconds out of danger before health starts recovering.")]
    public float regenDelay = 1.5f;

    public float CurrentHealth { get; private set; }   // read-only to the outside

    // Event raised whenever health changes. The HUD (HealthBarUI) subscribes to
    // this instead of reading health every frame — so the data and the UI stay
    // decoupled. Args are (current, max).
    public event Action<float, float> OnHealthChanged;

    [Header("Death")]
    [Tooltip("Seconds to let the death animation play before the Lose scene loads.")]
    public float deathDelay = 0.9f;

    // ---- cached references / internal state ----
    SpriteRenderer sr;          // used for the red damage flash
    Animator anim;             // optional; plays a "Death" trigger if present
    float lastDamageTime = -999f;  // when we were last hurt (gates regen)
    float nextFlashTime;        // throttles the damage flash
    bool dead;

    void Awake()
    {
        CurrentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Push the starting value so the HUD shows 100% immediately.
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update()
    {
        // Nothing to do if dead or already full.
        if (dead || CurrentHealth >= maxHealth) return;

        // Regenerate, but only once enough time has passed since the last hit.
        if (Time.time >= lastDamageTime + regenDelay)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + regenPerSecond * Time.deltaTime);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);   // update the HUD
        }
    }

    // Called by danger zones and the boss to remove health.
    public void TakeDamage(float amount)
    {
        if (dead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Abs(amount));
        lastDamageTime = Time.time;                          // pauses regen
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);   // update the HUD

        // Brief red flash for feedback (throttled so it can't spam).
        if (sr != null && Time.time >= nextFlashTime)
        {
            nextFlashTime = Time.time + 0.15f;
            StartCoroutine(Flash());
        }

        if (CurrentHealth <= 0f)
            Die();
    }

    // Restore health (kept for pickups / future use).
    public void Heal(float amount)
    {
        if (dead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + Mathf.Abs(amount));
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // Tint the sprite red for a moment, then restore the original colour.
    System.Collections.IEnumerator Flash()
    {
        Color original = sr.color;
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }

    // Run the death sequence: play the animation (if any) then load Lose.
    void Die()
    {
        dead = true;
        Debug.Log("Player died -> loading Lose scene");
        if (HasParam("Death")) anim.SetTrigger("Death");
        StartCoroutine(LoadLoseAfterDeath());
    }

    bool HasParam(string n)
    {
        if (anim == null) return false;
        foreach (var p in anim.parameters) if (p.name == n) return true;
        return false;
    }

    // Wait for the death animation, then hand off to the Lose scene.
    System.Collections.IEnumerator LoadLoseAfterDeath()
    {
        yield return new WaitForSeconds(anim != null ? deathDelay : 0f);
        if (GameManager.Instance != null) GameManager.Instance.Lose();
        else GameFlow.LoadLose();
    }

    // Right-click the component in the Inspector to test taking damage.
    [ContextMenu("TEST: Take 25 Damage")]
    void TestDamage() => TakeDamage(25f);
}
