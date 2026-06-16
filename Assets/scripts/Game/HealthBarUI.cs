using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the HUD: fills the health bar meter and updates the "Chances" text
/// from a PlayerHealth. Auto-finds the player and the HUD objects by name
/// (HealthBarFill, ChancesText) if they aren't assigned in the Inspector.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth player;
    public Image fillImage;        // the HealthBarFill image (type = Filled)
    public TMP_Text chancesText;   // the ChancesText label

    void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();

        if (fillImage == null)
        {
            var go = GameObject.Find("HealthBarFill");
            if (go != null) fillImage = go.GetComponent<Image>();
        }
        if (chancesText == null)
        {
            var go = GameObject.Find("ChancesText");
            if (go != null) chancesText = go.GetComponent<TMP_Text>();
        }
    }

    void OnEnable()
    {
        if (player == null) return;
        player.OnHealthChanged += UpdateHealth;
        player.OnChancesChanged += UpdateChances;
        UpdateHealth(player.CurrentHealth, player.maxHealth);
        UpdateChances(player.CurrentChances);
    }

    void OnDisable()
    {
        if (player == null) return;
        player.OnHealthChanged -= UpdateHealth;
        player.OnChancesChanged -= UpdateChances;
    }

    void UpdateHealth(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    void UpdateChances(int remaining)
    {
        if (chancesText != null)
            chancesText.text = "Chances: " + remaining;
    }
}
