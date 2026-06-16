using System;
using UnityEngine;

/// <summary>
/// Player hit points + a "chances" (lives) system.
/// Taking damage drains the current health bar. When it empties you lose one
/// chance and the bar refills. When all chances are used up the Lose scene
/// loads. UI (HealthBarUI) subscribes to the two events below.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int maxChances = 3;
    public float invincibleTime = 0.8f;

    public int CurrentHealth { get; private set; }
    public int CurrentChances { get; private set; }

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action<int> OnChancesChanged;      // (remaining)

    SpriteRenderer sr;
    float invulnUntil;
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

    public void TakeDamage(int amount)
    {
        if (gameOver || Time.time < invulnUntil) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Abs(amount));
        invulnUntil = Time.time + invincibleTime;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Debug.Log($"Hurt! HP = {CurrentHealth}/{maxHealth}, chances = {CurrentChances}");

        if (sr != null) StartCoroutine(Flash());

        if (CurrentHealth <= 0)
            LoseChance();
    }

    public void Heal(int amount)
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
            GameOver();
        }
        else
        {
            // Refill the bar for the next chance.
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"Lost a chance. {CurrentChances} left.");
        }
    }

    System.Collections.IEnumerator Flash()
    {
        Color original = sr.color;
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.12f);
        sr.color = original;
    }

    void GameOver()
    {
        gameOver = true;
        Debug.Log("Out of chances -> loading Lose scene");
        if (GameManager.Instance != null) GameManager.Instance.Lose();
        else GameFlow.LoadLose();
    }

    // Verify in Play mode with no other systems:
    // right-click the PlayerHealth header -> "TEST: Take 1 Damage".
    [ContextMenu("TEST: Take 1 Damage")]
    void TestDamage()
    {
        invulnUntil = 0f;
        TakeDamage(1);
    }
}
