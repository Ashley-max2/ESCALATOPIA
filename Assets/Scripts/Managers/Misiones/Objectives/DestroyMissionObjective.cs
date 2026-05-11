using UnityEngine;

public class DestroyMissionObjective : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string nextObjective;

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetObjective(nextObjective);
        }
    }
}