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
        
        public PlacementSaveData placementData = new PlacementSaveData();
        public InventorySaveData inventoryData = new InventorySaveData();
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
}
