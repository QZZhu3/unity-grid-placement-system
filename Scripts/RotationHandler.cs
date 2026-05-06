using UnityEngine;

/// <summary>
/// Utility class for handling rotation calculations and transformations.
/// Supports 0°, 90°, 180°, 270° rotations with proper footprint adjustment.
/// </summary>
public static class RotationHandler
{
    /// <summary>
    /// Valid rotation angles (in degrees).
    /// </summary>
    public static readonly int[] ValidRotations = { 0, 90, 180, 270 };

    /// <summary>
    /// Gets the next rotation angle in the cycle.
    /// </summary>
    public static int GetNextRotation(int currentRotation)
    {
        for (int i = 0; i < ValidRotations.Length; i++)
        {
            if (ValidRotations[i] == currentRotation)
            {
                return ValidRotations[(i + 1) % ValidRotations.Length];
            }
        }
        return 0;
    }

    /// <summary>
    /// Converts rotation angle to quaternion.
    /// </summary>
    public static Quaternion AngleToQuaternion(int rotationAngle)
    {
        return Quaternion.Euler(0, rotationAngle, 0);
    }

    /// <summary>
    /// Converts quaternion to rotation angle (0, 90, 180, or 270).
    /// </summary>
    public static int QuaternionToAngle(Quaternion rotation)
    {
        float yRotation = rotation.eulerAngles.y;
        
        // Normalize to 0-360 range
        if (yRotation < 0)
            yRotation += 360;

        // Find closest valid rotation
        int closestRotation = 0;
        float closestDifference = 360;

        foreach (int validRotation in ValidRotations)
        {
            float difference = Mathf.Abs(yRotation - validRotation);
            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestRotation = validRotation;
            }
        }

        return closestRotation;
    }

    /// <summary>
    /// Calculates the adjusted footprint size based on rotation.
    /// For 90° and 270° rotations, width and height are swapped.
    /// </summary>
    public static Vector2Int GetAdjustedSize(Vector2Int originalSize, int rotationAngle)
    {
        if (rotationAngle == 90 || rotationAngle == 270)
        {
            return new Vector2Int(originalSize.y, originalSize.x);
        }
        return originalSize;
    }

    /// <summary>
    /// Rotates a 2D point around the origin by the specified angle.
    /// Used for calculating local positions during rotation.
    /// </summary>
    public static Vector2 RotatePoint(Vector2 point, int rotationAngle)
    {
        float radians = rotationAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos
        );
    }

    /// <summary>
    /// Calculates the grid offset adjustment when rotating an item.
    /// This ensures the item rotates around its center, not the corner.
    /// </summary>
    public static Vector2Int GetRotationOffsetAdjustment(Vector2Int originalSize, int fromRotation, int toRotation)
    {
        Vector2Int originalAdjusted = GetAdjustedSize(originalSize, fromRotation);
        Vector2Int newAdjusted = GetAdjustedSize(originalSize, toRotation);

        // Calculate center offset difference
        Vector2 originalCenter = new Vector2(originalAdjusted.x - 1, originalAdjusted.y - 1) * 0.5f;
        Vector2 newCenter = new Vector2(newAdjusted.x - 1, newAdjusted.y - 1) * 0.5f;

        return Vector2Int.FloorToInt(newCenter - originalCenter);
    }

    /// <summary>
    /// Validates if a rotation angle is valid.
    /// </summary>
    public static bool IsValidRotation(int rotationAngle)
    {
        foreach (int valid in ValidRotations)
        {
            if (valid == rotationAngle)
                return true;
        }
        return false;
    }
}
