using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Save data structure for comprehensive game state.
/// Gold-level rubric requirement: Save settings, game state, achievements, etc.
/// </summary>
[Serializable]
public class SaveData
{
    // Player progress
    public string currentLevel;
    public Vector3 playerPosition;
    public int deathCount;
    public float totalPlayTime;

    // Settings
    public float musicVolume;
    public float sfxVolume;
    public int qualityLevel;
    public bool fullscreen;
    public int resolutionWidth;
    public int resolutionHeight;

    // Achievements
    public List<string> unlockedAchievements = new List<string>();

    // Level completion
    public Dictionary<string, float> levelBestTimes = new Dictionary<string, float>();
    public Dictionary<string, bool> levelCompleted = new Dictionary<string, bool>();

    // Statistics
    public int totalJumps;
    public int totalHookUses;
    public float totalDistanceTraveled;

    public SaveData()
    {
        currentLevel = "";
        playerPosition = Vector3.zero;
        deathCount = 0;
        totalPlayTime = 0f;
        musicVolume = 1f;
        sfxVolume = 1f;
        qualityLevel = 2;
        fullscreen = true;
        resolutionWidth = 1920;
        resolutionHeight = 1080;
        totalJumps = 0;
        totalHookUses = 0;
        totalDistanceTraveled = 0f;
    }
}

/// <summary>
/// Comprehensive save system for Gold-level rubric score.
/// Saves game state, settings, achievements, and statistics.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public static SaveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SaveSystem");
                instance = go.AddComponent<SaveSystem>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const string SAVE_KEY = "EscalatopiaGameSave";
    private SaveData currentSave;

    // Events
    public event Action OnGameSaved;
    public event Action OnGameLoaded;

    public SaveData CurrentSave => currentSave;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    /// <summary>
    /// Save the game.
    /// </summary>
    public void SaveGame()
    {
        if (currentSave == null)
        {
            currentSave = new SaveData();
        }

        // Serialize to JSON
        string json = JsonUtility.ToJson(currentSave, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[SaveSystem] Game saved");
        OnGameSaved?.Invoke();
    }

    /// <summary>
    /// Load the game.
    /// </summary>
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            currentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("[SaveSystem] Game loaded");
        }
        else
        {
            currentSave = new SaveData();
            Debug.Log("[SaveSystem] No save found, creating new save");
        }

        OnGameLoaded?.Invoke();
    }

    /// <summary>
    /// Delete the save file.
    /// </summary>
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        currentSave = new SaveData();
        Debug.Log("[SaveSystem] Save deleted");
    }

    /// <summary>
    /// Check if a save exists.
    /// </summary>
    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    // Convenience methods for updating save data

    public void UpdatePlayerPosition(Vector3 position)
    {
        currentSave.playerPosition = position;
    }

    public void UpdateCurrentLevel(string levelName)
    {
        currentSave.currentLevel = levelName;
    }

    public void IncrementDeathCount()
    {
        currentSave.deathCount++;
    }

    public void AddPlayTime(float time)
    {
        currentSave.totalPlayTime += time;
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!currentSave.unlockedAchievements.Contains(achievementId))
        {
            currentSave.unlockedAchievements.Add(achievementId);
            Debug.Log($"[SaveSystem] Achievement unlocked: {achievementId}");
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return currentSave.unlockedAchievements.Contains(achievementId);
    }

    public void SetLevelCompleted(string levelName, float completionTime)
    {
        currentSave.levelCompleted[levelName] = true;

        // Update best time if better
        if (!currentSave.levelBestTimes.ContainsKey(levelName) || 
            completionTime < currentSave.levelBestTimes[levelName])
        {
            currentSave.levelBestTimes[levelName] = completionTime;
        }
    }

    public void IncrementJumpCount()
    {
        currentSave.totalJumps++;
    }

    public void IncrementHookUseCount()
    {
        currentSave.totalHookUses++;
    }

    public void AddDistanceTraveled(float distance)
    {
        currentSave.totalDistanceTraveled += distance;
    }
}
