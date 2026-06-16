using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the HUD from PlayerHealth: shows health as a percentage TextMesh
/// (100% -> 0%) and the remaining chances. Still fills an optional bar Image if
/// one is present. Auto-finds the player and HUD objects by name if not
/// assigned (HealthText, HealthBarFill, ChancesText).
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth player;
    public TMP_Text healthText;    // "Health: NN%"
    public TMP_Text chancesText;   // "Chances: N"
    public Image fillImage;        // optional bar meter

    void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
        if (healthText == null)  healthText  = FindText("HealthText");
        if (chancesText == null) chancesText = FindText("ChancesText");
        if (fillImage == null)
        {
            var go = GameObject.Find("HealthBarFill");
            if (go != null) fillImage = go.GetComponent<Image>();
        }
    }

    static TMP_Text FindText(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
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

    void UpdateHealth(float current, float max)
    {
        float pct = max > 0f ? (current / max) * 100f : 0f;
        if (healthText != null)
            healthText.text = "Health: " + Mathf.CeilToInt(Mathf.Clamp(pct, 0f, 100f)) + "%";
        if (fillImage != null)
            fillImage.fillAmount = max > 0f ? current / max : 0f;
    }

    void UpdateChances(int remaining)
    {
        if (chancesText != null)
            chancesText.text = "Chances: " + remaining;
    }
}
