using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Automatically grants items to the player's inventory the FIRST TIME a category unlocks.
///
/// Uses a HashSet to track which categories have already been granted, so items are
/// never awarded twice -- even across save/load cycles or multiple Play sessions.
///
/// Attach this to the ProgressionSystem GameObject.
/// InventoryManager and UnlockManager are auto-discovered at startup.
///
/// Setup:
///   1. Add this component to ProgressionSystem.
///   2. In the Inspector, add entries to "Grants On Unlock".
///   3. For each entry, set the Category and add item+quantity pairs.
/// </summary>
public class UnlockRewardGranter : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private UnlockManager    unlockManager;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Grants On Unlock")]
    [Tooltip("Each entry defines which items (and how many) to grant when a specific category unlocks.")]
    [SerializeField] private List<CategoryGrantEntry> grantsOnUnlock = new List<CategoryGrantEntry>();

    // -- Runtime state ---------------------------------------------------------

    /// <summary>
    /// Tracks category IDs that have already been granted this session.
    /// Prevents re-granting when the save system re-fires unlock events on load.
    /// </summary>
    private HashSet<string> alreadyGranted = new HashSet<string>();

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
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

    // -- Event handler ---------------------------------------------------------

    private void HandleCategoryUnlocked(ItemCategory category)
    {
        if (inventoryManager == null) return;

        // Skip if we have already processed grants for this category this session.
        // This prevents re-granting when EvaluateUnlocks() re-fires on load.
        if (alreadyGranted.Contains(category.Id)) return;
        alreadyGranted.Add(category.Id);

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

    // -- Save integration ------------------------------------------------------

    /// <summary>
    /// Call this after loading a save to pre-populate the already-granted set
    /// with categories that were unlocked in a previous session.
    /// This prevents re-granting items the player already received.
    /// </summary>
    public void MarkCategoriesAsAlreadyGranted(IEnumerable<string> categoryIds)
    {
        foreach (string id in categoryIds)
            alreadyGranted.Add(id);
    }
}

// -- Supporting types ----------------------------------------------------------

[System.Serializable]
public class CategoryGrantEntry
{
    [Tooltip("The category whose unlock triggers these grants.")]
    public ItemCategory Category;

    [Tooltip("Items and quantities to add to inventory when this category unlocks.")]
    public List<ItemGrantEntry> ItemsToGrant = new List<ItemGrantEntry>();
}

[System.Serializable]
public class ItemGrantEntry
{
    [Tooltip("The PlaceableItem to add to the player's inventory.")]
    public PlaceableItem Item;

    [Tooltip("How many of this item to grant. Minimum 1.")]
    [Min(1)]
    public int Quantity = 1;
}
