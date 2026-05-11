using UnityEngine;
using UnityEngine.Playables;

public class TimelineMission : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector timeline;

    [Header("Mission")]
    [TextArea]
    [SerializeField] private string objectiveText;

    [Header("Objects to destroy")]
    [SerializeField] private GameObject[] objectsToDestroy;

    private bool triggered = false;

    private void OnEnable()
    {
        if (timeline != null)
        {
            timeline.stopped += OnTimelineFinished;
        }
    }

    private void OnDisable()
    {
        if (timeline != null)
        {
            timeline.stopped -= OnTimelineFinished;
        }
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        if (triggered) return;

        triggered = true;

        // Cambiar misión
        MissionManager.Instance.SetObjective(objectiveText);

        // Destruir objetos
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}