using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlacementSystem.SaveSystem
{
    /// <summary>
    /// Centralized Save/Load manager.
    /// Serializes data to JSON and coordinates with ISaveable systems.
    /// Designed to be easily expandable for cloud sync in the future.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [SerializeField] private string saveFileName = "savegame.json";
        
        // List of all systems that participate in save/load
        private List<ISaveable> saveables = new List<ISaveable>();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private void Awake()
        {
            // Auto-register all ISaveable components in the scene
            var foundSaveables = FindObjectsByType<MonoBehaviour>();
            foreach (var mono in foundSaveables)
            {
                if (mono is ISaveable saveable)
                {
                    saveables.Add(saveable);
                }
            }
        }

        public void SaveGame()
        {
            GameSaveData data = new GameSaveData
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 1. Gather data from all systems
            foreach (var saveable in saveables)
            {
                saveable.PopulateSaveData(data);
            }

            // 2. Serialize to JSON
            string json = JsonUtility.ToJson(data, true);

            // 3. Write to disk (or send to cloud)
            try
            {
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveManager] Game saved successfully to {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        public void LoadGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("[SaveManager] No save file found.");
                return;
            }

            try
            {
                // 1. Read JSON from disk (or cloud)
                string json = File.ReadAllText(SaveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                // 2. Distribute data to all systems
                foreach (var saveable in saveables)
                {
                    saveable.LoadFromSaveData(data);
                }

                Debug.Log("[SaveManager] Game loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
            }
        }
        
        // Editor utility for testing
        [ContextMenu("Force Save")]
        private void ForceSave() => SaveGame();

        [ContextMenu("Force Load")]
        private void ForceLoad() => LoadGame();
    }
}
