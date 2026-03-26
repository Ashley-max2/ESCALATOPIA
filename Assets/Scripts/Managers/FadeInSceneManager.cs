using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class FadeInSceneManager : MonoBehaviour
{
    [Header("Referencia al Playable Director")]
    public PlayableDirector director;

    [Header("Nombre de la escena a cargar")]
    public string sceneToLoad;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Saliendo del nivel");
        director.Play();
        if (director != null)
        {
            director.stopped -= OnTimelineFinished;
            Debug.Log("Escena cargando");
        }
        
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        SceneManager.LoadScene(sceneToLoad);
        
    }

}
