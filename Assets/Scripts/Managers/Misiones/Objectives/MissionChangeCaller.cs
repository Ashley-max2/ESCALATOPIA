using UnityEngine;

public class MissionChangeManager : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string newMission;

    // Esto lo llamas desde Inspector (Button, Timeline, Trigger, etc.)
    public void ChangeMission()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetObjective(newMission);
        }
    }
}