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

        private List<ISaveable> saveables = new List<ISaveable>();
        private PlacementManager placementManager;

        public string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
        public bool HasSaveFile => File.Exists(SaveFilePath);

        private void Awake()
        {
            placementManager = FindObjectsByType<PlacementManager>()[0];

            // Auto-register all ISaveable components in the scene
            var foundSaveables = FindObjectsByType<MonoBehaviour>();
            foreach (var mono in foundSaveables)
            {
                if (mono is ISaveable saveable)
                    saveables.Add(saveable);
            }
        }

        public void SaveGame()
        {
            GameSaveData data = new GameSaveData
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            foreach (var saveable in saveables)
                saveable.PopulateSaveData(data);

            string json = JsonUtility.ToJson(data, true);

            try
            {
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveManager] Game saved to {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        public void LoadGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("[SaveManager] No save file found.");
                return;
            }

            // Cancel any active drag before loading to prevent ghost objects
            if (placementManager != null && placementManager.IsDragging)
            {
                placementManager.CancelPlacement();
                Debug.Log("[SaveManager] Cancelled active drag before loading.");
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                foreach (var saveable in saveables)
                    saveable.LoadFromSaveData(data);

                Debug.Log("[SaveManager] Game loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            }
        }

        [ContextMenu("Force Save")]
        private void ForceSave() => SaveGame();

        [ContextMenu("Force Load")]
        private void ForceLoad() => LoadGame();
    }
}
