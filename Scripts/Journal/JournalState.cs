/// <summary>
/// Represents the three core states of the Ambient Task Journal.
/// </summary>
public enum JournalState
{
    /// <summary>Fully hidden off-screen.</summary>
    Hidden,

    /// <summary>
    /// Partially slid/faded into view (e.g. mouse hover near edge).
    /// Shows only minimal info: icon, title, tiny progress.
    /// </summary>
    Peek,

    /// <summary>
    /// Fully expanded and interactive.
    /// Background is blurred/darkened.
    /// </summary>
    Pinned
}
