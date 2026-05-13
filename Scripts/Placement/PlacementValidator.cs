using UnityEngine;

// -- ValidationResult ----------------------------------------------------------

/// <summary>
/// Immutable result returned by PlacementValidator.
/// Carries both the pass/fail flag and a human-readable reason for failure.
/// </summary>
public readonly struct ValidationResult
{
    public readonly bool   IsValid;
    public readonly string Reason;

    public static readonly ValidationResult Valid =
        new ValidationResult(true, string.Empty);

    public ValidationResult(bool isValid, string reason)
    {
        IsValid = isValid;
        Reason  = reason;
    }

    public static ValidationResult Fail(string reason) =>
        new ValidationResult(false, reason);
}

// -- PlacementValidator --------------------------------------------------------

/// <summary>
/// Stateless service that validates whether a placement is legal.
/// Queries GridManager as the single source of truth.
///
/// This class is intentionally stateless -- it holds no data between calls.
/// Attach it to any GameObject; it requires a GridManager reference.
/// </summary>
public class PlacementValidator : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();
    }

    /// <summary>
    /// Validates whether an item of the given size can be placed at the given grid position.
    /// </summary>
    /// <param name="gridPos">Top-left cell of the item's footprint.</param>
    /// <param name="size">Width (x) and depth (y) of the item in grid cells.</param>
    /// <param name="excludeGridPos">
    ///   Optional: the item's current grid position to exclude from occupancy checks.
    ///   Pass when moving an already-placed item so it doesn't block itself.
    /// </param>
    public ValidationResult Validate(Vector2Int gridPos, Vector2Int size,
                                     Vector2Int? excludeGridPos = null)
    {
        // 1. Bounds check
        if (!gridManager.IsAreaWithinBounds(gridPos, size))
            return ValidationResult.Fail("Out of bounds");

        // 2. Occupancy check (with optional self-exclusion for pick-up-and-move)
        if (excludeGridPos.HasValue)
        {
            if (!IsAreaAvailableExcluding(gridPos, size, excludeGridPos.Value,
                    GetExcludeSize(excludeGridPos.Value)))
                return ValidationResult.Fail("Area occupied");
        }
        else
        {
            if (!gridManager.IsAreaAvailable(gridPos, size))
                return ValidationResult.Fail("Area occupied");
        }

        return ValidationResult.Valid;
    }

    /// <summary>
    /// Checks area availability while ignoring cells belonging to the excluded item.
    /// Used when an item is being moved -- its own footprint must not block itself.
    /// </summary>
    private bool IsAreaAvailableExcluding(Vector2Int pos, Vector2Int size,
                                          Vector2Int excludePos, Vector2Int excludeSize)
    {
        if (!gridManager.IsAreaWithinBounds(pos, size)) return false;

        for (int x = pos.x; x < pos.x + size.x; x++)
        {
            for (int z = pos.y; z < pos.y + size.y; z++)
            {
                Vector2Int cell = new Vector2Int(x, z);

                // Skip cells that belong to the excluded item's footprint
                bool inExclude = x >= excludePos.x && x < excludePos.x + excludeSize.x &&
                                 z >= excludePos.y && z < excludePos.y + excludeSize.y;
                if (inExclude) continue;

                if (gridManager.IsCellOccupied(cell)) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Retrieves the size of the item currently registered at a grid position.
    /// Returns Vector2Int.one as a safe fallback.
    /// </summary>
    private Vector2Int GetExcludeSize(Vector2Int pos)
    {
        PlacedItem item = gridManager.GetPlacedItem(pos);
        return item != null ? item.Size : Vector2Int.one;
    }
}
