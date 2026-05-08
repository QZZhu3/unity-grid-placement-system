using System.Collections.Generic;
using UnityEngine;

namespace PlacementSystem.SaveSystem
{
    /// <summary>
    /// Implements ISaveable for the Placement system.
    /// Gathers all placed items from GridManager to save.
    /// Instantiates and registers items on load.
    /// </summary>
    [RequireComponent(typeof(GridManager))]
    public class PlacementSaveHandler : MonoBehaviour, ISaveable
    {
        private GridManager gridManager;

        private void Awake()
        {
            gridManager = GetComponent<GridManager>();
        }

        public void PopulateSaveData(GameSaveData data)
        {
            data.placementData.items.Clear();

            // Note: We intentionally do NOT save the currently dragged item.
            // If the game is saved mid-drag, the item is considered unplaced
            // and should safely remain in the inventory.

            foreach (var kvp in gridManager.GetAllPlacedItems())
            {
                PlacedItem item = kvp.Value;
                data.placementData.items.Add(new PlacedItemSaveData
                {
                    itemId = item.ItemId,
                    gridPosition = item.GridPosition,
                    size = item.Size,
                    rotation = item.Rotation
                });
            }
        }

        public void LoadFromSaveData(GameSaveData data)
        {
            // 1. Clear existing world state
            ClearCurrentPlacement();

            // 2. Load and instantiate saved items
            PlaceableItem[] allItems = Resources.FindObjectsOfTypeAll<PlaceableItem>();
            Dictionary<string, PlaceableItem> itemLookup = new Dictionary<string, PlaceableItem>();
            foreach (var item in allItems)
            {
                itemLookup[item.ItemId] = item;
            }

            foreach (var savedItem in data.placementData.items)
            {
                if (!itemLookup.TryGetValue(savedItem.itemId, out PlaceableItem dataAsset))
                {
                    Debug.LogWarning($"[PlacementSaveHandler] Could not find item asset for ID: {savedItem.itemId}");
                    continue;
                }

                // Instantiate prefab
                GameObject go = Instantiate(dataAsset.Prefab);
                
                // Set position and rotation
                go.transform.position = gridManager.GridToWorld(savedItem.gridPosition);
                go.transform.rotation = Quaternion.Euler(0, savedItem.rotation, 0);

                // Create data record
                PlacedItem placed = new PlacedItem(
                    savedItem.itemId,
                    savedItem.gridPosition,
                    savedItem.size,
                    savedItem.rotation,
                    go
                );

                // Register with GridManager
                gridManager.MarkAreaOccupied(savedItem.gridPosition, savedItem.size);
                gridManager.RegisterPlacedItem(savedItem.gridPosition, placed);
            }
        }

        private void ClearCurrentPlacement()
        {
            foreach (var kvp in gridManager.GetAllPlacedItems())
            {
                if (kvp.Value.GameObject != null)
                {
                    Destroy(kvp.Value.GameObject);
                }
            }
            // A hypothetical Clear() method on GridManager would be cleaner,
            // but for now we'll just clear the dictionary and occupancy array manually.
            // In a real implementation, we should add `ClearGrid()` to GridManager.
            
            // For now, we rely on the fact that GridManager's dictionaries are 
            // populated dynamically, but we need to ensure the occupancy grid is wiped.
            // The safest way without modifying GridManager is to unregister one by one:
            var allItems = new List<PlacedItem>(gridManager.GetAllPlacedItems().Values);
            foreach (var item in allItems)
            {
                gridManager.MarkAreaUnoccupied(item.GridPosition, item.Size);
                gridManager.UnregisterPlacedItem(item.GridPosition);
            }
        }
    }
}
