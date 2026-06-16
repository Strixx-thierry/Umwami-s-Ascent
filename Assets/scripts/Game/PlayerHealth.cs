using System;
using UnityEngine;

/// <summary>
/// Player health (0..maxHealth, shown as a percentage) plus a "chances" system.
/// Health drains while in a danger zone and slowly REGENERATES when the player
/// has been out of danger for regenDelay seconds. When health hits 0 the player
/// loses a chance and health refills; out of chances -> Lose scene.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float regenPerSecond = 12f;
    [Tooltip("Seconds out of danger before health starts recovering.")]
    public float regenDelay = 1.5f;

    [Header("Chances")]
    public int maxChances = 3;

    public float CurrentHealth { get; private set; }
    public int CurrentChances { get; private set; }

    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<int> OnChancesChanged;          // (remaining)

    SpriteRenderer sr;
    float lastDamageTime = -999f;
    float nextFlashTime;
    bool gameOver;

    void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentChances = maxChances;
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnChancesChanged?.Invoke(CurrentChances);
    }

    void Update()
    {
        if (gameOver || CurrentHealth >= maxHealth) return;

        // Recover slowly once we've been out of danger long enough.
        if (Time.time >= lastDamageTime + regenDelay)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + regenPerSecond * Time.deltaTime);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (gameOver) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Abs(amount));
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (sr != null && Time.time >= nextFlashTime)
        {
            nextFlashTime = Time.time + 0.15f;
            StartCoroutine(Flash());
        }

        if (CurrentHealth <= 0f)
            LoseChance();
    }

    public void Heal(float amount)
    {
        if (gameOver) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + Mathf.Abs(amount));
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void LoseChance()
    {
        CurrentChances--;
        OnChancesChanged?.Invoke(Mathf.Max(0, CurrentChances));

        if (CurrentChances <= 0)
        {
            gameOver = true;
            Debug.Log("Out of chances -> loading Lose scene");
            if (GameManager.Instance != null) GameManager.Instance.Lose();
            else GameFlow.LoadLose();
        }
        else
        {
            CurrentHealth = maxHealth; // refill for the next chance
            lastDamageTime = Time.time;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"Lost a chance. {CurrentChances} left.");
        }
    }

    System.Collections.IEnumerator Flash()
    {
        Color original = sr.color;
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }

    [ContextMenu("TEST: Take 25 Damage")]
    void TestDamage() => TakeDamage(25f);
}
