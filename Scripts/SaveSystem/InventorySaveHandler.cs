using System.Collections.Generic;
using UnityEngine;

namespace PlacementSystem.SaveSystem
{
    /// <summary>
    /// Implements ISaveable for the Inventory system.
    /// Serializes current inventory quantities and restores them on load.
    /// </summary>
    [RequireComponent(typeof(InventoryManager))]
    public class InventorySaveHandler : MonoBehaviour, ISaveable
    {
        private InventoryManager inventoryManager;

        private void Awake()
        {
            inventoryManager = GetComponent<InventoryManager>();
        }

        public void PopulateSaveData(GameSaveData data)
        {
            data.inventoryData.slots.Clear();

            foreach (var slot in inventoryManager.GetAllSlots())
            {
                data.inventoryData.slots.Add(new InventorySlotSaveData
                {
                    itemId = slot.Item.ItemId,
                    quantity = slot.Quantity
                });
            }
        }

        public void LoadFromSaveData(GameSaveData data)
        {
            inventoryManager.ClearInventory();

            // Build lookup of all available PlaceableItems
            PlaceableItem[] allItems = Resources.FindObjectsOfTypeAll<PlaceableItem>();
            Dictionary<string, PlaceableItem> itemLookup = new Dictionary<string, PlaceableItem>();
            foreach (var item in allItems)
            {
                itemLookup[item.ItemId] = item;
            }

            // Restore saved quantities
            foreach (var savedSlot in data.inventoryData.slots)
            {
                if (itemLookup.TryGetValue(savedSlot.itemId, out PlaceableItem dataAsset))
                {
                    inventoryManager.AddItem(dataAsset, savedSlot.quantity);
                }
                else
                {
                    Debug.LogWarning($"[InventorySaveHandler] Could not find item asset for ID: {savedSlot.itemId}");
                }
            }
        }
    }
}
