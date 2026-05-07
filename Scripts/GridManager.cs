using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Owns the occupancy grid, world↔grid coordinate conversion, and
/// placed-item registration. This is the single source of truth for
/// what cells are free or occupied.
///
/// GridManager has no knowledge of input, UI, dragging, or validation logic.
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    private bool[,] occupancyGrid;
    private Dictionary<Vector2Int, PlacedItem> placedItems = new Dictionary<Vector2Int, PlacedItem>();

    // ── Public read-only properties ───────────────────────────────────────────
    public int GridWidth  => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;

    private void Awake() => InitializeGrid();

    private void InitializeGrid()
    {
        occupancyGrid = new bool[gridWidth, gridHeight];
        placedItems   = new Dictionary<Vector2Int, PlacedItem>();
    }

    // ── Coordinate conversion ─────────────────────────────────────────────────

    /// <summary>
    /// Converts a world position to the nearest grid cell coordinate.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int z = Mathf.FloorToInt(local.z / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Returns the world-space centre of a grid cell.
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(
            gridPos.x * cellSize + cellSize * 0.5f,
            0f,
            gridPos.y * cellSize + cellSize * 0.5f
        );
    }

    // ── Bounds checks ─────────────────────────────────────────────────────────

    public bool IsWithinBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;

    public bool IsAreaWithinBounds(Vector2Int pos, Vector2Int size) =>
        pos.x >= 0 && pos.x + size.x <= gridWidth &&
        pos.y >= 0 && pos.y + size.y <= gridHeight;

    // ── Occupancy queries ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the cell is occupied or out of bounds.
    /// </summary>
    public bool IsCellOccupied(Vector2Int pos)
    {
        if (!IsWithinBounds(pos)) return true;
        return occupancyGrid[pos.x, pos.y];
    }

    /// <summary>
    /// Returns true if every cell in the rectangular area is free and in bounds.
    /// </summary>
    public bool IsAreaAvailable(Vector2Int pos, Vector2Int size)
    {
        if (!IsAreaWithinBounds(pos, size)) return false;

        for (int x = pos.x; x < pos.x + size.x; x++)
            for (int z = pos.y; z < pos.y + size.y; z++)
                if (occupancyGrid[x, z]) return false;

        return true;
    }

    // ── Occupancy mutation ────────────────────────────────────────────────────

    public void MarkAreaOccupied(Vector2Int pos, Vector2Int size)
    {
        for (int x = pos.x; x < pos.x + size.x; x++)
            for (int z = pos.y; z < pos.y + size.y; z++)
                if (IsWithinBounds(new Vector2Int(x, z)))
                    occupancyGrid[x, z] = true;
    }

    public void MarkAreaUnoccupied(Vector2Int pos, Vector2Int size)
    {
        for (int x = pos.x; x < pos.x + size.x; x++)
            for (int z = pos.y; z < pos.y + size.y; z++)
                if (IsWithinBounds(new Vector2Int(x, z)))
                    occupancyGrid[x, z] = false;
    }

    // ── PlacedItem registry ───────────────────────────────────────────────────

    public void RegisterPlacedItem(Vector2Int pos, PlacedItem item)   => placedItems[pos] = item;
    public void UnregisterPlacedItem(Vector2Int pos)                  => placedItems.Remove(pos);
    public PlacedItem GetPlacedItem(Vector2Int pos)                   => placedItems.TryGetValue(pos, out var item) ? item : null;
    public Dictionary<Vector2Int, PlacedItem> GetAllPlacedItems()     => new Dictionary<Vector2Int, PlacedItem>(placedItems);

    public void ClearGrid() => InitializeGrid();

    // ── Editor Gizmos ─────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Grid lines (always visible in editor)
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int x = 0; x <= gridWidth; x++)
        {
            Gizmos.DrawLine(
                gridOrigin + new Vector3(x * cellSize, 0.01f, 0),
                gridOrigin + new Vector3(x * cellSize, 0.01f, gridHeight * cellSize));
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Gizmos.DrawLine(
                gridOrigin + new Vector3(0, 0.01f, z * cellSize),
                gridOrigin + new Vector3(gridWidth * cellSize, 0.01f, z * cellSize));
        }

        // Occupied cells (play mode only)
        if (!Application.isPlaying || occupancyGrid == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        for (int x = 0; x < gridWidth; x++)
            for (int z = 0; z < gridHeight; z++)
                if (occupancyGrid[x, z])
                    Gizmos.DrawCube(GridToWorld(new Vector2Int(x, z)),
                        new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));
    }
}

// ── PlacedItem data class ──────────────────────────────────────────────────────

/// <summary>
/// Record of an item that has been placed on the grid.
///
/// OccupiedCells is stored explicitly (not recomputed from Size) so that:
///   1. Non-rectangular footprints are supported in future.
///   2. Save/load can serialise the exact cell list without recomputation.
///   3. Pick-up-and-move can free the correct cells even after rotation.
/// </summary>
public class PlacedItem
{
    public string             ItemId        { get; }
    public Vector2Int         GridPosition  { get; set; }   // mutable for pick-up-and-move
    public Vector2Int         Size          { get; set; }   // mutable for rotation changes
    public int                Rotation      { get; set; }
    public GameObject         GameObject    { get; }

    /// <summary>
    /// Every grid cell this item currently occupies.
    /// Rebuilt automatically in the constructor and via RefreshOccupiedCells().
    /// </summary>
    public List<Vector2Int>   OccupiedCells { get; private set; }

    public PlacedItem(string itemId, Vector2Int gridPosition, Vector2Int size, int rotation, GameObject go)
    {
        ItemId        = itemId;
        GridPosition  = gridPosition;
        Size          = size;
        Rotation      = rotation;
        GameObject    = go;
        OccupiedCells = ComputeCells(gridPosition, size);
    }

    /// <summary>
    /// Recomputes OccupiedCells after GridPosition or Size changes.
    /// Call this whenever the item is moved or rotated.
    /// </summary>
    public void RefreshOccupiedCells()
    {
        OccupiedCells = ComputeCells(GridPosition, Size);
    }

    /// <summary>
    /// Builds the list of all cells covered by a rectangular footprint.
    /// Override this method in a subclass to support non-rectangular shapes.
    /// </summary>
    public static List<Vector2Int> ComputeCells(Vector2Int origin, Vector2Int size)
    {
        var cells = new List<Vector2Int>(size.x * size.y);
        for (int x = origin.x; x < origin.x + size.x; x++)
            for (int z = origin.y; z < origin.y + size.y; z++)
                cells.Add(new Vector2Int(x, z));
        return cells;
    }
}
