using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hit points for the Shaman boss (and any future enemy). Normal enemies are
/// destroyed on death; if isBoss is true, defeating it ends the bossfight music
/// and loads the Win scene.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    [Tooltip("The Shaman. Defeating it wins the game.")]
    public bool isBoss = false;
    public UnityEvent onDeath;

    public int CurrentHealth { get; private set; }

    SpriteRenderer sr;
    bool dead;

    void Awake()
    {
        CurrentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Abs(amount));
        Debug.Log($"{name} hit. HP = {CurrentHealth}/{maxHealth}");
        if (sr != null) StartCoroutine(Flash());
        if (CurrentHealth <= 0) Die();
    }

    System.Collections.IEnumerator Flash()
    {
        Color c = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        sr.color = c;
    }

    void Die()
    {
        dead = true;
        onDeath?.Invoke();

        if (isBoss)
        {
            Debug.Log("Shaman defeated -> Win");
            if (MusicManager.Instance != null) MusicManager.Instance.ExitBossFight();
            if (GameManager.Instance != null) GameManager.Instance.Win();
            else GameFlow.LoadWin();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
