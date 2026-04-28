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

    private void OnTriggerEnter(Collider other)
    {
        // Solo reaccionar si colisiona el jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("Saliendo del nivel");

            if (director != null)
            {
                // Nos aseguramos de no duplicar suscripciones
                director.stopped -= OnTimelineFinished;
                director.stopped += OnTimelineFinished;

                // Reproducir la timeline
                director.Play();
            }
        }
    }

    private void OnTimelineFinished(PlayableDirector pd)
    {
        Debug.Log("Timeline terminada, cargando escena...");
        LoadingManager.LoadLevel(sceneToLoad);
    }
}