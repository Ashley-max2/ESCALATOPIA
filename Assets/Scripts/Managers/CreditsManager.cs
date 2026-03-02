using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [Header("Referencia al Playable Director")]
    public PlayableDirector director;

    [Header("Nombre de la escena a cargar")]
    public string sceneToLoad;

    private void OnEnable()
    {
        if (director != null)
            director.stopped += OnTimelineFinished;
    }

    private void OnDisable()
    {
        if (director != null)
            director.stopped -= OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}