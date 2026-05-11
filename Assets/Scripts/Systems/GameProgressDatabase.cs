using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameProgressDatabase
{
    private const string FileName = "game_progress.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Save(GameProgressData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static GameProgressData Load()
    {
        if (!File.Exists(SavePath))
            return GameProgressData.CreateDefault();

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
            return GameProgressData.CreateDefault();

        GameProgressData data = JsonUtility.FromJson<GameProgressData>(json);
        return data ?? GameProgressData.CreateDefault();
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static void SaveCurrentProgress(
        string ultimaMisionCompletada,
        string misionActual,
        bool bossCompleted,
        bool puzzleCompleted)
    {
        GameProgressData data = new GameProgressData
        {
            Scene = SceneManager.GetActiveScene().name,
            UltimaMisionCompletada = ultimaMisionCompletada ?? string.Empty,
            MisionActual = misionActual ?? string.Empty,
            BossCompleted = bossCompleted,
            PuzzleCompleted = puzzleCompleted,
            CollectedItemIds = new System.Collections.Generic.List<string>()
        };

        Save(data);
    }

    public static void SaveSceneAndSpawn(string sceneName, Vector3 spawnPosition)
    {
        GameProgressData data = Load();
        data.Scene = sceneName ?? string.Empty;
        data.SetSpawnPosition(spawnPosition);
        Save(data);
    }

    public static void SaveSceneKeepingSpawn(string sceneName)
    {
        GameProgressData data = Load();
        data.Scene = sceneName ?? string.Empty;
        Save(data);
    }

    public static void AddCollectedItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;

        GameProgressData data = Load();
        if (data.CollectedItemIds == null)
            data.CollectedItemIds = new System.Collections.Generic.List<string>();

        if (!data.CollectedItemIds.Contains(itemId))
        {
            data.CollectedItemIds.Add(itemId);
            Save(data);
        }
    }

    public static bool HasCollectedItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;

        GameProgressData data = Load();
        return data.CollectedItemIds != null && data.CollectedItemIds.Contains(itemId);
    }

    public static System.Collections.Generic.IReadOnlyList<string> GetCollectedItems()
    {
        GameProgressData data = Load();
        if (data.CollectedItemIds == null)
            data.CollectedItemIds = new System.Collections.Generic.List<string>();

        return data.CollectedItemIds;
    }
}
