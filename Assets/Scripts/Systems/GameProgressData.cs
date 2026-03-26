using System;

[Serializable]
public class GameProgressData
{
    public string Scene;
    public string UltimaMisionCompletada;
    public string MisionActual;
    public bool BossCompleted;
    public bool PuzzleCompleted;

    public static GameProgressData CreateDefault()
    {
        return new GameProgressData
        {
            Scene = string.Empty,
            UltimaMisionCompletada = string.Empty,
            MisionActual = string.Empty,
            BossCompleted = false,
            PuzzleCompleted = false
        };
    }
}
