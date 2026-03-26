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
            PuzzleCompleted = puzzleCompleted
        };

        Save(data);
    }
}
