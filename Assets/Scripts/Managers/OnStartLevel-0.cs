using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [TextArea]
    public string objectiveText;

    void start()
    {
        MissionManager.Instance.SetObjective(objectiveText);
    }
}