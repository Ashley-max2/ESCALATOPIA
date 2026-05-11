using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [TextArea]
    public string objectiveText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MissionManager.Instance.SetObjective(objectiveText);
        }
    }
}