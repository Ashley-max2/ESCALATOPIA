using UnityEngine;

public class DestroyMissionObjective : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string nextObjective;

    [Header("Objects to destroy")]
    [SerializeField] private GameObject[] objectsToDestroy;

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetObjective(nextObjective);
        }

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}