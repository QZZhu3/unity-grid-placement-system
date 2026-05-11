using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized static input gate for the game.
///
/// Any system can block or unblock input using a named reason string.
/// Input is considered blocked as long as at least one reason is active.
/// This prevents ordering bugs where one system unblocks before another is done.
///
/// Usage:
///   GameInputState.Block("chest_ui");
///   GameInputState.Unblock("chest_ui");
///   if (GameInputState.IsInputBlocked) return;
/// </summary>
public static class GameInputState
{
    // ── State ─────────────────────────────────────────────────────────────────

    private static readonly HashSet<string> activeBlockers = new HashSet<string>();

    /// <summary>True if any system has requested input to be blocked.</summary>
    public static bool IsInputBlocked => activeBlockers.Count > 0;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired whenever the blocked state changes.
    /// Arg: true = input is now blocked, false = input is now unblocked.
    /// </summary>
    public static event Action<bool> OnInputBlockChanged;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a named blocker. Input will be blocked until all blockers are removed.
    /// Safe to call multiple times with the same reason.
    /// </summary>
    public static void Block(string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            Debug.LogWarning("[GameInputState] Block called with null/empty reason.");
            return;
        }

        bool wasPreviouslyBlocked = IsInputBlocked;
        activeBlockers.Add(reason);

        if (!wasPreviouslyBlocked)
        {
            Debug.Log($"[GameInputState] Input BLOCKED by '{reason}'.");
            OnInputBlockChanged?.Invoke(true);
        }
    }

    /// <summary>
    /// Removes a named blocker. If no blockers remain, input is unblocked.
    /// Safe to call even if the reason was never added.
    /// </summary>
    public static void Unblock(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return;

        activeBlockers.Remove(reason);

        if (!IsInputBlocked)
        {
            Debug.Log($"[GameInputState] Input UNBLOCKED ('{reason}' removed).");
            OnInputBlockChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// Clears all blockers immediately. Use only in exceptional cases (e.g. scene reload).
    /// </summary>
    public static void ForceUnblockAll()
    {
        if (activeBlockers.Count == 0) return;
        activeBlockers.Clear();
        Debug.LogWarning("[GameInputState] All input blockers force-cleared.");
        OnInputBlockChanged?.Invoke(false);
    }

    /// <summary>Returns a debug string listing all active blockers.</summary>
    public static string GetBlockerList()
    {
        return activeBlockers.Count == 0
            ? "(none)"
            : string.Join(", ", activeBlockers);
    }
}
