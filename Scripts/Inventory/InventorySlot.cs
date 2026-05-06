using UnityEngine;

/// <summary>
/// Represents a single slot in the inventory, pairing a PlaceableItem with a quantity.
/// Serializable so it can be configured directly in the Unity Inspector.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [SerializeField] private PlaceableItem item;
    [SerializeField] private int quantity;

    public PlaceableItem Item => item;

    public int Quantity
    {
        get => quantity;
        set => quantity = Mathf.Max(0, value); // Quantity can never go below 0
    }

    public bool IsEmpty => quantity <= 0;

    public InventorySlot(PlaceableItem item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Max(0, quantity);
    }
}
