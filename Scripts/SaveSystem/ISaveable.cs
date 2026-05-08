namespace PlacementSystem.SaveSystem
{
    /// <summary>
    /// Interface for any system that needs to save or load data.
    /// Keeps SaveManager decoupled from specific managers.
    /// </summary>
    public interface ISaveable
    {
        void PopulateSaveData(GameSaveData data);
        void LoadFromSaveData(GameSaveData data);
    }
}
