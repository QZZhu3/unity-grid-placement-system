using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically grants items to the player's inventory when their category is unlocked.
///
/// Attach this to any GameObject in the scene (e.g. ProgressionSystem).
/// InventoryManager is auto-discovered at startup — no manual wiring needed.
///
/// For each entry, specify a category and the items (with quantities) to grant
/// when that category first unlocks.
///
/// Usage:
///   1. Add this component to ProgressionSystem.
///   2. In the Inspector, add entries to "Grants On Unlock".
///   3. For each entry, set the Category and add item+quantity pairs.
/// </summary>
public class UnlockRewardGranter : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private UnlockManager    unlockManager;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Grants On Unlock")]
    [Tooltip("Each entry defines which items (and how many) to grant when a specific category unlocks.")]
    [SerializeField] private List<CategoryGrantEntry> grantsOnUnlock = new List<CategoryGrantEntry>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Auto-discover anywhere in the scene — not just on the same GameObject
        if (unlockManager    == null) unlockManager    = FindAnyObjectByType<UnlockManager>();
        if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (unlockManager    == null) Debug.LogWarning("[UnlockRewardGranter] UnlockManager not found.");
        if (inventoryManager == null) Debug.LogWarning("[UnlockRewardGranter] InventoryManager not found.");
    }

    private void OnEnable()
    {
        if (unlockManager != null)
            unlockManager.OnCategoryUnlocked += HandleCategoryUnlocked;
    }

    private void OnDisable()
    {
        if (unlockManager != null)
            unlockManager.OnCategoryUnlocked -= HandleCategoryUnlocked;
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    private void HandleCategoryUnlocked(ItemCategory category)
    {
        if (inventoryManager == null) return;

        foreach (CategoryGrantEntry entry in grantsOnUnlock)
        {
            if (entry.Category == null) continue;
            if (entry.Category.Id != category.Id) continue;

            foreach (ItemGrantEntry grant in entry.ItemsToGrant)
            {
                if (grant.Item == null) continue;
                int qty = Mathf.Max(1, grant.Quantity);
                inventoryManager.AddItem(grant.Item, qty);
                Debug.Log($"[UnlockRewardGranter] Granted {qty}x '{grant.Item.DisplayName}' " +
                          $"(category '{category.DisplayName}' unlocked).");
            }
        }
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

/// <summary>
/// Pairs a category with a list of item+quantity grants triggered when it unlocks.
/// </summary>
[System.Serializable]
public class CategoryGrantEntry
{
    [Tooltip("The category whose unlock triggers these grants.")]
    public ItemCategory Category;

    [Tooltip("Items and quantities to add to inventory when this category unlocks.")]
    public List<ItemGrantEntry> ItemsToGrant = new List<ItemGrantEntry>();
}

/// <summary>
/// A single item and the quantity to grant.
/// </summary>
[System.Serializable]
public class ItemGrantEntry
{
    [Tooltip("The PlaceableItem to add to the player's inventory.")]
    public PlaceableItem Item;

    [Tooltip("How many of this item to grant. Minimum 1.")]
    [Min(1)]
    public int Quantity = 1;
}
