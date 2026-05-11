using UnityEngine;
using UnityEngine.Playables;

public class TimelineMissionSimple : MonoBehaviour
{
    [SerializeField] private PlayableDirector timeline;

    [TextArea]
    public string objectiveText;

    [SerializeField] private GameObject[] objectsToDestroy;

    private bool triggered;

    private void OnEnable()
    {
        if (timeline != null)
            timeline.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        if (timeline != null)
            timeline.stopped -= OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        if (triggered) return;

        triggered = true;

        MissionManager.Instance.SetObjective(objectiveText);

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }
    }
}