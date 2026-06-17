using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the HUD from PlayerHealth: shows health as a percentage TextMesh
/// (Health: NN%). Also fills an optional bar Image if one is assigned/present.
/// Auto-finds the player and HealthText by name if not assigned in the Inspector.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth player;    // whose health we display
    public TMP_Text healthText;    // "Health: NN%"
    public Image fillImage;        // optional bar meter

    void Awake()
    {
        // Auto-wire references so the HUD works even if nothing is dragged in
        // the Inspector: find the player, the text, and the optional bar fill.
        if (player == null) player = FindAnyObjectByType<PlayerHealth>();
        if (healthText == null)
        {
            var go = GameObject.Find("HealthText");
            if (go != null) healthText = go.GetComponent<TMP_Text>();
        }
        if (fillImage == null)
        {
            var go = GameObject.Find("HealthBarFill");
            if (go != null) fillImage = go.GetComponent<Image>();
        }
    }

    // Subscribe to the health event while this HUD is enabled, and draw the
    // current value once so it isn't blank at the start.
    void OnEnable()
    {
        if (player == null) return;
        player.OnHealthChanged += UpdateHealth;
        UpdateHealth(player.CurrentHealth, player.maxHealth);
    }

    // Always unsubscribe when disabled to avoid dangling event handlers.
    void OnDisable()
    {
        if (player == null) return;
        player.OnHealthChanged -= UpdateHealth;
    }

    // Called by the event: convert health to a 0-100% and update the text/bar.
    void UpdateHealth(float current, float max)
    {
        float pct = max > 0f ? (current / max) * 100f : 0f;
        if (healthText != null)
            healthText.text = "Health: " + Mathf.CeilToInt(Mathf.Clamp(pct, 0f, 100f)) + "%";
        if (fillImage != null)
            fillImage.fillAmount = max > 0f ? current / max : 0f;
    }
}
