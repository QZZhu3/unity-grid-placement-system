/// <summary>
/// Identifies the ambient interaction shown during a Focus Session.
///
/// These are purely cosmetic, relaxing interactions with no fail state.
/// Each type maps to a corresponding AmbientInteractionController subclass
/// or a simple particle/animation system.
///
/// Adding a new ambient type:
///   1. Add a value here.
///   2. Create or extend an AmbientInteractionController to handle it.
///   3. Assign the new type in a FocusSessionDefinition asset.
/// </summary>
public enum AmbientInteractionType
{
    /// <summary>No ambient interaction -- timer only.</summary>
    None,

    /// <summary>Gentle drifting leaves float across the screen.</summary>
    DriftingLeaves,

    /// <summary>Player can optionally tap to water flowers in the garden.</summary>
    WateringFlowers,

    /// <summary>Player can tap to light small lanterns around the garden.</summary>
    LightingLanterns,

    /// <summary>Soft fireflies drift and glow -- tap to collect (no penalty for missing).</summary>
    FireflyInteraction,
}
