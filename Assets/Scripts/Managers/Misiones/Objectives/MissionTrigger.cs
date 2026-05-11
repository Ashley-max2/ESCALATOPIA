using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [TextArea]
    public string objectiveText;

    [Header("Objects to destroy")]
    [SerializeField] private GameObject[] objectsToDestroy;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Cambiar objetivo
        MissionManager.Instance.SetObjective(objectiveText);

        // Destruir otros objetos
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}