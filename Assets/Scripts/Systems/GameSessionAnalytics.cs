using System;
using System.Collections.Generic;

[Serializable]
public class GameSessionAnalytics {
    public string sessionId;
    public string timestamp;
    public string playerName;
    public int timeTotalSeconds;
    public float maxHeightReached;
    public int checkpointsReached;
    public float averageTimePerPuzzleSeconds;
    public int totalDeaths;
    public Dictionary<string,int> bossAttempts;
    public Dictionary<string,float> bossClearTimeSeconds;
    public int hookUsesCount;
    public int puzzlesCompleted;
    public int puzzlesTotal;
    public int itemsCollected;
    public List<string> itemIds;
    public Dictionary<string,int> movementStats;
    public string maxLevelReached;
    public bool hasCompletedGame;
    public string sessionOutcome;
    public string notes;
}
