using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Presentation-only component for a single reward slot in the chest opening panel.
///
/// Responsibilities:
///   - Display item icon, name, and rarity visuals
///   - Play a reveal animation (scale from 0 -> 1)
///   - Provide rarity border/glow hooks for future FX
///
/// This component MUST NOT:
///   - Grant rewards
///   - Modify inventory
///   - Execute any reward logic
///
/// All reward logic lives in RewardManager. This is a pure display component.
/// </summary>
public class RewardSlotUI : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Display References")]
    [SerializeField] private Image            itemIcon;
    [SerializeField] private TextMeshProUGUI  itemNameText;
    [SerializeField] private TextMeshProUGUI  rarityText;
    [SerializeField] private TextMeshProUGUI  quantityText;

    [Header("Rarity Visuals (FX hooks)")]
    [Tooltip("Border image -- tinted by rarity colour.")]
    [SerializeField] private Image rarityBorder;
    [Tooltip("Glow image -- placeholder for future particle/shader FX.")]
    [SerializeField] private Image rarityGlow;

    [Header("Reveal Animation")]
    [Tooltip("Duration of the scale-in reveal animation in seconds.")]
    [SerializeField] private float revealDuration = 0.35f;

    // -- Rarity colour palette -------------------------------------------------

    private static readonly Color ColourCommon    = new Color(0.75f, 0.75f, 0.75f);
    private static readonly Color ColourUncommon  = new Color(0.30f, 0.85f, 0.30f);
    private static readonly Color ColourRare      = new Color(0.20f, 0.50f, 1.00f);
    private static readonly Color ColourSeasonal  = new Color(1.00f, 0.65f, 0.10f);

    // -- Runtime state ---------------------------------------------------------

    private RewardResult currentReward;
    private Coroutine    revealCoroutine;

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Populates the slot with reward data. Does not play the reveal animation.
    /// Call <see cref="PlayRevealAnimation"/> separately to animate it in.
    /// </summary>
    public void SetReward(RewardResult reward)
    {
        currentReward = reward;

        if (reward == null || reward.Item == null)
        {
            SetEmpty();
            return;
        }

        // Icon
        if (itemIcon != null)
        {
            if (reward.Item.Icon != null)
            {
                itemIcon.sprite  = reward.Item.Icon;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false;
            }
        }

        // Name
        if (itemNameText != null)
            itemNameText.text = reward.Item.DisplayName;

        // Rarity label
        if (rarityText != null)
            rarityText.text = reward.DrawnRarity.ToString();

        // Quantity (hide if 1)
        if (quantityText != null)
            quantityText.text = reward.Quantity > 1 ? $"x{reward.Quantity}" : string.Empty;

        // Rarity visuals
        SetRarityVisuals(reward.DrawnRarity);

        // Start hidden for reveal animation
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Plays the scale-in reveal animation. Returns immediately; animation runs as coroutine.
    /// </summary>
    public void PlayRevealAnimation()
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);
        revealCoroutine = StartCoroutine(RevealCoroutine(revealDuration));
    }

    /// <summary>
    /// Instantly skips to the fully-revealed state without animation.
    /// </summary>
    public void SkipReveal()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Sets rarity border and glow colours. Extend this for shaders and particles.
    /// </summary>
    public void SetRarityVisuals(ItemRarity rarity)
    {
        Color colour = RarityToColour(rarity);

        if (rarityBorder != null)
            rarityBorder.color = colour;

        if (rarityGlow != null)
        {
            Color glow = colour;
            glow.a = 0.35f;   // subtle glow placeholder
            rarityGlow.color = glow;
        }
    }

    // -- Private helpers -------------------------------------------------------

    private void SetEmpty()
    {
        if (itemIcon     != null) itemIcon.enabled    = false;
        if (itemNameText != null) itemNameText.text   = string.Empty;
        if (rarityText   != null) rarityText.text     = string.Empty;
        if (quantityText != null) quantityText.text   = string.Empty;
    }

    private IEnumerator RevealCoroutine(float duration)
    {
        float elapsed = 0f;
        transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out cubic
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.one * eased;
            yield return null;
        }

        transform.localScale = Vector3.one;
        revealCoroutine = null;
    }

    private static Color RarityToColour(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common   => ColourCommon,
            ItemRarity.Uncommon => ColourUncommon,
            ItemRarity.Rare     => ColourRare,
            ItemRarity.Seasonal => ColourSeasonal,
            _                   => ColourCommon
        };
    }
}
