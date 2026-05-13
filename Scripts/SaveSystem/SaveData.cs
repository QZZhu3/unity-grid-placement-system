using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlacementSystem.SaveSystem
{
    [Serializable]
    public class GameSaveData
    {
        public string version = "1.0";
        public long timestamp;

        public PlacementSaveData   placementData   = new PlacementSaveData();
        public InventorySaveData   inventoryData   = new InventorySaveData();

        /// <summary>Player level, XP, and achieved milestone IDs.</summary>
        public ProgressionSaveData progressionData = new ProgressionSaveData();

        /// <summary>Unlocked category and theme IDs.</summary>
        public UnlockSaveData      unlockData      = new UnlockSaveData();

        /// <summary>Chest task progress and pending chest queue.</summary>
        public ChestSaveData       chestData       = new ChestSaveData();
    }

    [Serializable]
    public class PlacementSaveData
    {
        public List<PlacedItemSaveData> items = new List<PlacedItemSaveData>();
    }

    [Serializable]
    public class PlacedItemSaveData
    {
        public string itemId;
        public Vector2Int gridPosition;
        public Vector2Int size;
        public int rotation;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventorySlotSaveData> slots = new List<InventorySlotSaveData>();
    }

    [Serializable]
    public class InventorySlotSaveData
    {
        public string itemId;
        public int quantity;
    }

    // -- Progression -----------------------------------------------------------

    /// <summary>
    /// Serializable snapshot of <see cref="PlayerProgressionManager"/> state.
    /// All references are stored as stable string IDs -- never as asset references.
    /// </summary>
    [Serializable]
    public class ProgressionSaveData
    {
        /// <summary>Current player level.</summary>
        public int level = 1;

        /// <summary>Current XP within the current level.</summary>
        public float xp = 0f;

        /// <summary>
        /// Stable IDs of all <see cref="ProgressionMilestone"/> assets that have been achieved.
        /// Populated from <see cref="ProgressionMilestone.Id"/>.
        /// </summary>
        public List<string> achievedMilestoneIds = new List<string>();
    }

    // -- Chest -----------------------------------------------------------------

    /// <summary>
    /// Serializable snapshot of chest progress and pending chest queue.
    /// Chest IDs are stored as stable string IDs only.
    /// </summary>
    [Serializable]
    public class ChestSaveData
    {
        /// <summary>Current task progress toward the next chest (0 to tasksPerChest-1).</summary>
        public int currentProgress = 0;

        /// <summary>
        /// Ordered list of chest definition IDs in the pending queue.
        /// First element is the next chest to be opened.
        /// </summary>
        public List<string> pendingChestIds = new List<string>();
    }

    // -- Unlock state ----------------------------------------------------------

    /// <summary>
    /// Serializable snapshot of <see cref="UnlockManager"/> state.
    /// All references are stored as stable string IDs -- never as asset references.
    /// </summary>
    [Serializable]
    public class UnlockSaveData
    {
        /// <summary>
        /// Stable IDs of all unlocked <see cref="ItemCategory"/> assets.
        /// Populated from <see cref="ItemCategory.Id"/>.
        /// </summary>
        public List<string> unlockedCategoryIds = new List<string>();

        /// <summary>
        /// Stable IDs of all unlocked <see cref="DecorationTheme"/> assets.
        /// Populated from <see cref="DecorationTheme.Id"/>.
        /// </summary>
        public List<string> unlockedThemeIds = new List<string>();
    }
}
