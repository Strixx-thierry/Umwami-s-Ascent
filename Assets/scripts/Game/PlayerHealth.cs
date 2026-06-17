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

    public float CurrentHealth { get; private set; }

    // (current, max)
    public event Action<float, float> OnHealthChanged;

    [Header("Death")]
    [Tooltip("Seconds to let the death animation play before the Lose scene loads.")]
    public float deathDelay = 0.9f;

    SpriteRenderer sr;
    Animator anim;
    float lastDamageTime = -999f;
    float nextFlashTime;
    bool dead;

    void Awake()
    {
        CurrentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update()
    {
        if (dead || CurrentHealth >= maxHealth) return;

        if (Time.time >= lastDamageTime + regenDelay)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + regenPerSecond * Time.deltaTime);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Abs(amount));
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (sr != null && Time.time >= nextFlashTime)
        {
            nextFlashTime = Time.time + 0.15f;
            StartCoroutine(Flash());
        }

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (dead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + Mathf.Abs(amount));
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    System.Collections.IEnumerator Flash()
    {
        Color original = sr.color;
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }

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

    System.Collections.IEnumerator LoadLoseAfterDeath()
    {
        yield return new WaitForSeconds(anim != null ? deathDelay : 0f);
        if (GameManager.Instance != null) GameManager.Instance.Lose();
        else GameFlow.LoadLose();
    }

    [ContextMenu("TEST: Take 25 Damage")]
    void TestDamage() => TakeDamage(25f);
}
