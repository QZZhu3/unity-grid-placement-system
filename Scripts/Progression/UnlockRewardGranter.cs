using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically grants items to the player's inventory when their category is unlocked.
///
/// Attach this to the ProgressionSystem GameObject alongside UnlockManager and InventoryManager.
/// For each entry, specify a category and the items to grant when that category first unlocks.
///
/// This is the "auto-grant on unlock" feature — when Cat_SakuraDecor unlocks at level 5,
/// the PinkBlock (and any other configured items) are immediately added to the inventory.
///
/// Usage:
///   1. Add this component to ProgressionSystem.
///   2. In the Inspector, add entries to the "Grants On Unlock" list.
///   3. For each entry, set the Category and add the items to grant.
/// </summary>
public class UnlockRewardGranter : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies")]
    [SerializeField] private UnlockManager   unlockManager;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Grants On Unlock")]
    [Tooltip("Each entry defines which items to grant when a specific category is unlocked.")]
    [SerializeField] private List<CategoryGrantEntry> grantsOnUnlock = new List<CategoryGrantEntry>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (unlockManager    == null) unlockManager    = FindAnyObjectByType<UnlockManager>();
        if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();
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
        foreach (CategoryGrantEntry entry in grantsOnUnlock)
        {
            if (entry.Category == null) continue;
            if (entry.Category.Id != category.Id) continue;

            foreach (PlaceableItem item in entry.ItemsToGrant)
            {
                if (item == null) continue;
                inventoryManager.AddItem(item, 1);
                Debug.Log($"[UnlockRewardGranter] Granted '{item.DisplayName}' to inventory " +
                          $"(category '{category.DisplayName}' unlocked).");
            }
        }
    }
}

// ── Supporting type ───────────────────────────────────────────────────────────

/// <summary>
/// Pairs a category with a list of items to grant when that category unlocks.
/// </summary>
[System.Serializable]
public class CategoryGrantEntry
{
    [Tooltip("The category that triggers the grant when it unlocks.")]
    public ItemCategory Category;

    [Tooltip("Items added to the player's inventory (qty 1 each) when the category unlocks.")]
    public List<PlaceableItem> ItemsToGrant = new List<PlaceableItem>();
}
