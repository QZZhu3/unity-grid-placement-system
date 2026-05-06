using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the grid system for placement, including world-to-grid conversion,
/// occupancy tracking, and area validation.
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    private bool[,] occupancyGrid;
    private Dictionary<Vector2Int, PlacedItem> placedItems;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;

    private void Awake()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Initializes the occupancy grid and placed items dictionary.
    /// </summary>
    private void InitializeGrid()
    {
        occupancyGrid = new bool[gridWidth, gridHeight];
        placedItems = new Dictionary<Vector2Int, PlacedItem>();
    }

    /// <summary>
    /// Converts world position to grid coordinates.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        int gridX = Mathf.RoundToInt(localPosition.x / cellSize);
        int gridZ = Mathf.RoundToInt(localPosition.z / cellSize);
        return new Vector2Int(gridX, gridZ);
    }

    /// <summary>
    /// Converts grid coordinates to world position (center of cell).
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        Vector3 worldPosition = gridOrigin;
        worldPosition.x += gridPosition.x * cellSize + cellSize * 0.5f;
        worldPosition.z += gridPosition.y * cellSize + cellSize * 0.5f;
        return worldPosition;
    }

    /// <summary>
    /// Checks if a grid position is within bounds.
    /// </summary>
    public bool IsWithinBounds(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    /// <summary>
    /// Checks if a rectangular area is within bounds.
    /// </summary>
    public bool IsAreaWithinBounds(Vector2Int gridPosition, Vector2Int size)
    {
        return gridPosition.x >= 0 && gridPosition.x + size.x <= gridWidth &&
               gridPosition.y >= 0 && gridPosition.y + size.y <= gridHeight;
    }

    /// <summary>
    /// Checks if a specific cell is occupied.
    /// </summary>
    public bool IsCellOccupied(Vector2Int gridPosition)
    {
        if (!IsWithinBounds(gridPosition))
            return true; // Treat out-of-bounds as occupied

        return occupancyGrid[gridPosition.x, gridPosition.y];
    }

    /// <summary>
    /// Checks if an entire rectangular area is available for placement.
    /// </summary>
    public bool IsAreaAvailable(Vector2Int gridPosition, Vector2Int size)
    {
        if (!IsAreaWithinBounds(gridPosition, size))
            return false;

        for (int x = gridPosition.x; x < gridPosition.x + size.x; x++)
        {
            for (int z = gridPosition.y; z < gridPosition.y + size.y; z++)
            {
                if (occupancyGrid[x, z])
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Marks an area as occupied on the grid.
    /// </summary>
    public void MarkAreaOccupied(Vector2Int gridPosition, Vector2Int size)
    {
        for (int x = gridPosition.x; x < gridPosition.x + size.x; x++)
        {
            for (int z = gridPosition.y; z < gridPosition.y + size.y; z++)
            {
                occupancyGrid[x, z] = true;
            }
        }
    }

    /// <summary>
    /// Marks an area as unoccupied on the grid.
    /// </summary>
    public void MarkAreaUnoccupied(Vector2Int gridPosition, Vector2Int size)
    {
        for (int x = gridPosition.x; x < gridPosition.x + size.x; x++)
        {
            for (int z = gridPosition.y; z < gridPosition.y + size.y; z++)
            {
                occupancyGrid[x, z] = false;
            }
        }
    }

    /// <summary>
    /// Registers a placed item in the tracking dictionary.
    /// </summary>
    public void RegisterPlacedItem(Vector2Int gridPosition, PlacedItem item)
    {
        placedItems[gridPosition] = item;
    }

    /// <summary>
    /// Unregisters a placed item from the tracking dictionary.
    /// </summary>
    public void UnregisterPlacedItem(Vector2Int gridPosition)
    {
        if (placedItems.ContainsKey(gridPosition))
            placedItems.Remove(gridPosition);
    }

    /// <summary>
    /// Gets a placed item at a specific grid position.
    /// </summary>
    public PlacedItem GetPlacedItem(Vector2Int gridPosition)
    {
        return placedItems.ContainsKey(gridPosition) ? placedItems[gridPosition] : null;
    }

    /// <summary>
    /// Gets all placed items.
    /// </summary>
    public Dictionary<Vector2Int, PlacedItem> GetAllPlacedItems()
    {
        return new Dictionary<Vector2Int, PlacedItem>(placedItems);
    }

    /// <summary>
    /// Clears all placements and resets the grid.
    /// </summary>
    public void ClearGrid()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Visualizes the grid in the scene editor for debugging (only when selected).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.white;
        
        // Draw grid lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = gridOrigin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = gridOrigin + new Vector3(x * cellSize, 0, gridHeight * cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = gridOrigin + new Vector3(0, 0, z * cellSize);
            Vector3 end = gridOrigin + new Vector3(gridWidth * cellSize, 0, z * cellSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw occupied cells
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (occupancyGrid[x, z])
                {
                    Vector3 cellCenter = GridToWorld(new Vector2Int(x, z));
                    Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
            }
        }
    }
}

/// <summary>
/// Represents a placed item on the grid.
/// </summary>
public class PlacedItem
{
    public string ItemId { get; set; }
    public Vector2Int GridPosition { get; set; }
    public Vector2Int Size { get; set; }
    public int Rotation { get; set; } // 0, 90, 180, 270
    public GameObject GameObject { get; set; }

    public PlacedItem(string itemId, Vector2Int gridPosition, Vector2Int size, int rotation, GameObject gameObject)
    {
        ItemId = itemId;
        GridPosition = gridPosition;
        Size = size;
        Rotation = rotation;
        GameObject = gameObject;
    }
}
