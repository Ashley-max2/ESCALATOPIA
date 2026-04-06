using System;

[Serializable]
public class GameProgressData
{
    public string Scene;
    public string UltimaMisionCompletada;
    public string MisionActual;
    public bool BossCompleted;
    public bool PuzzleCompleted;
    public bool HasSpawnPosition;
    public float SpawnX;
    public float SpawnY;
    public float SpawnZ;

    public void SetSpawnPosition(UnityEngine.Vector3 spawnPosition)
    {
        HasSpawnPosition = true;
        SpawnX = spawnPosition.x;
        SpawnY = spawnPosition.y;
        SpawnZ = spawnPosition.z;
    }

    public UnityEngine.Vector3 GetSpawnPosition()
    {
        return new UnityEngine.Vector3(SpawnX, SpawnY, SpawnZ);
    }

    public static GameProgressData CreateDefault()
    {
        return new GameProgressData
        {
            Scene = string.Empty,
            UltimaMisionCompletada = string.Empty,
            MisionActual = string.Empty,
            BossCompleted = false,
            PuzzleCompleted = false,
            HasSpawnPosition = false,
            SpawnX = 0f,
            SpawnY = 0f,
            SpawnZ = 0f
        };
    }
}
