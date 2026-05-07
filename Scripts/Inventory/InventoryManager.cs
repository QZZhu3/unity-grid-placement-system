using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's inventory, storing PlaceableItems with quantities.
/// Pure data and logic layer. Fires events when inventory changes.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<InventorySlot> startingItems = new List<InventorySlot>();

    private Dictionary<string, InventorySlot> inventory = new Dictionary<string, InventorySlot>();

    // Events
    public delegate void InventoryChangedDelegate(string itemId, int newQuantity);
    public event InventoryChangedDelegate OnInventoryChanged;
    public event System.Action OnInventoryRefreshed;

    private void Awake()
    {
        InitializeInventory();
    }

    /// <summary>
    /// Populates the inventory from the starting items list defined in the Inspector.
    /// </summary>
    private void InitializeInventory()
    {
        inventory.Clear();
        foreach (InventorySlot slot in startingItems)
        {
            if (slot.Item != null)
            {
                inventory[slot.Item.ItemId] = new InventorySlot(slot.Item, slot.Quantity);
            }
        }
        OnInventoryRefreshed?.Invoke();
    }

    /// <summary>
    /// Adds a quantity of an item to the inventory.
    /// </summary>
    public void AddItem(PlaceableItem item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return;

        if (inventory.ContainsKey(item.ItemId))
        {
            inventory[item.ItemId].Quantity += quantity;
        }
        else
        {
            inventory[item.ItemId] = new InventorySlot(item, quantity);
        }

        OnInventoryChanged?.Invoke(item.ItemId, inventory[item.ItemId].Quantity);
    }

    /// <summary>
    /// Removes a quantity of an item from the inventory.
    /// Returns true if successful, false if insufficient quantity.
    /// </summary>
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (!inventory.ContainsKey(itemId)) return false;

        InventorySlot slot = inventory[itemId];
        if (slot.Quantity < quantity) return false;

        slot.Quantity -= quantity;
        OnInventoryChanged?.Invoke(itemId, slot.Quantity);
        return true;
    }

    /// <summary>
    /// Checks if the inventory has at least the specified quantity of an item.
    /// </summary>
    public bool HasItem(string itemId, int quantity = 1)
    {
        return inventory.ContainsKey(itemId) && inventory[itemId].Quantity >= quantity;
    }

    /// <summary>
    /// Gets the current quantity of an item.
    /// </summary>
    public int GetQuantity(string itemId)
    {
        return inventory.ContainsKey(itemId) ? inventory[itemId].Quantity : 0;
    }

    /// <summary>
    /// Gets all inventory slots as a read-only list.
    /// </summary>
    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(inventory.Values);
    }

    /// <summary>
    /// Gets a specific inventory slot by item ID.
    /// </summary>
    public InventorySlot GetSlot(string itemId)
    {
        return inventory.ContainsKey(itemId) ? inventory[itemId] : null;
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryRefreshed?.Invoke();
    }
}
