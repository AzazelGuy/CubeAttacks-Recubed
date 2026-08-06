using System.IO;
using System;
using UnityEngine;

public static class SaveSystem
{
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    /// <summary>
    /// Saves the current PlayerSaveData instance to JSON file on disk.
    /// </summary>
    public static void SaveGame(PlayerSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveSystem] Game saved successfully to: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// Loads PlayerSaveData from disk. Creates a default save if none exists.
    /// </summary>
    public static PlayerSaveData LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("[SaveSystem] No save file found. Creating new save data.");
            PlayerSaveData newSave = new PlayerSaveData();
            SaveGame(newSave);
            return newSave;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
            Debug.Log("[SaveSystem] Game loaded successfully.");
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load save file: {e.Message}. Creating fresh default save.");
            return new PlayerSaveData();
        }
    }

    /// <summary>
    /// Deletes save file (useful for full game resets / debugging).
    /// </summary>
    public static void ResetSaveData()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("[SaveSystem] Save file deleted.");
        }
    }
}